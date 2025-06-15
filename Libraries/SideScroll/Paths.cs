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

	// Windows: C:\Users\<User>
	// macOS: /Users/<user>
	public static string HomePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

	// Windows: ApplicationData -> C:\Users\<User>\AppData\Roaming
	// macOS: /Users/<user>/Library/Application Support/ (same as Local, no official support for Remote?)
	public static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

	// Windows: C:\Users\<User>\AppData\Local
	// macOS: /Users/<user>/Library/Application Support/
	public static string LocalDataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

	// Windows: C:\Users\<User>\Pictures
	// macOS: /Users/<user>/Pictures
	public static string PicturesPath => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

	public static string DownloadPath => Combine(HomePath, "Downloads");
}
