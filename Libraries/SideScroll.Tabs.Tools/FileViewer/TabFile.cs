using SideScroll.Attributes;
using SideScroll.Resources;
using SideScroll.Tabs.Lists;
using SideScroll.Tabs.Toolbar;
using SideScroll.Tasks;
using SideScroll.Utilities;

namespace SideScroll.Tabs.Tools.FileViewer;

/// <summary>Marker interface for tab types that display a file, exposing the file path.</summary>
public interface IFileTypeView
{
	/// <summary>Gets or sets the path of the file being displayed.</summary>
	string? Path { get; set; }
}

[PrivateData]
public class TabFile(FileView fileView) : ITab
{
	public TabFile(string filePath) : this(new FileView(filePath)) { }

	public FileView FileView => fileView;

	public string Path => fileView.Path;

	// Ordinal so extensions match regardless of case, without ToLower() mangling them in
	// cultures where 'I' doesn't lowercase to 'i' (tr-TR turns ".ZIP" into ".zıp")
	public static Dictionary<string, Type> ExtensionTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
	{
		[".zip"] = typeof(TabZipFile),
	};

	/// <summary>
	/// Registers a tab type for specific file extensions.
	/// </summary>
	public static void RegisterType<T>(params string[] extensions) where T : IFileTypeView, new()
	{
		foreach (string extension in extensions)
		{
			ExtensionTypes[extension] = typeof(T);
		}
	}

	/// <summary>
	/// Detects the appropriate tab type for a file by checking probes and then extensions.
	/// </summary>
	private static Type? DetectFileType(string path, Call? call = null)
	{
		if (!File.Exists(path))
			return null;

		// First try content-based probing
		Type? probedType = FileTypeDetector.ProbeFile(path, call);
		if (probedType != null)
			return probedType;

		// Fall back to extension-based detection, ExtensionTypes ignores case
		string extension = System.IO.Path.GetExtension(path);
		if (ExtensionTypes.TryGetValue(extension, out Type? type))
		{
			return type;
		}

		return null;
	}

	public TabInstance Create() => new Instance(this);

	public class Toolbar : TabToolbar
	{
		public ToolToggleButton? ButtonStar { get; set; }

		[Separator]
		public ToolButton ButtonOpenFolder { get; } = new("Open Folder", Icons.Svg.OpenFolder);

		[Separator]
		public ToolButton ButtonDelete { get; } = new("Delete", Icons.Svg.Delete, showTask: true)
		{
			Flyout = new ConfirmationFlyoutConfig("Are you sure you want to delete this file?", "Delete"),
		};

		[Separator]
		public ToolButton? ButtonSelect { get; set; }
	}

	public class Instance(TabFile tab) : TabInstance, ITabAsync
	{
		public FileView FileView => tab.FileView;
		public SelectFileDelegate? SelectFileDelegate => tab.FileView.FileSelectorOptions?.SelectFileDelegate;

		public async Task LoadAsync(Call call, TabModel model)
		{
			string path = tab.Path;
			if (!File.Exists(path))
			{
				model.AddObject("File doesn't exist");
				return;
			}

			FileView.FileSelectorOptions ??= new()
			{
				DataRepoFavorites = await FileDataRepos.Favorites.LoadViewAsync(call, Project),
			};

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

			string extension = System.IO.Path.GetExtension(path).ToLowerInvariant();

			// Use probe-based detection or fall back to extension-based detection
			Type? type = DetectFileType(path, call);
			if (type != null)
			{
				var viewTab = (IFileTypeView)Activator.CreateInstance(type)!;
				viewTab.Path = path;
				items.Add(new ListItem(extension, viewTab));
			}

			if (extension == ".json")
			{
				// Contents is a path like every other text file, rather than the file read into a
				// string and held for as long as the tab is open. LoadPath() reads it again for the
				// parse, but that copy is released once the nodes are built, where holding both the
				// raw text and the parsed nodes kept two copies of the file for the tab's lifetime
				items.Add(new ListItem("Contents", new FilePath(path)));

				// A file that doesn't parse still opens, showing its contents. The exception used to
				// escape LoadAsync() before any item was added, so a malformed file gave an empty tab
				// rather than the text needed to see what was wrong with it
				try
				{
					items.Add(new ListItem("Json", LazyJsonNode.LoadPath(path)));
				}
				catch (Exception e)
				{
					call.Log.AddWarning("Couldn't parse JSON",
						new Tag("Path", path),
						new Tag("Exception", e.Message));
				}
			}
			else
			{
				if (FileUtils.IsTextFile(path))
				{
					items.Add(new ListItem("Contents", new FilePath(path)));
				}
				else
				{
					items.Add(new ListItem("Bytes", new TabFileBytes(path)));
				}
			}
			items.Add(new ListItem("File Info", new FileInfo(path)));

			model.AddItems(items);
		}

		private void OpenFolder(Call call)
		{
			ProcessUtils.OpenFolder(tab.Path);
		}

		private void Delete(Call call)
		{
			if (File.Exists(tab.Path))
			{
				File.Delete(tab.Path);
			}

			Reload();
		}

		private void SelectFile(Call call)
		{
			SelectFileDelegate!(call, tab.Path);
		}
	}
}
