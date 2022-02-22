using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Tabs.Tools;

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
		public readonly TabDirectory Tab;

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

			var items = new ItemCollection<INodeView>();
			foreach (string directoryPath in Directory.EnumerateDirectories(Tab.Path))
			{
				var directoryView = new DirectoryView(directoryPath);
				items.Add(directoryView);
			}

			foreach (string filePath in Directory.EnumerateFiles(Tab.Path))
			{
				var fileView = new FileView(filePath);
				items.Add(fileView);
			}
			model.ItemList.Add(items);
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

public interface INodeView : IHasLinks
{
	public string Name { get; }

	[StyleValue]
	public long? Size { get; }

	[StyleValue]
	public DateTime Modified { get; }
}

public class DirectoryView : INodeView
{
	public string Directory { get; set; }

	public string Name => Directory;
	public long? Size => null;
	public DateTime Modified { get; set; }
	public bool HasLinks => true;

	[InnerValue]
	public ITab Tab;

	public string DirectoryPath;

	public DirectoryView(string directoryPath)
	{
		DirectoryPath = directoryPath;
		Directory = Path.GetFileName(directoryPath);
		var info = new DirectoryInfo(directoryPath);
		Modified = info.LastWriteTime.Trim();
		Tab = new TabDirectory(directoryPath);
	}

	public override string ToString() => Directory;
}

public class FileView : INodeView
{
	public string Filename { get; set; }
	public long? Size { get; set; }
	public DateTime Modified { get; set; }
	public bool HasLinks => false;

	public string Name => Filename;

	[InnerValue]
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
		Modified = FileInfo.LastWriteTime.Trim();

		if (Filename.EndsWith(".atlas"))
			Tab = new TabFileSerialized(filePath);
		else
			Tab = new TabFile(filePath);
	}
}
