using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Atlas.Core
{
	public class ProcessUtils
	{
		public static void OpenBrowser(string url)
		{
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

		public static void OpenFolder(string path)
		{
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					path = path.Replace('/', '\\');
					string argument;
					if (File.Exists(path))
						argument = "/select,\"" + path + "\"";
					else
						argument = '"' + Path.GetDirectoryName(path) + '"';

					Process.Start("explorer.exe", argument);
				}
			}
			catch
			{
			}
		}

		public static void StartDotnetProcess(string arguments)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo()
				{
					//FileName = "dotnet",
					FileName = "/usr/local/share/dotnet/dotnet",
					Arguments = arguments,
				};
				// Required for Mac .apps (doesn't work)
				// processStartInfo.Environment.Add("PATH", Environment.GetEnvironmentVariable("PATH"));
				
				Process process = Process.Start(processStartInfo);
			}
			else
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo()
				{
					FileName = "dotnet.exe",
					Arguments = arguments,
					WorkingDirectory = Directory.GetCurrentDirectory(),
					CreateNoWindow = true,
					//UseShellExecute = true, // doesn't work on mac yet, last checked for dotnet 3.1
				};
				Process process = Process.Start(processStartInfo);
			}
		}
	}
}
