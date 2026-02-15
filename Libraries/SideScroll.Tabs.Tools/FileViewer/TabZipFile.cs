using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using SideScroll.Utilities;
using System.IO.Compression;

namespace SideScroll.Tabs.Tools.FileViewer;

public class TabZipFile : ITab, IFileTypeView
{
	public string? Path { get; set; }

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);
	}

	public class Instance(TabZipFile tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.ShowTasks = true;

			if (string.IsNullOrEmpty(tab.Path) || !File.Exists(tab.Path))
			{
				model.AddObject("Zip file doesn't exist");
				return;
			}

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			model.AddObject(toolbar);

			try
			{
				List<ZipNodeView> nodes = LoadZipContents(call, tab.Path);
				model.Items = nodes;
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
				model.AddObject($"Error loading zip file: {ex.Message}");
			}
		}

		private List<ZipNodeView> LoadZipContents(Call call, string zipPath)
		{
			var rootNodes = new List<ZipNodeView>();
			var directories = new Dictionary<string, ZipDirectoryView>(StringComparer.OrdinalIgnoreCase);

			try
			{
				using var archive = ZipFile.OpenRead(zipPath);

				foreach (var entry in archive.Entries)
				{
					string fullName = entry.FullName.Replace('\\', '/');

					// Skip empty entries (sometimes present in zip files)
					if (string.IsNullOrEmpty(fullName))
						continue;

					// Check if this is a directory entry
					if (fullName.EndsWith('/'))
					{
						string dirPath = fullName.TrimEnd('/');
						EnsureDirectoryPath(directories, dirPath, rootNodes);
					}
					else
					{
						// It's a file
						string dirPath = System.IO.Path.GetDirectoryName(fullName)?.Replace('\\', '/') ?? "";
						ZipDirectoryView? parentDir = null;

						if (!string.IsNullOrEmpty(dirPath))
						{
							parentDir = EnsureDirectoryPath(directories, dirPath, rootNodes);
						}

						var fileView = new ZipFileView(entry);

						if (parentDir != null)
						{
							parentDir.Children.Add(fileView);
						}
						else
						{
							rootNodes.Add(fileView);
						}
					}
				}
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return rootNodes;
		}

		private ZipDirectoryView EnsureDirectoryPath(
			Dictionary<string, ZipDirectoryView> directories,
			string path,
			List<ZipNodeView> rootNodes)
		{
			if (directories.TryGetValue(path, out var existing))
				return existing;

			string[] parts = path.Split('/');
			ZipDirectoryView? parent = null;
			string currentPath = "";

			for (int i = 0; i < parts.Length; i++)
			{
				if (i > 0)
					currentPath += "/";
				currentPath += parts[i];

				if (!directories.TryGetValue(currentPath, out var dir))
				{
					dir = new ZipDirectoryView(parts[i], currentPath);
					directories[currentPath] = dir;

					if (parent != null)
					{
						parent.Children.Add(dir);
					}
					else
					{
						rootNodes.Add(dir);
					}
				}

				parent = dir;
			}

			return parent!;
		}

		private void Refresh(Call call)
		{
			Reload();
		}
	}
}

// Base class for zip entries
[Unserialized]
public abstract class ZipNodeView : IHasLinks
{
	public abstract string Name { get; }

	[StyleValue, Formatter(typeof(ByteFormatter))]
	public abstract long? Size { get; }

	[StyleValue, Formatted]
	public abstract TimeSpan? Modified { get; }

	[Hidden]
	public abstract bool HasLinks { get; }

	[HiddenColumn]
	public string FullPath { get; }

	[InnerValue, Unserialized, HiddenColumn]
	public ITab? Tab { get; set; }

	public override string ToString() => Name;

	protected ZipNodeView(string fullPath)
	{
		FullPath = fullPath;
	}
}

// Represents a directory within a zip file
public class ZipDirectoryView : ZipNodeView, IHasLinks
{
	public override string Name { get; }
	public override long? Size => null;
	public DateTime? LastWriteTime { get; }
	public override TimeSpan? Modified => LastWriteTime?.Age();
	public override bool HasLinks => true;

	[InnerValue, HiddenColumn]
	public List<ZipNodeView> Children { get; } = [];

	public ZipDirectoryView(string name, string fullPath) : base(fullPath)
	{
		Name = name;
		Tab = new TabZipDirectory(this);
	}
}

// Represents a file within a zip file
public class ZipFileView : ZipNodeView
{
	public override string Name { get; }
	public override long? Size { get; }
	public DateTime? LastWriteTime { get; }
	public override TimeSpan? Modified => LastWriteTime?.Age();
	public override bool HasLinks => false;

	[HiddenColumn]
	public long CompressedSize { get; }

	public ZipFileView(ZipArchiveEntry entry) : base(entry.FullName)
	{
		Name = System.IO.Path.GetFileName(entry.FullName);
		Size = entry.Length;
		CompressedSize = entry.CompressedLength;
		LastWriteTime = entry.LastWriteTime.DateTime;
	}
}

// Tab for displaying a zip directory's contents
[PrivateData]
public class TabZipDirectory : ITab
{
	private readonly ZipDirectoryView _directoryView;

	public string Path => _directoryView.FullPath;

	public TabZipDirectory(ZipDirectoryView directoryView)
	{
		_directoryView = directoryView;
	}

	public override string ToString() => Path;

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);
	}

	public class Instance(TabZipDirectory tab) : TabInstance
	{
		public override void Load(Call call, TabModel model)
		{
			model.Editing = true;
			model.ShowTasks = true;

			Toolbar toolbar = new();
			toolbar.ButtonRefresh.Action = Refresh;
			model.AddObject(toolbar);

			// Display the children of this directory
			List<ZipNodeView> nodes = tab._directoryView.Children;

			// Separate directories and files for better organization
			var directories = nodes.OfType<ZipDirectoryView>().ToList();
			var files = nodes.OfType<ZipFileView>().ToList();

			if (files.Count == 0)
			{
				model.Items = new List<ZipDirectoryView>(directories);
			}
			else
			{
				model.Items = nodes;
			}
		}

		private void Refresh(Call call)
		{
			Reload();
		}
	}
}
