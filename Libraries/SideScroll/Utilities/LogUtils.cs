using System.Diagnostics;

namespace SideScroll.Utilities;

public static class LogUtils
{
	public static void Save(string directory, string filePrefix, Exception e)
	{
		string filename = filePrefix + ".Exception." + FileUtils.TimestampString + ".log";
		string filePath = Paths.Combine(directory, filename);
		string message = e.ToString();

		Directory.CreateDirectory(directory);
		File.WriteAllText(filePath, message);

		Console.WriteLine("Exception stacktrace written to:");
		Console.WriteLine(filePath);
		Console.WriteLine();
		Console.WriteLine(message);

		Debug.Fail(message);
	}
}
