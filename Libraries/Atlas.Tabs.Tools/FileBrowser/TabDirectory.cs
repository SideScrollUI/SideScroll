using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;

namespace Atlas.Tabs.Tools;

public class TabDirectory : ITab
{
	public string Path;

	public override string ToString() => Path;

	public TabDirectory(string path)
	{
		Path = path;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		//[Separator]
		//public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Streams.Delete);
	}

	public class Instance(TabDirectory Tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.ShowTasks = true;
			model.CustomSettingsPath = Tab.Path;

			if (!Directory.Exists(Tab.Path))
			{
				model.AddObject("Directory doesn't exist");
				return;
			}

			var toolbar = new Toolbar();
			toolbar.ButtonOpenFolder.Action = OpenFolder;
			//toolbar.ButtonDelete.Action = Delete;
			model.AddObject(toolbar);

			List<DirectoryView> directories = GetDirectories(call);
			List<FileView> files = GetFiles(call);

			List<INodeView> nodes = new(directories);
			nodes.AddRange(files);

			if (directories.Count == nodes.Count)
				model.Items = new List<IDirectoryView>(directories);
			else
				model.Items = nodes;
		}

		private List<FileView> GetFiles(Call call)
		{
			try
			{
				return Directory.EnumerateFiles(Tab.Path)
					.Select(f => new FileView(f))
					.ToList();
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return new List<FileView>();
		}

		private List<DirectoryView> GetDirectories(Call call)
		{
			try
			{
				return Directory.EnumerateDirectories(Tab.Path)
					.Select(f => new DirectoryView(f))
					.ToList();
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return new List<DirectoryView>();
		}

		private void OpenFolder(Call call)
		{
			string path = Tab.Path;

			// Select file if possible
			List<SelectedRow> selectedRows = GetSelectedRows();
			string? select = selectedRows.FirstOrDefault()?.Label;

			ProcessUtils.OpenFolder(path, select);
		}

		private List<SelectedRow> GetSelectedRows()
		{
			return TabViewSettings.TabDataSettings.SelectMany(s => s.SelectedRows).ToList();
		}

		private void Delete(Call call)
		{
			// todo: Confirmation prompt?
			List<SelectedRow> selectedRows = GetSelectedRows();
			foreach (SelectedRow selectedRow in selectedRows)
			{
				string path = Paths.Combine(Tab.Path, selectedRow.Label);

				if (Directory.Exists(path))
					Directory.Delete(path, true);

				if (File.Exists(path))
					File.Delete(path);
			}
			Reload();
		}
	}
}

// Shows if only directories present
public interface IDirectoryView : IHasLinks
{
	public string Name { get; }
}

// Shows if files present
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

	public override string ToString() => Directory;

	public DirectoryView(string directoryPath)
	{
		DirectoryPath = directoryPath;
		Directory = Path.GetFileName(directoryPath);
		var info = new DirectoryInfo(directoryPath);
		LastWriteTime = info.LastWriteTime.Trim();
		Tab = new TabDirectory(directoryPath);
	}
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
