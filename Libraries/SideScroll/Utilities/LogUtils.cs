namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for logging exceptions to files
/// </summary>
public static class LogUtils
{
	/// <summary>
	/// Saves an exception to a log file and writes it to the console
	/// </summary>
	public static void Save(string directory, string filePrefix, Exception e)
	{
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
}
