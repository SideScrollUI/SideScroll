using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Atlas.Network
{
	public class FtpTransfer
	{
		public FTP.Info ftpInfo;
		private FtpFileInfo ftpFileInfo;

		public FtpTransfer()
		{
		}

		public FtpTransfer(FtpFileInfo ftpFileInfo)
		{
			this.ftpFileInfo = ftpFileInfo;
			ftpInfo = new FTP.Info(ftpFileInfo.FtpHost);
			LocalFilePath = ftpFileInfo.LocalPath;
			RemoteFile.fullPath = ftpFileInfo.RemotePath;
			RemoteFile.Filename = FileName;
		}

		public string FileName
		{
			get
			{
				return Path.GetFileName(LocalFilePath);
			}
		}

		public FtpItem RemoteFile { get; set; } = new FtpItem();
		[HiddenColumn]
		public string LocalFilePath { get; set; }

		public FtpItem LocalFile
		{
			get
			{
				FtpItem fileInfo = new FtpItem();
				fileInfo.fullPath = LocalFilePath;
				fileInfo.Filename = FileName;
				if (File.Exists(LocalFilePath))
				{
					fileInfo.Size = new FileInfo(LocalFilePath).Length;
					fileInfo.Modified = File.GetCreationTime(LocalFilePath); // this can take a little time
				}
				return fileInfo;
			}
		}

		public bool FilesMatch
		{
			get
			{
				return FileExists && LocalFile.Size == RemoteFile.Size;
				// check created when fixed
			}
		}

		public long? Size
		{
			get
			{
				if (LocalFile != null)
					return LocalFile.Size;
				return null;
			}
		}

		public override string ToString()
		{
			return FileName ?? "";
		}

		public void Download(Call call, bool overwrite, long maxBytes = long.MaxValue)
		{
			if (overwrite == false)
			{
				UpdateRemoteFileSize(call);
				if (FilesMatch)
				{
					call.log.Add("Files match, skipping downloading");
					return;
				}
			}
			FTP ftp = new FTP(call, ftpInfo);
			ftp.Download(RemoteFile.fullPath, LocalFilePath, call.taskInstance, maxBytes);
			FileInfo fileInfo = new FileInfo(LocalFilePath);
			if (fileInfo.Extension == ".gz")
				Compression.Decompress(call, fileInfo);
		}

		public long UpdateRemoteFileSize(Call call, CancellationToken? cancellationToken = null)
		{
			FTP ftp = new FTP(call, ftpInfo);
			RemoteFile.Size = ftp.GetFileSize(RemoteFile.fullPath);
			return RemoteFile.Size;
		}

		public DateTime UpdateFileCreatedDateTime(Call call, CancellationToken? cancellationToken = null)
		{
			FTP ftp = new FTP(call, ftpInfo);
			RemoteFile.Modified = ftp.GetFileModifiedDateTime(RemoteFile.fullPath);
			return RemoteFile.Modified;
		}

		public bool FileExists
		{
			get
			{
				return File.Exists(LocalFilePath);
			}
		}

		public void Delete()
		{
			if (FileExists)
				File.Delete(LocalFilePath);
		}

		/*public static List<TransferFtp> GetRemoteFiles(Call call, string remotePath)
		{
			List<TransferFtp> transfers = new List<TransferFtp>();
			FTP ftp = new FTP(log, ftpHost);
			var files = ftp.DirectoryListDetailed(remotePath);
			foreach (FTP.FileInfo remoteFile in files)
			{
				TransferFtp transfer = new TransferFtp();
				transfer.ftp = new FTP(log, ftpHost);
				transfer.localFile = Paths.Combine(directoryPath, remoteFile.fullPath);
				transfer.remoteFile = remoteFile;
				transfers.Add(transfer);
			}
			return transfers;
		}*/
	}
}
