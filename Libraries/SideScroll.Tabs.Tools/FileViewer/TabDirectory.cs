using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Settings;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Tools.FileViewer;

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

	public class Toolbar : TabToolbar
	{
		public ToolToggleButton? ButtonStar { get; set; }

		[Separator]
		public ToolButton ButtonRefresh { get; set; } = new("Refresh", Icons.Svg.Refresh);

		[Separator]
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Svg.Delete, showTask: true);
	}

	public class Instance(TabDirectory tab) : TabInstance, ITabAsync
	{
		public DirectoryView DirectoryView => tab.DirectoryView;

		public async Task LoadAsync(Call call, TabModel model)
		{
			DirectoryView.FileSelectorOptions ??= new()
			{
				DataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project),
			};
		}

		public override void Load(Call call, TabModel model)
		{
			model.CustomSettingsPath = tab.Path;
			model.Editing = true;
			model.ShowTasks = true;

			if (!Directory.Exists(tab.Path))
			{
				model.AddObject("Directory doesn't exist");
				return;
			}

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
				model.Items = new List<IDirectoryView>(directories);
			}
			else
			{
				model.Items = nodes;
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
						fileExtensions.Any(ext => ext.Equals(System.IO.Path.GetExtension(name), StringComparison.CurrentCultureIgnoreCase)))
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
				string path = Paths.Combine(tab.Path, selectedRow.Label);

				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
				}

				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			Reload();
		}
	}
}
