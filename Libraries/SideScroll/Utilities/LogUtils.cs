namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for logging exceptions to files
/// </summary>
public static class LogUtils
{
	/// <summary>
	/// Saves an exception to a log file and writes it to the console
	/// </summary>
	/// <param name="directory">Directory the log is written into</param>
	/// <param name="filePrefix">Names the log file, and has to be a name rather than a path</param>
	/// <param name="e">The exception to record</param>
	/// <exception cref="ArgumentException"><paramref name="filePrefix"/> isn't a plain file name</exception>
	public static void Save(string directory, string filePrefix, Exception e)
	{
		ValidateFilePrefix(filePrefix);

		// Exceptions often cascade within the same second. Keep the readable timestamp, but add a
		// unique suffix so a later failure never overwrites the first stack trace.
		string filename = filePrefix + ".Exception." + FileUtils.TimestampString + "." + Guid.NewGuid().ToString("N") + ".log";
		string filePath = Paths.Combine(directory, filename);
		string message = e.ToString();

		Directory.CreateDirectory(directory);
		File.WriteAllText(filePath, message);

		Console.WriteLine("Exception stacktrace written to:");
		Console.WriteLine(filePath);
		Console.WriteLine();
		Console.WriteLine(message);
	}

	/// <summary>
	/// Rejects a prefix that would name a file outside the directory it was given
	/// </summary>
	/// <remarks>
	/// The prefix is concatenated into a file name, so one containing a separator, a parent
	/// segment, or a root writes somewhere the caller never chose — a rooted or drive qualified
	/// one makes <see cref="Path.Combine(string, string)"/> discard the directory entirely
	/// </remarks>
	private static void ValidateFilePrefix(string filePrefix)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);

		if (Path.IsPathRooted(filePrefix) ||
			filePrefix.Contains(Path.DirectorySeparatorChar) ||
			filePrefix.Contains(Path.AltDirectorySeparatorChar) ||
			filePrefix.Contains(Path.VolumeSeparatorChar) ||
			filePrefix.Contains("..", StringComparison.Ordinal) ||
			filePrefix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
		{
			throw new ArgumentException(
				$"Prefix has to be a file name, not a path: {filePrefix}", nameof(filePrefix));
		}
	}
}
