using System;
using System.IO;

namespace Atlas.Core
{
	public struct FilePath
	{
		public string Path;

		public FilePath(string path)
		{
			Path = path;
		}
	}

	public class FileUtils
	{
		public static void DirectoryCopy(Call call, string sourceDirPath, string destDirPath, bool copySubDirs)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirPath);

			// too much nesting
			//using (CallTimer callTimer = call.Timer("Copying", new Tag("Directory", directoryInfo.Name)))
			{
				if (!directoryInfo.Exists)
				{
					throw new DirectoryNotFoundException(
						"Source directory does not exist or could not be found: "
						+ sourceDirPath);
				}

				// Create destination directory
				if (!Directory.Exists(destDirPath))
				{
					Directory.CreateDirectory(destDirPath);
				}

				// Copy files
				FileInfo[] fileInfos = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in fileInfos)
				{
					string destFilePath = Path.Combine(destDirPath, fileInfo.Name);
					call.Log.Add("Copying", new Tag("File", fileInfo.Name));
					fileInfo.CopyTo(destFilePath, true);
				}

				// Copy subdirectories
				if (copySubDirs)
				{
					DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
					foreach (DirectoryInfo subDirInfo in subDirectories)
					{
						string destSubPath = Path.Combine(destDirPath, subDirInfo.Name);
						DirectoryCopy(call, subDirInfo.FullName, destSubPath, copySubDirs);
					}
				}
			}
		}
	}
}
/*
Based on:
https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
*/
