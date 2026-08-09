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
	public string Path => path ?? string.Empty;

	/// <summary>Returns the file path string.</summary>
	public override string ToString() => path ?? string.Empty;
}

/// <summary>
/// Provides utilities for file operations and file system permissions
/// </summary>
public static class FileUtils
{
	/// <summary>
	/// Number of characters to read when checking if a file or stream is text
	/// </summary>
	public const int TextCheckBufferSize = 1024;

	/// <summary>
	/// Unix permission bit: User read permission
	/// </summary>
	public const int S_IRUSR = 0x100;

	/// <summary>
	/// Unix permission bit: User write permission
	/// </summary>
	public const int S_IWUSR = 0x80;

	/// <summary>
	/// Unix permission bit: User execute permission
	/// </summary>
	public const int S_IXUSR = 0x40;

	/// <summary>
	/// Unix permission bit: Group read permission
	/// </summary>
	public const int S_IRGRP = 0x20;

	/// <summary>
	/// Unix permission bit: Group write permission
	/// </summary>
	public const int S_IWGRP = 0x10;

	/// <summary>
	/// Unix permission bit: Group execute permission
	/// </summary>
	public const int S_IXGRP = 0x8;

	/// <summary>
	/// Unix permission bit: Other read permission
	/// </summary>
	public const int S_IROTH = 0x4;

	/// <summary>
	/// Unix permission bit: Other write permission
	/// </summary>
	public const int S_IWOTH = 0x2;

	/// <summary>
	/// Unix permission bit: Other execute permission
	/// </summary>
	public const int S_IXOTH = 0x1;

	/// <summary>
	/// Umask value that disallows setting group and other permissions, allowing only user permissions
	/// </summary>
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

			// The destination is created below before the source subdirectories are enumerated, so a
			// destination inside the source would be found by GetDirectories() and copied into
			// itself, nesting until the path length limit stops it
			if (copySubDirs && IsSameOrInside(sourceDirPath, destDirPath))
			{
				throw new ArgumentException(
					$"Destination directory can't be inside the source directory: {destDirPath}",
					nameof(destDirPath));
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
	/// Returns whether <paramref name="path"/> is <paramref name="basePath"/> itself or sits inside it
	/// </summary>
	private static bool IsSameOrInside(string basePath, string path)
	{
		string relative = Path.GetRelativePath(Path.GetFullPath(basePath), Path.GetFullPath(path));

		// GetRelativePath() returns a rooted path when there's no shared root (a different drive)
		return !Path.IsPathRooted(relative) &&
			relative != ".." &&
			!relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
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
		if (TextExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
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
			long originalPosition = 0;
			if (stream.CanSeek)
			{
				originalPosition = stream.Position;
			}

			using var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8, true, TextCheckBufferSize, leaveOpen: true);
			bool result = IsTextStream(streamReader);

			if (stream.CanSeek)
			{
				stream.Position = originalPosition;
			}

			return result;
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
		Stream stream = streamReader.BaseStream;
		if (!stream.CanSeek)
			return false;

		long originalPosition = stream.Position;
		try
		{
			var buffer = new char[TextCheckBufferSize]; // 100 won't detect pdf's as binary
			int charsRead = streamReader.Read(buffer, 0, buffer.Length);
			Array.Resize(ref buffer, charsRead);
			return !buffer.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			streamReader.DiscardBufferedData();
			stream.Position = originalPosition;
		}
	}

	/// <summary>
	/// Why a path was rejected for recursive deletion, or <see cref="Allowed"/> when it wasn't
	/// </summary>
	internal enum DeletePathRejection
	{
		/// <summary>The path can be deleted</summary>
		Allowed,

		/// <summary>Null, empty, or whitespace</summary>
		Blank,

		/// <summary>The path couldn't be resolved to a full path</summary>
		Unresolvable,

		/// <summary>Rooted against the current directory or drive rather than named outright</summary>
		NotFullyQualified,

		/// <summary>A drive, share, or filesystem root</summary>
		FilesystemRoot,
	}

	/// <summary>
	/// Decides whether a path may be recursively deleted, without touching the filesystem
	/// </summary>
	/// <remarks>
	/// Separated from <see cref="DeleteDirectory"/> so the rules can be tested without invoking a
	/// recursive delete against a real root — a test that did would become the failure it guards
	/// against if this check ever regressed
	/// </remarks>
	internal static DeletePathRejection ValidateDeletePath(string? path, out string? fullPath)
	{
		fullPath = null;

		if (string.IsNullOrWhiteSpace(path))
			return DeletePathRejection.Blank;

		// Resolved against the current directory otherwise, so "C:" is the working directory and a
		// leading "/" rebases onto the current drive. Both read as absolute and neither is
		if (!Path.IsPathFullyQualified(path))
			return DeletePathRejection.NotFullyQualified;

		try
		{
			fullPath = Path.GetFullPath(path);
		}
		catch (Exception)
		{
			return DeletePathRejection.Unresolvable;
		}

		// TrimEndingDirectorySeparator leaves a root alone and trims anything below it, so a root
		// only ever equals its own root
		string? rootPath = Path.GetPathRoot(fullPath);
		if (rootPath != null && Path.TrimEndingDirectorySeparator(fullPath).Equals(
			Path.TrimEndingDirectorySeparator(rootPath),
			OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
		{
			return DeletePathRejection.FilesystemRoot;
		}

		return DeletePathRejection.Allowed;
	}

	/// <summary>
	/// Deletes a directory and all its contents if it exists
	/// </summary>
	/// <remarks>
	/// Repository paths are publicly settable and deserialized, so a damaged or crafted value
	/// reaches this. Roots and paths that aren't fully qualified are refused; note that an
	/// otherwise valid directory outside SideScroll is still deleted
	/// </remarks>
	public static void DeleteDirectory(Call? call, string? path)
	{
		call ??= new();

		DeletePathRejection rejection = ValidateDeletePath(path, out string? fullPath);
		if (rejection != DeletePathRejection.Allowed)
		{
			if (rejection == DeletePathRejection.Blank)
			{
				call.Log.Add("Path is blank, no directory to delete");
			}
			else
			{
				call.Log.AddWarning("Refusing to delete directory", new Tag("Path", path), new Tag("Reason", rejection));
			}
			return;
		}

		if (!Directory.Exists(fullPath))
		{
			call.Log.Add("No directory found to delete", new Tag("Path", fullPath));
			return;
		}

		try
		{
			Directory.Delete(fullPath!, true);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}
}
