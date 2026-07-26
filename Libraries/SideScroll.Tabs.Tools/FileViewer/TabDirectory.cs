using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Bookmarks.Models;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Tools.FileViewer;

[PrivateData]
public class TabDirectory(DirectoryView directoryView) : ITab
{
	public DirectoryView DirectoryView => directoryView;
	public string Path => DirectoryView.Path;

	[HiddenColumn]
	public FileSelectorOptions? FileSelectorOptions => DirectoryView.FileSelectorOptions;

	public override string ToString() => Path;

	public TabDirectory(string path, FileSelectorOptions? fileSelectorOptions = null) :
		this(new DirectoryView(path, fileSelectorOptions))
	{ }

	public TabInstance Create() => new Instance(this);

	/// <summary>
	/// Resolves the file system path for a selected row, or <c>null</c> when it doesn't resolve to
	/// something inside <paramref name="directoryPath"/>.
	/// Selected rows are restored from deserialized view settings, so the path comes from the row's
	/// <c>[DataKey]</c> rather than recombining its display label, and it's checked before use
	/// </summary>
	public static string? GetSelectedPath(string directoryPath, SelectedRow selectedRow)
	{
		if (string.IsNullOrEmpty(selectedRow.DataKey)) return null;

		string root;
		string path;
		try
		{
			root = System.IO.Path.GetFullPath(directoryPath);
			path = System.IO.Path.GetFullPath(selectedRow.DataKey);
		}
		catch (Exception)
		{
			return null; // Invalid characters, or too long
		}

		// Reject anything outside the directory being viewed, including the directory itself.
		// GetRelativePath() returns a rooted path when there's no shared root (a different drive)
		string relative = System.IO.Path.GetRelativePath(root, path);
		if (relative is "." or ".." ||
			relative.StartsWith(".." + System.IO.Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
			System.IO.Path.IsPathRooted(relative))
		{
			return null;
		}

		return path;
	}

	public class Toolbar : TabToolbar
	{
		public ToolToggleButton? ButtonStar { get; set; }

		[Separator]
		public ToolButton ButtonRefresh { get; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonOpenFolder { get; } = new("Open Folder", Icons.Svg.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; } = new("Delete", Icons.Svg.Delete, showTask: true);
	}

	public class Instance(TabDirectory tab) : TabInstance, ITabAsync
	{
		public DirectoryView DirectoryView => tab.DirectoryView;

		public async Task LoadAsync(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.Path;
			model.Editing = true;

			if (!Directory.Exists(tab.Path))
			{
				model.AddObject("Directory doesn't exist");
				return;
			}

			DirectoryView.FileSelectorOptions ??= new()
			{
				DataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project),
			};

			Toolbar toolbar = new();
			toolbar.ButtonStar = new("Favorite", Icons.Svg.StarFilled, Icons.Svg.Star, new ListProperty(DirectoryView, nameof(DirectoryView.Favorite)));
			toolbar.ButtonRefresh.Action = Refresh;
			toolbar.ButtonOpenFolder.Action = OpenFolder;
			toolbar.ButtonDelete.Action = Delete;
			toolbar.ButtonDelete.Flyout = new ConfirmationFlyoutConfig("Are you sure you want to delete the selected items in this directory?\n\n" + DirectoryView.Name, "Delete");
			model.AddObject(toolbar);

			List<DirectoryView> directories = GetDirectories(call);
			List<FileView> files = GetFiles(call);

			List<NodeView> nodes = [.. directories, .. files];

			if (directories.Count == nodes.Count)
			{
				model.AddItems(new List<IDirectoryView>(directories));
			}
			else
			{
				model.AddItems(nodes);
			}
		}

		private List<FileView> GetFiles(Call call)
		{
			try
			{
				List<string>? fileExtensions = DirectoryView.FileSelectorOptions?.FileExtensions;
				return Directory.EnumerateFiles(tab.Path)
					.Where(name =>
						fileExtensions == null ||
						// Ordinal, tr-TR doesn't treat 'I' and 'i' as the same letter when ignoring case
						fileExtensions.Any(ext => ext.Equals(System.IO.Path.GetExtension(name), StringComparison.OrdinalIgnoreCase)))
					.Select(name => new FileView(name, tab.FileSelectorOptions))
					.ToList();
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return [];
		}

		private List<DirectoryView> GetDirectories(Call call)
		{
			try
			{
				return Directory.EnumerateDirectories(tab.Path)
					.Select(name => new DirectoryView(name, tab.FileSelectorOptions))
					.ToList();
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return [];
		}

		private void Refresh(Call call)
		{
			Reload();
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
			return TabViewSettings.TabDataSettings
				.SelectMany(s => s.SelectedRows)
				.ToList();
		}

		private void Delete(Call call)
		{
			List<SelectedRow> selectedRows = GetSelectedRows();
			foreach (SelectedRow selectedRow in selectedRows)
			{
				if (GetSelectedPath(tab.Path, selectedRow) is not { } path)
				{
					call.Log.AddWarning("Skipped deleting a row that isn't in this directory",
						new Tag("Directory", tab.Path),
						new Tag("Row", selectedRow.DataKey ?? selectedRow.Label));
					continue;
				}

				// Keep deleting the rest if one entry is locked or already gone
				try
				{
					if (Directory.Exists(path))
					{
						Directory.Delete(path, true);
					}
					else if (File.Exists(path))
					{
						File.Delete(path);
					}
				}
				catch (Exception e)
				{
					call.Log.Add(e, new Tag("Path", path));
				}
			}
			Reload();
		}
	}
}
