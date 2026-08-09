using System.IO.Compression;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for compressing and decompressing files
/// </summary>
public class CompressionUtils
{
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
					decompressionStream.CopyTo(decompressedFileStream);
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
