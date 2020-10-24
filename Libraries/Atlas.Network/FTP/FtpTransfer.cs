using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Atlas.Network
{
	public class FtpTransfer
	{
		public FTP.Info FtpInfo;
		public FtpFileInfo FtpFileInfo;

		public FtpTransfer()
		{
		}

		public FtpTransfer(FtpFileInfo ftpFileInfo)
		{
			FtpFileInfo = ftpFileInfo;
			FtpInfo = new FTP.Info(ftpFileInfo.FtpHost);
			LocalFilePath = ftpFileInfo.LocalPath;
			RemoteFile.FullPath = ftpFileInfo.RemotePath;
			RemoteFile.Filename = FileName;
		}

		public string FileName => Path.GetFileName(LocalFilePath);

		public FtpItem RemoteFile { get; set; } = new FtpItem();
		[HiddenColumn]
		public string LocalFilePath { get; set; }

		public FtpItem LocalFile
		{
			get
			{
				var fileInfo = new FtpItem()
				{
					FullPath = LocalFilePath,
					Filename = FileName,
				};
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

		public override string ToString() => FileName ?? "";

		public void Download(Call call, bool overwrite, long maxBytes = long.MaxValue)
		{
			if (overwrite == false)
			{
				UpdateRemoteFileSize(call);
				if (FilesMatch)
				{
					call.Log.Add("Files match, skipping downloading");
					return;
				}
			}
			FTP ftp = new FTP(call, FtpInfo);
			ftp.Download(RemoteFile.FullPath, LocalFilePath, call.TaskInstance, maxBytes);
			FileInfo fileInfo = new FileInfo(LocalFilePath);
			if (fileInfo.Extension == ".gz")
				Compression.Decompress(call, fileInfo);
		}

		public long UpdateRemoteFileSize(Call call, CancellationToken? cancellationToken = null)
		{
			FTP ftp = new FTP(call, FtpInfo);
			RemoteFile.Size = ftp.GetFileSize(RemoteFile.FullPath);
			return RemoteFile.Size;
		}

		public DateTime UpdateFileCreatedDateTime(Call call, CancellationToken? cancellationToken = null)
		{
			FTP ftp = new FTP(call, FtpInfo);
			RemoteFile.Modified = ftp.GetFileModifiedDateTime(RemoteFile.FullPath);
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
			var transfers = new List<TransferFtp>();
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
