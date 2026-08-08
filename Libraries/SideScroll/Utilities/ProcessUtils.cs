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
	/// Determines whether the current operating system is Windows 10 or earlier (excludes Windows 11 and later)
	/// </summary>
	/// <returns>True if running on Windows 10 or below; false for Windows 11+ or non-Windows platforms</returns>
	public static bool IsWindows10OrBelow()
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return false;

		// Windows 11 is version 10.0 with build >= 22000
		// Windows 10 is version 10.0 with build < 22000
		// Windows 8.1 is version 6.3
		// Windows 8 is version 6.2
		// Windows 7 is version 6.1

		var version = Environment.OSVersion.Version;

		if (version.Major < 10)
			return true; // Windows 8.1 or below

		if (version.Major == 10 && version.Build < 22000)
			return true; // Windows 10

		return false; // Windows 11 or above
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
			// UseShellExecute opens the default browser directly. It used to go through
			// "cmd /c start {url}", which pasted the url into a shell command line where only '&'
			// was escaped, so a url containing '|', '>', or '^' ran whatever followed it
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
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
		// There's nothing to show, and explorer.exe opens a default folder instead of doing nothing
		if (!File.Exists(folder) && !Directory.Exists(folder))
			return;

		try
		{
			// Select file instead if in folder path
			// Trying to open a file will use the default app to open it
			// GetAttributes() throws when the path is removed after the checks above, so it belongs
			// inside the same catch as every other failure here
			if (Path.GetDirectoryName(folder) is { } directoryName &&
				Path.GetFileName(folder) is { } fileName &&
				!File.GetAttributes(folder).HasFlag(FileAttributes.Directory))
			{
				folder = directoryName;
				selection = fileName;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				folder = folder.Replace('/', '\\');

				string argument = '"' + folder + '"';
				if (selection != null && !Path.IsPathRooted(selection))
				{
					// Ignore bad selections. Path.Combine() discards the folder when the selection is
					// rooted, which would select a file somewhere else entirely
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
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process.Start("xdg-open", folder);
			}
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
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
	/// Starts a new dotnet process with a pre-built command line
	/// </summary>
	/// <remarks>
	/// Prefer the <see cref="StartDotnetProcess(IReadOnlyList{string})"/> overload. A hand built
	/// command line has to quote every value itself, and an unquoted path under a directory like
	/// "C:\Users\First Last" splits into two arguments before the child process sees it
	/// </remarks>
	/// <returns>The started Process instance, which the caller owns and has to dispose</returns>
	public static Process StartDotnetProcess(string arguments)
	{
		ProcessStartInfo processStartInfo = CreateDotnetProcessStartInfo();
		processStartInfo.Arguments = arguments;

		return Process.Start(processStartInfo)!;
	}

	/// <summary>
	/// Starts a new dotnet process, passing each argument separately
	/// </summary>
	/// <remarks>
	/// ArgumentList escapes each element, so values containing spaces or quotes reach the child
	/// process intact instead of splitting into extra arguments. A string isn't convertible to
	/// IReadOnlyList&lt;string&gt;, so a single string argument still binds to the other overload
	/// </remarks>
	/// <returns>The started Process instance, which the caller owns and has to dispose</returns>
	public static Process StartDotnetProcess(IReadOnlyList<string> arguments)
	{
		ArgumentNullException.ThrowIfNull(arguments);

		ProcessStartInfo processStartInfo = CreateDotnetProcessStartInfo();
		foreach (string argument in arguments)
		{
			processStartInfo.ArgumentList.Add(argument);
		}

		return Process.Start(processStartInfo)!;
	}

	private static ProcessStartInfo CreateDotnetProcessStartInfo()
	{
		ProcessStartInfo processStartInfo = new()
		{
			FileName = GetDotnetFileName(),
			WorkingDirectory = Directory.GetCurrentDirectory(),
		};

		// Windows options
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			processStartInfo.CreateNoWindow = true;
			//processStartInfo.UseShellExecute = true, // doesn't work on mac yet, last checked for dotnet 3.1
		}

		return processStartInfo;
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
		using Process process = Process.Start(processStartInfo)!;
		while (!process.StandardOutput.EndOfStream)
		{
			string? line = process.StandardOutput.ReadLine();
			if (line == null) continue;

			string[] parts = line.Split(' ', 3);
			if (parts.Length < 3) continue;

			string versionStr = parts[1];
			int dashIndex = versionStr.IndexOf('-');
			if (dashIndex > 0)
			{
				versionStr = versionStr.Substring(0, dashIndex);
			}

			if (Version.TryParse(versionStr, out Version? parsedVersion))
			{
				string path = parts[2];
				if (path.Length >= 2 && path[0] == '[' && path[^1] == ']')
				{
					path = path[1..^1];
				}

				var runtime = new DotnetRuntimeInfo(parts[0], parsedVersion, path);
				runtimes.Add(runtime);
			}
		}
		return runtimes;
	}
}
