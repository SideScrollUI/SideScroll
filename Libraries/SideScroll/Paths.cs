using System.Text;

namespace SideScroll;

/// <summary>
/// Provides utility methods and properties for cross-platform path operations and common system directories
/// </summary>
public static class Paths
{
	/// <summary>
	/// Combines multiple path segments into a single path with forward slashes, handling null values and trimming leading slashes
	/// </summary>
	/// <example>
	/// <code>
	/// Paths.Combine("root", "folder", "file.txt") → "root/folder/file.txt"
	/// </code>
	/// </example>
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

	/// <summary>
	/// Escapes invalid path and filename characters by replacing them with underscore-hexadecimal-underscore format
	/// </summary>
	/// <example>
	/// <code>
	/// Paths.Escape("file:name") → "file_3a_name"
	/// </code>
	/// </example>
	public static string Escape(string path)
	{
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		char[] invalidPathChars = Path.GetInvalidPathChars();

		StringBuilder encodedUri = new();
		foreach (char c in path)
		{
			if (c != '/' && (invalidPathChars.Contains(c) || invalidFileNameChars.Contains(c)))
			{
				encodedUri.Append('_' + Convert.ToByte(c).ToString("x2") + '_');
			}
			else
			{
				encodedUri.Append(c);
			}
		}
		return encodedUri.ToString();
	}

	/// <summary>
	/// Gets the user's home directory path
	/// <para>Windows: C:\Users\[User]</para>
	/// <para>macOS: /Users/[user]</para>
	/// </summary>
	public static string HomePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

	/// <summary>
	/// Gets the application data directory path for roaming user data
	/// <para>Windows: C:\Users\[User]\AppData\Roaming</para>
	/// <para>macOS: /Users/[user]/Library/Application Support/</para>
	/// </summary>
	public static string AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

	/// <summary>
	/// Gets the local application data directory path
	/// <para>Windows: C:\Users\[User]\AppData\Local</para>
	/// <para>macOS: /Users/[user]/Library/Application Support/</para>
	/// </summary>
	public static string LocalDataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

	/// <summary>
	/// Gets the user's Pictures directory path
	/// <para>Windows: C:\Users\[User]\Pictures</para>
	/// <para>macOS: /Users/[user]/Pictures</para>
	/// </summary>
	public static string PicturesPath => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

	/// <summary>
	/// Gets the user's Downloads directory path
	/// </summary>
	public static string DownloadPath => Combine(HomePath, "Downloads");
}
