using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.Resources;
using Atlas.Serialize;
using Atlas.Tabs.Toolbar;

namespace Atlas.Tabs.Tools;

public class TabDirectory(DirectoryView directoryView) : ITab
{
	public DirectoryView DirectoryView = directoryView;
	public string Path => DirectoryView.Path;
	public DataRepoView<NodeView>? DataRepoFavorites => DirectoryView.DataRepoFavorites;

	public override string ToString() => Path;

	public TabDirectory(string path, DataRepoView<NodeView>? dataRepoFavorites = null) :
		this(new DirectoryView(path, dataRepoFavorites))
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

	public class Instance(TabDirectory tab) : TabInstance
	{
		public DirectoryView DirectoryView => tab.DirectoryView;

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
					.Select(f => new FileView(f, tab.DataRepoFavorites))
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
					.Select(f => new DirectoryView(f, tab.DataRepoFavorites))
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
