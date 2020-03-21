using Atlas.Core;
using Atlas.Network;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools
{
	public class TabFtpDirectory : ITab
	{
		public FTP.Info ftpInfo;
		public string path;

		public TabFtpDirectory(FTP.Info ftpInfo, string path)
		{
			this.ftpInfo = ftpInfo;
			this.path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			private TabFtpDirectory tab;

			public Instance(TabFtpDirectory tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				FTP ftp = new FTP(call, tab.ftpInfo);
				List<FtpItem> fileDatas = ftp.GetDirectoryListDetailed(tab.path);
				ItemCollection<ListDirectory> directories = new ItemCollection<ListDirectory>();
				ItemCollection<ListFile> files = new ItemCollection<ListFile>();
				foreach (FtpItem fileData in fileDatas)
				{
					if (fileData.directory)
					{
						ListDirectory listDirectory = new ListDirectory(tab.ftpInfo, fileData);
						directories.Add(listDirectory);
					}
					else
					{
						ListFile listFile = new ListFile(tab.ftpInfo, fileData);
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
			public FTP.Info ftpInfo;
			public string Directory { get; set; }
			[HiddenColumn]
			[InnerValue]
			public ITab iTab;
			private string directoryPath;
			private FtpItem fileData;

			public ListDirectory(FTP.Info ftpInfo, FtpItem fileData)
			{
				this.ftpInfo = ftpInfo;
				this.fileData = fileData;
				this.directoryPath = fileData.fullPath;
				Directory = Path.GetFileName(directoryPath);
				iTab = new TabFtpDirectory(ftpInfo, directoryPath);
			}

			public override string ToString()
			{
				return Directory;
			}
		}

		public class ListFile
		{
			public FTP.Info ftpInfo;
			public string Filename { get; set; }
			public long Size { get; set; }
			public DateTime Modified { get; set; }
			[HiddenColumn]
			[InnerValue]
			public ITab iTab;
			private string filePath;
			private FtpItem fileData;

			public ListFile(FTP.Info ftpInfo, FtpItem fileData)
			{
				this.ftpInfo = ftpInfo;
				this.fileData = fileData;
				this.filePath = fileData.fullPath;

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
