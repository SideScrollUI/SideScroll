using System;
using System.IO;

namespace Atlas.Core
{
	public class LogUtils
	{
		public static void LogException(Exception e, string domain, string projectName)
		{
			string filename = projectName + ".Exception." + FileUtils.TimestampString + ".log";
			string directory = Paths.Combine(Paths.AppDataPath, domain, projectName, "Exceptions");
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
}
