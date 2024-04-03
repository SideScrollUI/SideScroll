using Atlas.Core;
using Atlas.Extensions;
using Atlas.Resources;
using Atlas.Serialize;

namespace Atlas.Tabs.Tools;

public class TabDirectory(string path, DataRepoView<NodeView>? dataRepoNodes = null) : ITab
{
	public string Path = path;
	public DataRepoView<NodeView>? DataRepoNodes = dataRepoNodes;

	public override string ToString() => Path;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		//[Separator]
		//public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Streams.Delete);
	}

	public class Instance(TabDirectory tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.ShowTasks = true;
			model.CustomSettingsPath = tab.Path;
			model.Editing = true;

			if (!Directory.Exists(tab.Path))
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

			List<NodeView> nodes = new(directories);
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
				return Directory.EnumerateFiles(tab.Path)
					.Select(f => new FileView(f, tab.DataRepoNodes))
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
				return Directory.EnumerateDirectories(tab.Path)
					.Select(f => new DirectoryView(f, tab.DataRepoNodes))
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
			string path = tab.Path;

			// Select file if possible
			List<SelectedRow> selectedRows = GetSelectedRows();
			string? selection = selectedRows.FirstOrDefault()?.Label;

			ProcessUtils.OpenFolder(path, selection);
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
				string path = Paths.Combine(tab.Path, selectedRow.Label);

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
	[Name("  ★"), Editing]
	public bool Favorite { get; set; }

	public string Name { get; }
}

// Shows if files present
public abstract class NodeView : IHasLinks
{
	[Unserialized]
	public DataRepoView<NodeView>? DataRepo;

	[Name("  ★"), Editing]
	public bool Favorite
	{
		get => _favorite;
		set
		{
			_favorite = value;
			UpdateDataRepo();
		}
	}
	private bool _favorite;

	public abstract string Name { get; }

	[StyleValue, Formatter(typeof(ByteFormatter))]
	public abstract long? Size { get; set;  }

	[StyleValue, Formatted]
	public abstract TimeSpan Modified { get; }

	[Hidden]
	public abstract bool HasLinks { get; }

	public string Path;

	[InnerValue, Unserialized]
	public ITab? Tab;

	public override string ToString() => Name;

	public NodeView(string path)
		: this(path, null)
	{ }

	public NodeView(string path, DataRepoView<NodeView>? dataRepoNodes = null)
	{
		Path = path;
		DataRepo = dataRepoNodes;
		_favorite = dataRepoNodes?.Items.TryGetValue(Path, out _) == true;
	}

	private void UpdateDataRepo()
	{
		if (DataRepo == null) return;

		if (_favorite)
			DataRepo.Save(null, Path, this);
		else
			DataRepo.Delete(null, Path);
	}
}

public class DirectoryView : NodeView, IDirectoryView
{
	public string Directory { get; set; }

	public override string Name => Directory;
	public override long? Size { get; set; } = null;
	public DateTime LastWriteTime { get; set; }
	public override TimeSpan Modified => LastWriteTime.Age();
	public override bool HasLinks => true;

	public DirectoryView(string path)
		: this(path, null)
	{ }

	public DirectoryView(string path, DataRepoView<NodeView>? dataRepoNodes = null) :
		base(path, dataRepoNodes)
	{
		Directory = System.IO.Path.GetFileName(path);
		var info = new DirectoryInfo(path);
		LastWriteTime = info.LastWriteTime.Trim();
		Tab = new TabDirectory(path, dataRepoNodes);
	}
}

public class FileView : NodeView
{
	public string Filename { get; set; }
	public override long? Size { get; set; }
	public DateTime LastWriteTime { get; set; }
	public override TimeSpan Modified => LastWriteTime.Age();
	public override bool HasLinks => false;

	public override string Name => Filename;

	public FileInfo FileInfo;

	public FileView(string path)
		: this(path, null)
	{ }

	public FileView(string path, DataRepoView<NodeView>? dataRepoNodes = null)
		: base(path, dataRepoNodes)
	{
		FileInfo = new FileInfo(path);
		Filename = System.IO.Path.GetFileName(path);
		Size = FileInfo.Length;
		LastWriteTime = FileInfo.LastWriteTime.Trim();

		if (Filename.EndsWith(".atlas"))
			Tab = new TabFileSerialized(path);
		else
			Tab = new TabFile(path);
	}
}
