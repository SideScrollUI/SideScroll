using SideScroll.Extensions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SideScroll.Utilities;

/// <summary>
/// Represents information about a .NET runtime installation
/// </summary>
public record DotnetRuntimeInfo(string Name, Version Version, string Path);

/// <summary>
/// Provides utilities for process operations and platform detection
/// </summary>
public static class ProcessUtils
{
	/// <summary>
	/// Gets the current operating system platform name in camel case format
	/// </summary>
	public static string OSPlatformName => GetOSPlatform().ToString().CamelCased();

	/// <summary>
	/// Detects and returns the current operating system platform
	/// </summary>
	/// <returns>The current OSPlatform (Windows, Linux, OSX, or Unknown)</returns>
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

	/// <summary>
	/// Opens a URL in the default web browser
	/// </summary>
	public static void OpenBrowser(string url)
	{
		if (url == null)
			return;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			// Workaround because of this: https://github.com/dotnet/corefx/issues/10361
			// Can fix after updating to .Net Standard 2.1? Unclear
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
			throw new Exception("Unknown platform");
		}
	}

	/// <summary>
	/// Opens a folder in the system's file explorer, optionally selecting a specific file
	/// </summary>
	public static void OpenFolder(string folder, string? selection = null)
	{
		// Select file instead if in folder path
		// Trying to open a file will use the default app to open it
		if (Path.GetDirectoryName(folder) is string directoryName &&
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

	/// <summary>
	/// Gets the file name or path for the dotnet executable on the current platform
	/// </summary>
	/// <returns>The dotnet executable path or name</returns>
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

	/// <summary>
	/// Starts a new dotnet process with the specified arguments
	/// </summary>
	/// <returns>The started Process instance</returns>
	public static Process StartDotnetProcess(string arguments)
	{
		ProcessStartInfo processStartInfo = new()
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

	/// <summary>
	/// Gets a list of all installed .NET runtimes on the system
	/// </summary>
	/// <returns>A list of DotnetRuntimeInfo objects representing installed runtimes</returns>
	public static List<DotnetRuntimeInfo> GetDotnetRuntimes()
	{
		ProcessStartInfo processStartInfo = new()
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
