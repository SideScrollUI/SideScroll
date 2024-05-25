using Atlas.Extensions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Atlas.Core.Utilities;

public record DotnetRuntimeInfo(string Name, Version Version, string Path);

public static class ProcessUtils
{
	public static string OSPlatformName => GetOSPlatform().ToString().CamelCased();

	public static OSPlatform GetOSPlatform()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return OSPlatform.Windows;
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return OSPlatform.Linux;
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return OSPlatform.OSX;
		}

		return OSPlatform.Create("Unknown");
	}

	public static void OpenBrowser(string url)
	{
		if (url == null)
			return;

		// not working
		//if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
		//	throw new Exception("Invalid url: " + url);

		try
		{
			Process.Start(url);
		}
		catch
		{
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			// Can fix after updating to .Net Standard 2.1
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process.Start("xdg-open", url);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Process.Start("open", url);
			}
			else
			{
				throw;
			}
		}
	}

	public static void OpenFolder(string folder, string? selection = null)
	{
		// Select file instead if in folder path
		// Trying to open a file will use the default app to open it
		if (selection == null && 
			Path.GetDirectoryName(folder) is string directoryName &&
			Path.GetFileName(folder) is string fileName &&
			!File.GetAttributes(folder).HasFlag(FileAttributes.Directory))
		{
			folder = directoryName;
			selection = fileName;
		}

		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				folder = folder.Replace('/', '\\');

				string argument = '"' + folder + '"';
				if (selection != null)
				{
					// Ignore bad selections
					string fullPath = Path.Combine(folder, selection);
					if (File.Exists(fullPath))
					{
						argument = "/select,\"" + fullPath + "\"";
					}
				}

				Process.Start("explorer.exe", argument);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Process.Start("open", folder);
			}
		}
		catch
		{
		}
	}

	public static string GetDotnetFileName()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return "/usr/local/share/dotnet/dotnet";
		}
		else if (Environment.OSVersion.Platform == PlatformID.Unix)
		{
			return "dotnet";
		}
		else
		{
			return "dotnet.exe";
		}
	}

	public static Process StartDotnetProcess(string arguments)
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = GetDotnetFileName(),
			Arguments = arguments,
			WorkingDirectory = Directory.GetCurrentDirectory(),
		};

		// Windows options
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
			Environment.OSVersion.Platform != PlatformID.Unix)
		{
			processStartInfo.CreateNoWindow = true;
			//processStartInfo.UseShellExecute = true, // doesn't work on mac yet, last checked for dotnet 3.1
		}

		Process process = Process.Start(processStartInfo)!;
		return process;
	}

	public static List<DotnetRuntimeInfo> GetDotnetRuntimes()
	{
		var processStartInfo = new ProcessStartInfo
		{
			FileName = GetDotnetFileName(),
			Arguments = "--list-runtimes",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true,
		};

		List<DotnetRuntimeInfo> runtimes = [];
		Process process = Process.Start(processStartInfo)!;
		while (!process.StandardOutput.EndOfStream)
		{
			string? line = process.StandardOutput.ReadLine();
			if (line == null) continue;

			string[] parts = line.Split(' ', 3);
			var runtime = new DotnetRuntimeInfo(parts[0], Version.Parse(parts[1]), parts[2]);
			runtimes.Add(runtime);
		}
		return runtimes;
	}
}
