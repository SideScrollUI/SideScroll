using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Atlas.Core
{
	public class Compression
	{
		public static void Compress(Call call, FileInfo fileToCompress)
		{
			if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden)
				return;

			if (fileToCompress.Extension == ".gz")
				return;

			using CallTimer compressCall = call.Timer("Compressing", new Tag("File", fileToCompress.FullName));

			using FileStream originalFileStream = fileToCompress.OpenRead();
			
			using FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz");

			using GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress);
				
			originalFileStream.CopyTo(compressionStream);

			compressCall.Log.Add("Finished Compressing",
				new Tag("File", fileToCompress.Name),
				new Tag("Original Size", fileToCompress.Length),
				new Tag("Compressed Size", compressedFileStream.Length)
				);
		}

		public static void Decompress(Call call, FileInfo fileToDecompress)
		{
			using CallTimer decompressCall = call.Timer("Decompressing", new Tag("File", fileToDecompress.FullName));
			
			if (fileToDecompress.Extension == ".zip")
			{
				string targetPath = Path.ChangeExtension(fileToDecompress.FullName, null);

				if (Directory.Exists(targetPath))
					Directory.Delete(targetPath, true);

				ZipFile.ExtractToDirectory(fileToDecompress.FullName, targetPath);
			}
			else if (fileToDecompress.Extension == ".gz")
			{
				using FileStream originalFileStream = fileToDecompress.OpenRead();
				
				string currentFileName = fileToDecompress.FullName;
				string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

				using FileStream decompressedFileStream = File.Create(newFileName);

				using GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress);
				
				decompressionStream.CopyTo(decompressedFileStream);

				decompressCall.Log.Add("Finished Decompressing",
					new Tag("File", fileToDecompress.Name),
					new Tag("Original Size", fileToDecompress.Length),
					new Tag("Compressed Size", decompressedFileStream.Length)
					);
			}
		}
	}
}
