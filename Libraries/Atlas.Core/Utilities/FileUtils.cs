using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Atlas.Core;

public struct FilePath
{
	public string Path;

	public FilePath(string path)
	{
		Path = path;
	}
}

public static class FileUtils
{
	public static string TimestampString => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

	[DllImport("libc", SetLastError = true, CharSet = CharSet.Unicode)]
	internal static extern int chmod(string path, int mode);

	[DllImport("libc", SetLastError = true)]
	internal static extern int umask(uint mask);

	// User
	public const int S_IRUSR = 0x100;
	public const int S_IWUSR = 0x80;
	public const int S_IXUSR = 0x40;

	// Group
	public const int S_IRGRP = 0x20;
	public const int S_IWGRP = 0x10;
	public const int S_IXGRP = 0x8;

	// Other
	public const int S_IROTH = 0x4;
	public const int S_IWOTH = 0x2;
	public const int S_IXOTH = 0x1;

	// Disallow setting group and other permissions, only allow user
	private const int UmaskUserOnlyPermissions = S_IRGRP | S_IWGRP | S_IXGRP | S_IROTH | S_IWOTH | S_IXOTH;

	private static bool CanSetPermissions()
	{
		return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || Environment.OSVersion.Platform == PlatformID.Unix;
	}

	public static int SetUmaskUserOnly()
	{
		if (!CanSetPermissions())
			return 0;

		return umask(UmaskUserOnlyPermissions);
	}

	// https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
	public static void DirectoryCopy(Call call, string sourceDirPath, string destDirPath, bool copySubDirs)
	{
		var directoryInfo = new DirectoryInfo(sourceDirPath);

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

	public static bool IsFileOpen(string fileName)
	{
		var fileInfo = new FileInfo(fileName);

		try
		{
			using FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			stream.Close();
		}
		catch (IOException)
		{
			return true;
		}

		return false;
	}

	public static HashSet<string> TextExtensions = new()
	{
		".csv",
		".html",
		".ini",
		".log",
		".md",
		".txt",
	};

	public static bool IsTextFile(string path)
	{
		string extension = Path.GetExtension(path);
		if (TextExtensions.Contains(extension))
			return true;

		try
		{
			using StreamReader streamReader = File.OpenText(path);

			var buffer = new char[1000]; // 100 won't detect pdf's as binary
			streamReader.Read(buffer, 0, buffer.Length);
			return !buffer.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n');
		}
		catch (Exception)
		{
		}

		return false;
	}
}
