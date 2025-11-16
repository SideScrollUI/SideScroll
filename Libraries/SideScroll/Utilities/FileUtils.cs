using System.Runtime.InteropServices;

namespace SideScroll.Utilities;

/// <summary>
/// Represents a file path
/// </summary>
public readonly struct FilePath(string path)
{
	/// <summary>
	/// Gets the file path string
	/// </summary>
	public string Path => path;

	public override string ToString() => path;
}

/// <summary>
/// Provides utilities for file operations and file system permissions
/// </summary>
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
	/// <summary>
	/// Gets a timestamp string in the format yyyy-MM-dd_HH-mm-ss
	/// </summary>
	public static string TimestampString => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

	/// <summary>
	/// Gets or sets the set of file extensions that are considered text files
	/// </summary>
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

	/// <summary>
	/// Sets the umask to allow only user permissions on Unix-like systems
	/// </summary>
	/// <returns>The previous umask value, or 0 if not on a Unix-like system</returns>
	public static int SetUmaskUserOnly()
	{
		if (!CanSetPermissions())
			return 0;

		return umask(UmaskUserOnlyPermissions);
	}

	/// <summary>
	/// Recursively copies a directory and its contents to a new location
	/// </summary>
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

	/// <summary>
	/// Determines whether a file is currently open by attempting to open it exclusively
	/// </summary>
	/// <returns>True if the file is open; otherwise, false</returns>
	public static bool IsFileOpen(string fileName)
	{
		var fileInfo = new FileInfo(fileName);

		try
		{
			using FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			stream.Close();
			return false;
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
	}

	/// <summary>
	/// Determines whether a file is a text file based on its extension or content analysis
	/// </summary>
	/// <returns>True if the file is a text file; otherwise, false</returns>
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
			return false;
		}
	}

	/// <summary>
	/// Determines whether a stream contains text content
	/// </summary>
	/// <returns>True if the stream contains text; otherwise, false</returns>
	public static bool IsTextStream(Stream stream)
	{
		try
		{
			using var streamReader = new StreamReader(stream);
			return IsTextStream(streamReader);
		}
		catch (Exception)
		{
			return false;
		}
	}

	/// <summary>
	/// Determines whether a stream reader contains text content by analyzing its characters
	/// </summary>
	/// <returns>True if the stream contains text; otherwise, false</returns>
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
			return false;
		}
	}

	/// <summary>
	/// Deletes a directory and all its contents if it exists
	/// </summary>
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
