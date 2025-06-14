namespace SideScroll;

public static class Paths
{
	// Windows can't combine Linux paths correctly, which are needed for FTP (still true?)
	public static string Combine(string? path, params string?[] paths)
	{
		path ??= "";
		foreach (string? part in paths)
		{
			string name = part ?? "(null)";
			path = Path.Combine(path, name.TrimStart('/'));
		}
		return path.Replace('\\', '/');
	}

	public static string Escape(string path)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		char[] invalidPathChars = Path.GetInvalidPathChars();

		string encodedUri = "";
		foreach (char c in path)
		{
			if (c != '/' && (invalidPathChars.Contains(c) || invalidFileNameChars.Contains(c)))
			{
				encodedUri += "_" + Convert.ToByte(c).ToString("x2") + "_";
			}
			else
			{
				encodedUri += c;
			}
		}
		return encodedUri;
	}

	// Windows: ApplicationData -> Users/<User>/AppData/Roaming
	// macOS: /home/<user>/Library/Application Support/ (same as Local, no official support for Remote?)
	public static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

	// Windows: Users/<User>/AppData/Local
	// macOS: /home/<user>/Library/Application Support/
	public static string LocalDataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

	public static string DownloadPath => Combine(HomePath, "Downloads");

	public static string PicturesPath => Combine(HomePath, "Pictures");

	public static string HomePath
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return Environment.GetEnvironmentVariable("HOME")!;
			}
			else
			{
				return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			}
		}
	}
}
