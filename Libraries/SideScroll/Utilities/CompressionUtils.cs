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

		if (fileToCompress.Extension == ".gz")
			return;

		using CallTimer compressCall = call.Timer("Compressing", new Tag("File", fileToCompress.FullName));

		long compressedSize;
		using (FileStream originalFileStream = fileToCompress.OpenRead())
		using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
		{
			// Dispose before reading the size, GZipStream doesn't write the remaining blocks until then
			// leaveOpen so disposing it doesn't also close the file we're about to measure
			using (GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress, leaveOpen: true))
			{
				originalFileStream.CopyTo(compressionStream);
			}

			compressedSize = compressedFileStream.Length;
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

		if (fileToDecompress.Extension == ".zip")
		{
			ExtractZip(fileToDecompress);
		}
		else if (fileToDecompress.Extension == ".gz")
		{
			ExtractGzip(decompressCall, fileToDecompress);
		}
	}

	private static void ExtractZip(FileInfo fileToDecompress)
	{
		string targetPath = Path.ChangeExtension(fileToDecompress.FullName, null);

		if (Directory.Exists(targetPath))
		{
			Directory.Delete(targetPath, true);
		}

		ZipFile.ExtractToDirectory(fileToDecompress.FullName, targetPath);
	}

	private static void ExtractGzip(CallTimer decompressCall, FileInfo fileToDecompress)
	{
		string currentFileName = fileToDecompress.FullName;
		string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

		long decompressedSize;
		using (FileStream originalFileStream = fileToDecompress.OpenRead())
		using (FileStream decompressedFileStream = File.Create(newFileName))
		{
			using (GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress, leaveOpen: true))
			{
				decompressionStream.CopyTo(decompressedFileStream);
			}

			decompressedFileStream.Flush();
			decompressedSize = decompressedFileStream.Length;
		}

		decompressCall.Log.Add("Finished Decompressing",
			new Tag("File", fileToDecompress.Name),
			new Tag("Compressed Size", fileToDecompress.Length),
			new Tag("Decompressed Size", decompressedSize)
			);
	}
}
