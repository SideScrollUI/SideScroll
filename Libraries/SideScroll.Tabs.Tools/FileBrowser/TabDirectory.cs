using SideScroll.Core;
using SideScroll.Core.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs.Toolbar;

namespace SideScroll.Tabs.Tools;

public class TabDirectory(DirectoryView directoryView) : ITab
{
	public DirectoryView DirectoryView = directoryView;
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
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		//[Separator]
		//public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Svg.Delete);
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

			var toolbar = new Toolbar();
			toolbar.ButtonStar = new("Favorite", Icons.Svg.StarFilled, Icons.Svg.Star, new ListProperty(DirectoryView, nameof(DirectoryView.Favorite)));
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
				List<string>? fileExtensions = DirectoryView.FileSelectorOptions?.FileExtensions;
				return Directory.EnumerateDirectories(tab.Path)
					.Where(name =>
						fileExtensions == null ||
						fileExtensions.Any(ext => ext == System.IO.Path.GetExtension(name).ToLower()))
					.Select(name => new DirectoryView(name, tab.FileSelectorOptions))
					.ToList();
			}
			catch (Exception ex)
			{
				call.Log.Add(ex);
			}

			return [];
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

		/*private void Delete(Call call)
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
		}*/
	}
}
