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
			private TabDirectory tab;

			public Instance(TabDirectory tab)
			{
				this.tab = tab;
			}

			public override void Load(Call call, TabModel model)
			{
				if (!Directory.Exists(tab.Path))
					return;

				var actions = new ItemCollection<TaskCreator>()
				{
					new TaskDelegate("Delete", Delete, true),
				};
				model.Actions = actions;


				var directories = new ItemCollection<ListDirectory>();
				foreach (string directoryPath in Directory.EnumerateDirectories(tab.Path))
				{
					var listDirectory = new ListDirectory(directoryPath);
					directories.Add(listDirectory);
				}
				model.ItemList.Add(directories);

				var files = new ItemCollection<ListFile>();
				foreach (string filePath in Directory.EnumerateFiles(tab.Path))
				{
					var listFile = new ListFile(filePath);
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

	public class ListDirectory
	{
		public string Directory { get; set; }
		[HiddenColumn]
		[InnerValue]
		public ITab iTab;
		public string DirectoryPath;

		public ListDirectory(string directoryPath)
		{
			DirectoryPath = directoryPath;
			Directory = Path.GetFileName(directoryPath);
			iTab = new TabDirectory(directoryPath);
		}

		public override string ToString()
		{
			return Directory;
		}
	}

	public class ListFile
	{
		public string Filename { get; set; }
		public long Size { get; set; }
		public DateTime Modified { get; set; }
		[HiddenColumn]
		[InnerValue]
		public ITab iTab;
		private string filePath;
		private FileInfo fileInfo;

		public ListFile(string filePath)
		{
			this.filePath = filePath;
			this.fileInfo = new FileInfo(filePath);

			Filename = Path.GetFileName(filePath);
			Size = fileInfo.Length;
			Modified = fileInfo.LastWriteTime;
			if (Filename.EndsWith(".atlas"))
				iTab = new TabFileSerialized(filePath);
			else
				iTab = new TabFile(filePath);
		}

		public override string ToString()
		{
			return Filename;
		}
	}
}
