using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;
using SideScroll.Utilities;
using System.IO.Compression;

namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>
/// Tab that displays the contents of a ZIP archive file.
/// </summary>
public class TabZipFile : ITab, IFileTypeView
{
	/// <summary>Gets or sets the path to the ZIP file.</summary>
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

		private static List<ZipNodeView> LoadZipContents(Call call, string zipPath)
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

		private static ZipDirectoryView EnsureDirectoryPath(
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
					currentPath += '/';
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

/// <summary>
/// Abstract base class for entries within a ZIP archive.
/// </summary>
[Unserialized]
public abstract class ZipNodeView(string fullPath) : IHasLinks
{
	/// <summary>Gets the display name of the entry.</summary>
	public abstract string Name { get; }

	/// <summary>Gets the uncompressed size in bytes, or <c>null</c> if not applicable.</summary>
	[StyleValue, Formatter(typeof(ByteFormatter))]
	public abstract long? Size { get; }

	/// <summary>Gets the time elapsed since the entry was last modified, or <c>null</c> if unavailable.</summary>
	[StyleValue, Formatted]
	public abstract TimeSpan? Modified { get; }

	/// <summary>Gets whether this entry can be navigated into.</summary>
	[Hidden]
	public abstract bool HasLinks { get; }

	/// <summary>Gets the full path of this entry within the archive.</summary>
	[HiddenColumn]
	public string FullPath { get; } = fullPath;

	/// <summary>Gets or sets the inner tab used to display this entry's content.</summary>
	[InnerValue, Unserialized, HiddenColumn]
	public ITab? Tab { get; set; }

	public override string ToString() => Name;
}

/// <summary>
/// Represents a directory within a ZIP archive.
/// </summary>
public class ZipDirectoryView : ZipNodeView, IHasLinks
{
	public override string Name { get; }
	public override long? Size => null;

	/// <summary>Gets the last write time of the directory entry.</summary>
	public DateTime? LastWriteTime { get; }

	public override TimeSpan? Modified => LastWriteTime?.Age();
	public override bool HasLinks => true;

	/// <summary>Gets the list of child entries within this directory.</summary>
	[InnerValue, HiddenColumn]
	public List<ZipNodeView> Children { get; } = [];

	public ZipDirectoryView(string name, string fullPath) : base(fullPath)
	{
		Name = name;
		Tab = new TabZipDirectory(this);
	}
}

/// <summary>
/// Represents a file within a ZIP archive.
/// </summary>
public class ZipFileView(ZipArchiveEntry entry) : ZipNodeView(entry.FullName)
{
	public override string Name { get; } = Path.GetFileName(entry.FullName);
	public override long? Size { get; } = entry.Length;

	/// <summary>Gets the last write time of the file entry.</summary>
	public DateTime? LastWriteTime { get; } = entry.LastWriteTime.DateTime;

	public override TimeSpan? Modified => LastWriteTime?.Age();
	public override bool HasLinks => false;

	/// <summary>Gets the compressed size of the file in bytes.</summary>
	[HiddenColumn]
	public long CompressedSize { get; } = entry.CompressedLength;
}

/// <summary>
/// Tab for displaying a ZIP directory's contents.
/// </summary>
[PrivateData]
public class TabZipDirectory(ZipDirectoryView directoryView) : ITab
{
	private readonly ZipDirectoryView _directoryView = directoryView;

	/// <summary>Gets the full path of the directory within the archive.</summary>
	public string Path => _directoryView.FullPath;

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
