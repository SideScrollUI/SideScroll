using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Tools.FileViewer;

public interface IFileTypeView
{
	string? Path { get; set; }
}

public class TabFile(FileView fileView) : ITab
{
	public TabFile(string filePath) : this(new FileView(filePath)) { }

	public FileView FileView = fileView;

	public string Path = fileView.Path;

	public static Dictionary<string, Type> ExtensionTypes { get; set; } = [];

	public static void RegisterType<T>(params string[] extensions) where T : new()
	{
		foreach (string extension in extensions)
		{
			ExtensionTypes[extension] = typeof(T);
		}
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolToggleButton? ButtonStar { get; set; }

		[Separator]
		public ToolButton ButtonOpenFolder { get; set; } = new("Open Folder", Icons.Svg.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; set; } = new("Delete", Icons.Svg.Delete);

		[Separator]
		public ToolButton? ButtonSelect { get; set; }
	}

	public class Instance(TabFile tab) : TabInstance, ITabAsync
	{
		public FileView FileView => tab.FileView;
		public SelectFileDelegate? SelectFileDelegate => tab.FileView.FileSelectorOptions?.SelectFileDelegate;

		public async Task LoadAsync(Call call, TabModel model)
		{
			FileView.FileSelectorOptions ??= new()
			{
				DataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project),
			};
		}

		public override void Load(Call call, TabModel model)
		{
			string path = tab.Path;
			if (!File.Exists(path))
			{
				model.AddObject("File doesn't exist");
				return;
			}

			Toolbar toolbar = new()
			{
				ButtonStar = new("Favorite", Icons.Svg.StarFilled, Icons.Svg.Star, new ListProperty(FileView, nameof(FileView.Favorite)))
			};
			toolbar.ButtonOpenFolder.Action = OpenFolder;
			toolbar.ButtonDelete.Action = Delete;

			if (SelectFileDelegate != null)
			{
				toolbar.ButtonSelect = new("Select", Icons.Svg.Enter);
				toolbar.ButtonSelect.Action = SelectFile;
			}

			model.AddObject(toolbar);

			List<ListItem> items = [];

			string extension = System.IO.Path.GetExtension(path).ToLower();

			if (ExtensionTypes.TryGetValue(extension, out Type? type))
			{
				var tab = (IFileTypeView)Activator.CreateInstance(type)!;
				tab.Path = path;
				items.Add(new ListItem(extension, tab));
			}

			if (extension == ".json")
			{
				string text = File.ReadAllText(path);
				items.Add(new ListItem("Contents", text));
				items.Add(new ListItem("Json", LazyJsonNode.Parse(text)));
			}
			else
			{
				if (FileUtils.IsTextFile(path))
				{
					items.Add(new ListItem("Contents", new FilePath(path)));
				}
			}
			items.Add(new ListItem("File Info", new FileInfo(path)));

			model.Items = items;
		}

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(tab.Path);
		}

		private void Delete(Call call)
		{
			if (File.Exists(tab.Path))
				File.Delete(tab.Path);

			Refresh();
		}

		private void SelectFile(Call call)
		{
			SelectFileDelegate!(call, tab.Path);
		}
	}
}
