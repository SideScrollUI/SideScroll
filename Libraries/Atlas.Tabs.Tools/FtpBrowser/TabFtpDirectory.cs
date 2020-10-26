using Atlas.Core;
using Atlas.Network;
using System;
using System.Collections.Generic;

namespace Atlas.Tabs.Tools
{
	public class TabFtpDirectory : ITab
	{
		public FTP.Info FtpInfo;
		public string Path;

		public TabFtpDirectory(FTP.Info ftpInfo, string path)
		{
			FtpInfo = ftpInfo;
			Path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabFtpDirectory Tab;

			public Instance(TabFtpDirectory tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				FTP ftp = new FTP(call, Tab.FtpInfo);
				List<FtpItem> fileDatas = ftp.GetDirectoryListDetailed(Tab.Path);
				var directories = new ItemCollection<ListDirectory>();
				var files = new ItemCollection<ListFile>();
				foreach (FtpItem fileData in fileDatas)
				{
					if (fileData.Directory)
					{
						var listDirectory = new ListDirectory(Tab.FtpInfo, fileData);
						directories.Add(listDirectory);
					}
					else
					{
						var listFile = new ListFile(Tab.FtpInfo, fileData);
						files.Add(listFile);
					}
				}
				if (directories.Count > 0)
					model.ItemList.Add(directories);

				if (files.Count > 0)
					model.ItemList.Add(files);
			}
		}

		public class ListDirectory
		{
			public FTP.Info FtpInfo;
			public string Directory { get; set; }
			[HiddenColumn, InnerValue]
			public ITab Tab;
			public string DirectoryPath;
			public FtpItem FileData;

			public ListDirectory(FTP.Info ftpInfo, FtpItem fileData)
			{
				FtpInfo = ftpInfo;
				FileData = fileData;
				DirectoryPath = fileData.FullPath;
				Directory = System.IO.Path.GetFileName(DirectoryPath);
				Tab = new TabFtpDirectory(ftpInfo, DirectoryPath);
			}

			public override string ToString()
			{
				return Directory;
			}
		}

		public class ListFile
		{
			public FTP.Info FtpInfo;
			public string Filename { get; set; }
			public long Size { get; set; }
			public DateTime Modified { get; set; }
			[HiddenColumn, InnerValue]
			public ITab Tab;

			public string FilePath;
			public FtpItem FileData;

			public ListFile(FTP.Info ftpInfo, FtpItem fileData)
			{
				FtpInfo = ftpInfo;
				FileData = fileData;
				FilePath = fileData.FullPath;

				Filename = fileData.Filename;
				Size = fileData.Size;
				Modified = fileData.Modified;
				/*if (Filename.EndsWith(".atlas"))
					iTab = new TabFileSerialized(filePath);
				else
					iTab = new TabFile(filePath);*/
			}

			public override string ToString()
			{
				return Filename;
			}
		}
	}
}
