using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Atlas.Network
{
	// use System.IO.FileInfo instead? No place to store remote DateTime since it's a method
	public class FtpItem
	{
		public string Filename { get; set; }
		public long Size { get; set; }
		public DateTime Modified { get; set; }

		public bool directory;
		public string fullPath;

		public override string ToString() => fullPath ?? "(null)";
	}

	public class FtpFileInfo
	{
		private const string ftpPath = @"/pub/databases/genenames/new/tsv/hgnc_complete_set.txt";

		public string LocalBasePath { get; set; }
		public string FtpHost { get; set; }
		public string RemotePath { get; set; }
		
		public string LocalPath { get; set; }

		public FtpFileInfo(string localBasePath, string ftpHost, string remotePath)
		{
			LocalBasePath = localBasePath;
			FtpHost = ftpHost;
			RemotePath = remotePath;
			
			LocalPath = Paths.Combine(localBasePath, RemotePath);
		}
	}

	public class FTP
	{
		public class Info
		{
			public string HostIP { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public int BufferSize { get; set; } = 10000;
			public bool FileSizeSupported { get; set; } = true;

			public Info(string hostIP, string username = null, string password = null)
			{
				HostIP = hostIP;
				Username = username;
				Password = password;
			}

			public FtpWebRequest CreateRequest(string path)
			{
				path = path.Replace('\\', '/');
				FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create("ftp://" + HostIP + path);
				if (Username != null)
					ftpRequest.Credentials = new NetworkCredential(Username, Password);
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				return ftpRequest;
			}
		}

		public Log Log;
		public Info info;

		public FTP(Call call, Info info)
		{
			this.Log = call.Log;
			this.info = info;
		}

		public void Download(string remoteFile, string localFile, TaskInstance taskInstance = null, long maxBytes = long.MaxValue)
		{
			string directoryPath = Path.GetDirectoryName(localFile);
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

			using (LogTimer logTimer = Log.Timer("Downloading", new Tag("File", remoteFile)))
			{
				for (int attempt = 0; attempt < 3; attempt++)
				{
					try
					{
						DownloadInternal(logTimer, remoteFile, localFile, taskInstance, maxBytes);
						break;
					}
					catch (Exception e)
					{
						logTimer.AddError("Exception downloading file", new Tag("Exception", e.ToString()));
					}
				}
			}
		}

		private void DownloadInternal(Log log, string remoteFile, string localFile, TaskInstance taskInstance = null, long maxBytes = long.MaxValue)
		{
			FtpWebRequest ftpRequest = info.CreateRequest(remoteFile);
			ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			Stream ftpStream = ftpResponse.GetResponseStream();

			FileStream localFileStream = new FileStream(localFile, FileMode.Create);
			byte[] byteBuffer = new byte[info.BufferSize];
			
			long bytesTransferred = 0;
			while (bytesTransferred < maxBytes)
			{
				long transferred = bytesTransferred;
				long contentLength = ftpResponse.ContentLength;
				if (contentLength <= 0)
				{
					//"150 Opening BINARY mode data connection for pub/CCDS/archive/20/CCDS.20160908.txt (9578571 bytes)\r\n"
					string pattern = @"\((?<Bytes>[0-9]+) bytes\)";
					Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
					Match match = regex.Match(ftpResponse.StatusDescription);
					if (match.Success)
					{
						contentLength = long.Parse(match.Groups["Bytes"].Value);
					}
				}
				double percent = 0;
				if (contentLength > 0)
					percent = (double)transferred / contentLength;

				if (taskInstance != null)
				{
					if (taskInstance.TokenSource.IsCancellationRequested)
						break;
					taskInstance.Percent = (int)(100.0 * percent);
				}

				int bytesRead = ftpStream.Read(byteBuffer, 0, info.BufferSize);
				if (bytesRead <= 0)
					break;
				localFileStream.Write(byteBuffer, 0, bytesRead);
				bytesTransferred += bytesRead;
			}

			if (ftpStream.CanRead)
				ftpRequest.Abort();

			log.Add("Download Finished", 
				new Tag("Bytes Transferred", bytesTransferred), 
				new Tag("Size", ftpResponse.ContentLength));

			localFileStream.Close();
			ftpStream.Close();
			ftpResponse.Close();
		}
		
		public void Upload(string remoteFile, string localFile)
		{
			try
			{
				FtpWebRequest ftpRequest = info.CreateRequest(remoteFile);
				ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
				Stream ftpStream = ftpRequest.GetRequestStream();
				FileStream localFileStream = new FileStream(localFile, FileMode.Create);
				byte[] byteBuffer = new byte[info.BufferSize];
				int bytesSent = localFileStream.Read(byteBuffer, 0, info.BufferSize);
				try
				{
					while (bytesSent != 0)
					{
						ftpStream.Write(byteBuffer, 0, bytesSent);
						bytesSent = localFileStream.Read(byteBuffer, 0, info.BufferSize);
					}
				}
				catch (Exception ex)
				{
					Log.Add(ex.ToString());
				}

				localFileStream.Close();
				ftpStream.Close();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
		}
		
		public void Delete(string deleteFile)
		{
			FtpWebRequest ftpRequest = info.CreateRequest(deleteFile);
			ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			ftpResponse.Close();
		}
		
		public void Rename(string currentFileNameAndPath, string newFileName)
		{
			FtpWebRequest ftpRequest = info.CreateRequest(currentFileNameAndPath);
			ftpRequest.Method = WebRequestMethods.Ftp.Rename;
			ftpRequest.RenameTo = newFileName;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			ftpResponse.Close();
		}
		
		public void CreateDirectory(string newDirectory)
		{
			FtpWebRequest ftpRequest = info.CreateRequest(newDirectory);
			ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			ftpResponse.Close();
		}
		
		public DateTime GetFileModifiedDateTime(string filePath)
		{
			FtpWebRequest ftpRequest = info.CreateRequest(filePath);
			ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			Stream ftpStream = ftpResponse.GetResponseStream();
			StreamReader ftpReader = new StreamReader(ftpStream);

			string fileInfo = ftpReader.ReadToEnd();
			
			ftpReader.Close();
			ftpStream.Close();
			ftpResponse.Close();

			//DateTime created = DateTime.Parse(fileInfo);

			Log.Add("Retrieved Remote File DateTimestamp",
				new Tag("File Path", filePath),
				new Tag("Created", ftpResponse.LastModified));

			return ftpResponse.LastModified;
		}
		
		public long GetFileSize(string filePath)
		{
			using (LogTimer logTimer = Log.Timer("Retrieving Remote File Size", new Tag("File Path", filePath)))
			{
				if (info.FileSizeSupported == false)
				{
					List<FtpItem> fileInfos = GetDirectoryListDetailed(Path.GetDirectoryName(filePath));
					foreach (FtpItem fileInfo in fileInfos)
					{
						if (fileInfo.fullPath == filePath)
							return fileInfo.Size;
					}
				}
				FtpWebRequest ftpRequest = info.CreateRequest(filePath);
				ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
				FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				Stream ftpStream = ftpResponse.GetResponseStream();
				StreamReader ftpReader = new StreamReader(ftpStream);
				/*string fileInfo = null;
				while (ftpReader.Peek() != -1)
				{
					fileInfo = ftpReader.ReadToEnd(); // filesize ever?
				}*/

				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();

				logTimer.Add("Retrieved Remote File Size",
					new Tag("File Path", filePath),
					new Tag("Size", ftpResponse.ContentLength));
				return ftpResponse.ContentLength;
			}
		}

		// List Directory Contents File/Folder Name Only
		public List<string> GetDirectoryListSimple(string directory)
		{
			var files = new List<string>();

			FtpWebRequest ftpRequest = info.CreateRequest(directory);
			ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			Stream ftpStream = ftpResponse.GetResponseStream();
			StreamReader ftpReader = new StreamReader(ftpStream);
			while (ftpReader.Peek() != -1)
			{
				files.Add(ftpReader.ReadLine());
			}
			ftpReader.Close();
			ftpStream.Close();
			ftpResponse.Close();
			
			return files;
		}

		// List Directory Contents in Detail (Name, Size, Modified, etc.)
		public List<FtpItem> GetDirectoryListDetailed(string directory)
		{
			if (directory == null || directory.Length == 0)
				directory = "/";
			var files = new List<FtpItem>();

			FtpWebRequest ftpRequest = info.CreateRequest(directory);
			ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			Stream ftpStream = ftpResponse.GetResponseStream();
			StreamReader ftpReader = new StreamReader(ftpStream);

			while (ftpReader.Peek() != -1)
			{
				// "-rwxrwxr-x   1 proftpd  proftpd     19840 Sep 30 00:49 alt_allele_attrib.txt.gz"
				// "-rwxrwxr-x   1 proftpd  proftpd      4580 Sep 30 00:49 alt_allele_group.txt.gz"
				string line = ftpReader.ReadLine();
				string[] columns = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				FtpItem fileData = new FtpItem();
				fileData.directory = (columns[0][0] == 'd');
				fileData.Size = long.Parse(columns[4]);

				string month = columns[5];	// "Sep"
				string day = columns[6];	// "30"
				string time = columns[7];   // "00:49"

				//fileData.modified = new DateTime(
				fileData.Filename = columns[8]; // "alt_allele_attrib.txt.gz"
				string nativePath = Paths.Combine(directory, fileData.Filename);
				string linuxPath = nativePath.Replace('\\', '/');
				fileData.fullPath = linuxPath;
				//fileInfo.modified = new DateTime();
				files.Add(fileData);
			}
			ftpReader.Close();
			ftpStream.Close();
			ftpResponse.Close();

			return files;
		}

		// Machine List Directory Contents in Detail (Name, Size, Modified, etc.), provides accurate modified time
		/*public List<FtpItem> GetMachineListDirectory(string directory)
		{
			if (directory == null || directory.Length == 0)
				directory = "/";
			var files = new List<FtpItem>();

			FtpWebRequest ftpRequest = info.CreateRequest(directory);
			ftpRequest.Method = "MLSD";
			FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
			Stream ftpStream = ftpResponse.GetResponseStream();
			StreamReader ftpReader = new StreamReader(ftpStream);

			while (ftpReader.Peek() != -1)
			{
				// "-rwxrwxr-x   1 proftpd  proftpd     19840 Sep 30 00:49 alt_allele_attrib.txt.gz"
				// "-rwxrwxr-x   1 proftpd  proftpd      4580 Sep 30 00:49 alt_allele_group.txt.gz"
				string line = ftpReader.ReadLine();
				string[] columns = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				FtpItem fileData = new FtpItem();
				fileData.directory = (columns[0][0] == 'd');
				fileData.Size = long.Parse(columns[4]);

				string month = columns[5];  // "Sep"
				string day = columns[6];    // "30"
				string time = columns[7];   // "00:49"

				//fileData.modified = new DateTime(
				fileData.Filename = columns[8]; // "alt_allele_attrib.txt.gz"
				string nativePath = Paths.Combine(directory, fileData.Filename);
				string linuxPath = nativePath.Replace('\\', '/');
				fileData.fullPath = linuxPath;
				//fileInfo.modified = new DateTime();
				files.Add(fileData);
			}
			ftpReader.Close();
			ftpStream.Close();
			ftpResponse.Close();

			return files;
		}*/
	}
}
