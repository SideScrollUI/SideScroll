using System.Runtime.InteropServices;

namespace SideScroll.Utilities;

public struct FilePath(string path)
{
	public readonly string Path => path;
}

public static class FileUtils
{
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
	public const int UmaskUserOnlyPermissions = S_IRGRP | S_IWGRP | S_IXGRP | S_IROTH | S_IWOTH | S_IXOTH;
	public static string TimestampString => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

	public static HashSet<string> TextExtensions { get; set; } =
	[
		".csv",
		".html",
		".ini",
		".log",
		".md",
		".txt",
	];

	[DllImport("libc", SetLastError = true, CharSet = CharSet.Unicode)]
	internal static extern int chmod(string path, int mode);

	[DllImport("libc", SetLastError = true)]
	internal static extern int umask(uint mask);

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
		catch (DirectoryNotFoundException)
		{
			return false;
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		catch (IOException)
		{
			return true;
		}

		return false;
	}

	public static bool IsTextFile(string path)
	{
		string extension = Path.GetExtension(path);
		if (TextExtensions.Contains(extension))
			return true;

		try
		{
			using StreamReader streamReader = File.OpenText(path);
			return IsTextStream(streamReader);
		}
		catch (Exception)
		{
		}

		return false;
	}

	public static bool IsTextStream(Stream stream)
	{
		try
		{
			using var streamReader = new StreamReader(stream);
			return IsTextStream(streamReader);
		}
		catch (Exception)
		{
		}

		return false;
	}

	public static bool IsTextStream(StreamReader streamReader)
	{
		try
		{
			var buffer = new char[1000]; // 100 won't detect pdf's as binary
			int bytesRead = streamReader.Read(buffer, 0, buffer.Length);
			Array.Resize(ref buffer, bytesRead);
			return !buffer.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
		}
		catch (Exception)
		{
		}

		return false;
	}

	public static void DeleteDirectory(Call? call, string? path)
	{
		call ??= new();

		if (path == null)
		{
			call.Log.Add("Path is blank, no directory to delete");
			return;
		}

		if (!Directory.Exists(path))
		{
			call.Log.Add("No directory found to delete", new Tag("Path", path));
			return;
		}

		try
		{
			Directory.Delete(path, true);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}
}
