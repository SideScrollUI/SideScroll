using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;
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

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Streams.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Streams.Delete);
	}

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

			var toolbar = new Toolbar();
			toolbar.ButtonOpenFolder.Action = OpenFolder;
			toolbar.ButtonDelete.Action = Delete;
			model.AddObject(toolbar);

			var directories = new List<IDirectoryView>();
			var items = new List<INodeView>();
			foreach (string directoryPath in Directory.EnumerateDirectories(Tab.Path))
			{
				var directoryView = new DirectoryView(directoryPath);
				directories.Add(directoryView);
				items.Add(directoryView);
			}

			foreach (string filePath in Directory.EnumerateFiles(Tab.Path))
			{
				var fileView = new FileView(filePath);
				items.Add(fileView);
			}

			if (directories.Count == items.Count)
				model.Items = directories;
			else
				model.Items = items;
		}

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(Tab.Path);
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

public interface IDirectoryView : IHasLinks
{
	public string Name { get; }
}

public interface INodeView : IHasLinks
{
	public string Name { get; }

	[StyleValue, Formatter(typeof(ByteFormatter))]
	public long? Size { get; }

	[StyleValue, Formatted]
	public TimeSpan Modified { get; }
}

public class DirectoryView : INodeView, IDirectoryView
{
	public string Directory { get; set; }

	public string Name => Directory;
	public long? Size => null;
	public DateTime LastWriteTime { get; set; }
	public TimeSpan Modified => LastWriteTime.Age();
	public bool HasLinks => true;

	[InnerValue]
	public ITab Tab;

	public string DirectoryPath;

	public DirectoryView(string directoryPath)
	{
		DirectoryPath = directoryPath;
		Directory = Path.GetFileName(directoryPath);
		var info = new DirectoryInfo(directoryPath);
		LastWriteTime = info.LastWriteTime.Trim();
		Tab = new TabDirectory(directoryPath);
	}

	public override string ToString() => Directory;
}

public class FileView : INodeView
{
	public string Filename { get; set; }
	public long? Size { get; set; }
	public DateTime LastWriteTime { get; set; }
	public TimeSpan Modified => LastWriteTime.Age();
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
		LastWriteTime = FileInfo.LastWriteTime.Trim();

		if (Filename.EndsWith(".atlas"))
			Tab = new TabFileSerialized(filePath);
		else
			Tab = new TabFile(filePath);
	}
}
