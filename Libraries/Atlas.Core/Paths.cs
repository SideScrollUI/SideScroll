using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Atlas.Core
{
	public class Paths
	{
		public static string Combine(string path1, string path2)
		{
			return Path.Combine(path1, path2.TrimStart('/')).Replace('\\', '/');
		}

		public static string Combine(string path1, string path2, string path3)
		{
			return Path.Combine(path1, path2.TrimStart('/'), path3.TrimStart('/')).Replace('\\', '/');
		}

		public static string Combine(string path1, string path2, string path3, string path4)
		{
			return Path.Combine(path1, path2.TrimStart('/'), path3.TrimStart('/'), path4.TrimStart('/')).Replace('\\', '/');
		}

		public static string Combine(string path1, string path2, string path3, string path4, string path5)
		{
			return Path.Combine(path1, path2.TrimStart('/'), path3.TrimStart('/'), path4.TrimStart('/'), path5.TrimStart('/')).Replace('\\', '/');
		}

		public static string Escape(string path)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			char[] invalidPathChars = Path.GetInvalidPathChars();

			string encodedUri = "";
			foreach (char c in path)
			{
				if (c != '/' && (invalidPathChars.Contains(c) == true || invalidFileNameChars.Contains(c) == true))
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

		public static string AppDataPath
		{
			get
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library");
				else
					return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}
		}

		public static string DownloadPath => Combine(HomePath, "Downloads");

		public static string HomePath
		{
			get
			{
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					return Environment.GetEnvironmentVariable("HOME");
				else
					return Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			}
		}
	}
}
/*
Windows can't combine linux paths correctly, which are needed for FTP
*/