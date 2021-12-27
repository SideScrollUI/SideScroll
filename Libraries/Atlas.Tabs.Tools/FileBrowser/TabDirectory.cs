using Atlas.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools
{
	public class TabDirectory : ITab
	{
		public string Path;

		public TabDirectory(string path)
		{
			Path = path;
		}

		public TabInstance Create() => new Instance(this);

		public class Instance : TabInstance
		{
			public TabDirectory Tab;

			public Instance(TabDirectory tab)
			{
				Tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				if (!Directory.Exists(Tab.Path))
					return;

				model.Actions = new List<TaskCreator>()
				{
					new TaskDelegate("Delete", Delete, true),
				};


				var directories = new ItemCollection<DirectoryView>();
				foreach (string directoryPath in Directory.EnumerateDirectories(Tab.Path))
				{
					var listDirectory = new DirectoryView(directoryPath);
					directories.Add(listDirectory);
				}
				model.ItemList.Add(directories);

				var files = new ItemCollection<FileView>();
				foreach (string filePath in Directory.EnumerateFiles(Tab.Path))
				{
					var listFile = new FileView(filePath);
					files.Add(listFile);
				}
				if (files.Count > 0)
					model.ItemList.Add(files);
			}

			private void Delete(Call call)
			{
				// Should we delete both directories and files?
				foreach (TabDataSettings tabDataSettings in TabViewSettings.TabDataSettings)
				{
					foreach (SelectedRow selectedRow in tabDataSettings.SelectedRows)
					{
						if (Directory.Exists(selectedRow.Label))
							Directory.Delete(selectedRow.Label, true);
					}
				}
				Reload();
			}
		}
	}

	public class DirectoryView
	{
		public string Directory { get; set; }

		[HiddenColumn, InnerValue]
		public ITab Tab;

		public string DirectoryPath;

		public DirectoryView(string directoryPath)
		{
			DirectoryPath = directoryPath;
			Directory = Path.GetFileName(directoryPath);
			Tab = new TabDirectory(directoryPath);
		}

		public override string ToString() => Directory;
	}

	public class FileView
	{
		public string Filename { get; set; }
		public long Size { get; set; }
		public DateTime Modified { get; set; }

		[HiddenColumn, InnerValue]
		public ITab Tab;

		public string FilePath;
		public FileInfo FileInfo;

		public override string ToString() => Filename;

		public FileView(string filePath)
		{
			FilePath = filePath;
			FileInfo = new FileInfo(filePath);

			Filename = Path.GetFileName(filePath);
			Size = FileInfo.Length;
			Modified = FileInfo.LastWriteTime;

			if (Filename.EndsWith(".atlas"))
				Tab = new TabFileSerialized(filePath);
			else
				Tab = new TabFile(filePath);
		}
	}
}
