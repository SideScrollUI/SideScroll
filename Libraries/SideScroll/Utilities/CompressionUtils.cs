using SideScroll.Extensions;
using System.IO.Compression;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for compressing and decompressing files
/// </summary>
public class CompressionUtils
{
	/// <summary>
	/// Gets or sets the maximum number of bytes an archive is allowed to expand to
	/// </summary>
	/// <remarks>
	/// An archive's own size says nothing about how far it expands, so a small one can fill the
	/// disk it's extracted onto. Checked against the sizes a zip declares before anything is
	/// written, and against the bytes read for a gzip, which declares nothing
	/// </remarks>
	public static long MaxExtractedSize
	{
		get => _maxExtractedSize;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(MaxExtractedSize));
			_maxExtractedSize = value;
		}
	}
	private static long _maxExtractedSize = 1_000_000_000;

	/// <summary>
	/// Gets or sets the maximum number of entries an archive is allowed to contain
	/// </summary>
	/// <remarks>
	/// Each entry costs a file and its metadata, so an archive of many tiny entries takes far
	/// longer to extract than its size suggests
	/// </remarks>
	public static int MaxEntries
	{
		get => _maxEntries;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(MaxEntries));
			_maxEntries = value;
		}
	}
	private static int _maxEntries = 100_000;

	/// <summary>
	/// Compresses a file using GZip compression
	/// </summary>
	public static void Compress(Call call, FileInfo fileToCompress)
	{
		if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden)
			return;

		if (fileToCompress.Extension.Equals(".gz", StringComparison.OrdinalIgnoreCase))
			return;

		using CallTimer compressCall = call.Timer("Compressing", new Tag("File", fileToCompress.FullName));

		string compressedPath = fileToCompress.FullName + ".gz";

		// Compress into a temp file and move it into place once it succeeds, so a failure part way
		// through can't destroy an existing archive
		string tempPath = compressedPath + "." + Path.GetRandomFileName();

		long compressedSize;
		bool moved = false;
		try
		{
			using (FileStream originalFileStream = fileToCompress.OpenRead())
			using (FileStream compressedFileStream = File.Create(tempPath))
			{
				// Dispose before reading the size, GZipStream doesn't write the remaining blocks until then
				// leaveOpen so disposing it doesn't also close the file we're about to measure
				using (GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress, leaveOpen: true))
				{
					originalFileStream.CopyTo(compressionStream);
				}

				compressedSize = compressedFileStream.Length;
			}

			File.Move(tempPath, compressedPath, true);
			moved = true;
		}
		finally
		{
			// The move consumed it on success, only a failure leaves one behind
			if (!moved)
			{
				DeleteTempFile(tempPath);
			}
		}

		compressCall.Log.Add("Finished Compressing",
			new Tag("File", fileToCompress.Name),
			new Tag("Original Size", fileToCompress.Length),
			new Tag("Compressed Size", compressedSize)
			);
	}

	/// <summary>
	/// Decompresses a file (supports both .zip and .gz formats)
	/// </summary>
	public static void Decompress(Call call, FileInfo fileToDecompress)
	{
		using CallTimer decompressCall = call.Timer("Decompressing", new Tag("File", fileToDecompress.FullName));

		if (fileToDecompress.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
		{
			ExtractZip(fileToDecompress);
		}
		else if (fileToDecompress.Extension.Equals(".gz", StringComparison.OrdinalIgnoreCase))
		{
			ExtractGzip(decompressCall, fileToDecompress);
		}
		else
		{
			// Anything else fell through having already logged "Decompressing", so a caller couldn't
			// tell a format that was never handled apart from one that extracted successfully
			throw new InvalidDataException(
				$"Can't decompress a {fileToDecompress.Extension} file, expected .zip or .gz: {fileToDecompress.Name}");
		}
	}

	/// <summary>
	/// Rejects an archive that declares more than the limits allow, before any of it is written
	/// </summary>
	/// <remarks>
	/// A zip records each entry's uncompressed length in its own directory, so the total is known
	/// without extracting anything. That's the archive's own claim rather than a measurement, but
	/// an archive that lies about it is rejected by extraction itself
	/// </remarks>
	private static void ValidateZipLimits(FileInfo fileToDecompress)
	{
		using ZipArchive archive = ZipFile.OpenRead(fileToDecompress.FullName);

		if (archive.Entries.Count > MaxEntries)
		{
			throw new InvalidDataException(
				$"Archive has {archive.Entries.Count} entries, more than the {MaxEntries} allowed: {fileToDecompress.Name}");
		}

		long totalSize = 0;
		foreach (ZipArchiveEntry entry in archive.Entries)
		{
			totalSize += entry.Length;
			if (totalSize > MaxExtractedSize)
			{
				throw new InvalidDataException(
					$"Archive expands past the {MaxExtractedSize} bytes allowed: {fileToDecompress.Name}");
			}
		}
	}

	private static void ExtractZip(FileInfo fileToDecompress)
	{
		string targetPath = Path.ChangeExtension(fileToDecompress.FullName, null);

		// Extract into a temp directory and swap it in once it succeeds. Deleting the destination
		// first meant a corrupt or truncated archive destroyed the previous extraction before failing
		string tempPath = targetPath + "." + Path.GetRandomFileName();
		string backupPath = targetPath + "." + Path.GetRandomFileName();

		bool backedUp = false;
		try
		{
			ValidateZipLimits(fileToDecompress);

			ZipFile.ExtractToDirectory(fileToDecompress.FullName, tempPath);

			if (Directory.Exists(targetPath))
			{
				// Moved instead of deleted, a recursive delete can fail after removing part of it
				Directory.Move(targetPath, backupPath);
				backedUp = true;
			}

			try
			{
				Directory.Move(tempPath, targetPath);
			}
			catch (Exception) when (backedUp)
			{
				// Cleared first so a failed restore leaves the backup on disk instead of deleting it
				backedUp = false;
				Directory.Move(backupPath, targetPath);
				throw;
			}
		}
		finally
		{
			DeleteTempDirectory(tempPath);
			if (backedUp)
			{
				DeleteTempDirectory(backupPath);
			}
		}
	}

	private static void ExtractGzip(CallTimer decompressCall, FileInfo fileToDecompress)
	{
		string currentFileName = fileToDecompress.FullName;
		string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

		// Decompress into a temp file and move it into place once it succeeds. Creating the
		// destination first truncated it before decompressing anything, so a corrupt archive
		// destroyed the file it was meant to restore
		string tempPath = newFileName + "." + Path.GetRandomFileName();

		long decompressedSize;
		bool moved = false;
		try
		{
			using (FileStream originalFileStream = fileToDecompress.OpenRead())
			using (FileStream decompressedFileStream = File.Create(tempPath))
			{
				using (GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress, leaveOpen: true))
				{
					// A gzip carries no size to check ahead of time, so the bytes are counted as
					// they're read
					if (!decompressionStream.TryCopyUpTo(decompressedFileStream, MaxExtractedSize))
					{
						throw new InvalidDataException(
							$"Archive expands past the {MaxExtractedSize} bytes allowed: {fileToDecompress.Name}");
					}
				}

				decompressedFileStream.Flush();
				decompressedSize = decompressedFileStream.Length;
			}

			File.Move(tempPath, newFileName, true);
			moved = true;
		}
		finally
		{
			// The move consumed it on success, only a failure leaves one behind
			if (!moved)
			{
				DeleteTempFile(tempPath);
			}
		}

		decompressCall.Log.Add("Finished Decompressing",
			new Tag("File", fileToDecompress.Name),
			new Tag("Compressed Size", fileToDecompress.Length),
			new Tag("Decompressed Size", decompressedSize)
			);
	}

	// A failed compress or extract shouldn't leave its temp copy behind, and the cleanup can't hide
	// the original error
	private static void DeleteTempFile(string tempPath)
	{
		try
		{
			File.Delete(tempPath);
		}
		catch (Exception)
		{
		}
	}

	private static void DeleteTempDirectory(string tempPath)
	{
		try
		{
			if (Directory.Exists(tempPath))
			{
				Directory.Delete(tempPath, true);
			}
		}
		catch (Exception)
		{
		}
	}
}
