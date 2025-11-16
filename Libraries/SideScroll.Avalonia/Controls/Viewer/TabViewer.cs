using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.View;
using SideScroll.Avalonia.Utilities;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Controls.Viewer;

public class TabLoadedEventArgs(object obj) : EventArgs
{
	public object Object => obj;
}

public interface ITabViewerPlugin
{
	public void Initialize(TabViewer tabViewer);
}

public class TabViewer : Grid
{
	public int MaxScrollWidth { get; set; } = 1000; // should we also use a max percent?
	public double ScrollPercent { get; set; } = 0.5;
	public int DefaultScrollWidth => Math.Min(MaxScrollWidth, (int)(ScrollViewer.Viewport.Width * ScrollPercent));
	public int KeyboardScrollWidth { get; set; } = 500;

	public static TabViewer? BaseViewer { get; set; }
	public static string? LoadLinkUri { get; set; }
	public static Bookmark? LoadBookmark { get; set; }
	public static List<ITabViewerPlugin> Plugins { get; set; } = [];

	public Project Project { get; set; }

	// Controls
	public TabViewerToolbar? Toolbar { get; protected set; }
	protected Grid BottomGrid { get; set; }
	public ScrollViewer ScrollViewer { get; protected set; }
	public Grid ContentGrid { get; protected set; }
	public TabView? TabView { get; protected set; }

	public Control? ContentControl { get; protected set; }

	public event EventHandler<TabLoadedEventArgs>? OnTabLoaded;

	public TabViewer(Project project)
	{
		BaseViewer = this;

		// Toolbar
		// ScrollViewer | Buttons
		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto,*");

		BottomGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			RowDefinitions = new RowDefinitions("*"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			[RowProperty] = 1,
		};
		Children.Add(BottomGrid);

		// Placed inside scroll viewer
		ContentGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("Auto"),
			RowDefinitions = new RowDefinitions("*,16"),
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Stretch,
			MaxWidth = 10000,
			MaxHeight = 5000,
		};

		ScrollViewer = new ScrollViewer
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
			BringIntoViewOnFocusChange = false, // Doesn't work well with Tab GridSplitters
			MaxWidth = 5000,
			MaxHeight = 4000,
			Content = ContentGrid,
		};

		BottomGrid.Children.Add(ScrollViewer);

		Grid scrollButtons = CreateScrollButtons();

		BottomGrid.Children.Add(scrollButtons);

		LoadProject(project);
	}

	[MemberNotNull(nameof(Project))]
	public void LoadProject(Project project)
	{
		Project = project;

		AddToolbar();

		Plugins.ForEach(plugin => plugin.Initialize(this));
	}

	private void AddToolbar()
	{
		if (Project.ProjectSettings.ShowToolbar == false)
			return;

		Toolbar = new TabViewerToolbar(this);
		Toolbar.ButtonLink?.AddAsync(CreateLinkAsync);
		Toolbar.ButtonImport?.AddAsync(ImportClipboardLinkAsync);
		Children.Add(Toolbar);
	}

	public void Reload(Call? call = null)
	{
		call ??= new();
		LinkManager.Instance?.Reload(call);
		TabView!.Instance.Reload();
	}

	private async Task CreateLinkAsync(Call call)
	{
		var buttonLink = Toolbar!.ButtonLink!;

		Flyout flyout = new()
		{
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		AvaloniaUtils.ShowFlyout(buttonLink, flyout, "Creating Link ...");

		Bookmark bookmark = TabView!.Instance.CreateBookmark();
		TabBookmark? leafNode = bookmark.TabBookmark.GetLeaf(); // Get the shallowest root node
		if (leafNode != bookmark.TabBookmark)
		{
			bookmark.Name = leafNode!.Tab?.ToString();
			bookmark.TabBookmark = leafNode;
			bookmark.BookmarkType = BookmarkType.Leaf;
		}
		else
		{
			bookmark.BookmarkType = BookmarkType.Full;
		}

		try
		{
			LinkUri linkUri = await Project.Linker.AddLinkAsync(call, bookmark);

			LinkManager.Instance?.Created.AddNew(call, linkUri, bookmark);
			await ClipboardUtils.SetTextAsync(this, linkUri.ToString());
			AvaloniaUtils.ShowFlyout(buttonLink, flyout, "Link copied to clipboard");
		}
		catch (Exception ex)
		{
			AvaloniaUtils.ShowFlyout(buttonLink, flyout, ex.Message);
		}
	}

	private async Task ImportClipboardLinkAsync(Call call)
	{
		string? clipboardText = await ClipboardUtils.TryGetTextAsync(this);
		if (clipboardText == null) return;

		if (LinkUri.TryParse(clipboardText, out LinkUri? linkUri))
		{
			await ImportLinkAsync(call, linkUri, true);
		}
		else
		{
			Flyout flyout = new()
			{
				Placement = PlacementMode.BottomEdgeAlignedLeft,
			};
			AvaloniaUtils.ShowFlyout(Toolbar!.ButtonImport!, flyout, "Failed to parse Clipboard text");
		}
	}

	public async Task<Bookmark?> ImportLinkAsync(Call call, LinkUri linkUri, bool checkVersion)
	{
		var buttonImport = Toolbar!.ButtonImport!;

		Flyout flyout = new()
		{
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		AvaloniaUtils.ShowFlyout(buttonImport, flyout, "Importing Link ...");

		try
		{
			Bookmark bookmark = await Project.Linker.GetLinkAsync(call, linkUri, checkVersion);
			if (bookmark == null)
				return null;

			AvaloniaUtils.ShowFlyout(buttonImport, flyout, "Link retrieved, importing");

			ImportBookmark(call, linkUri, bookmark);

			AvaloniaUtils.ShowFlyout(buttonImport, flyout, "Link imported");

			return bookmark;
		}
		catch (Exception ex)
		{
			AvaloniaUtils.ShowFlyout(buttonImport, flyout, ex.Message);
			return null;
		}
	}

	private void ImportBookmark(Call call, LinkUri linkUri, Bookmark bookmark)
	{
		if (bookmark == null)
			return;

		if (LinkManager.Instance != null && bookmark.Imported)
		{
			// Add Bookmark to Link Manager
			TabView!.Instance.SelectPath("Links", "Imported"); // Select path first so the child tab autoselects the new bookmark
			LinkManager.Instance.Imported.AddNew(call, linkUri, bookmark);
			ScrollViewer.Offset = new Vector(0, 0);
		}
		else if (TabView != null)
		{
			// Load bookmark on top of everything (how navigation works)
			bool reloadBase = true;
			if (reloadBase)
			{
				TabView.Instance.TabBookmark = bookmark.TabBookmark;
				Reload(call);
			}
			else
			{
				// only if TabBookmarks used, don't need to reload the tab
				TabView.Instance.SelectBookmark(bookmark.TabBookmark);
			}
		}
	}

	public void SelectBookmark(TabBookmark tabBookmark, bool reload)
	{
		if (reload)
		{
			ScrollViewer.Offset = new Vector(0, 0);
			TabView!.Focus();
		}
		TabView!.Instance.SelectBookmark(tabBookmark, reload);
	}

	public void SetContent(Control control)
	{
		ClearContent();

		ContentControl = control;

		SetRowSpan(control, 2);

		Children.Remove(BottomGrid);
		Children.Add(control);
	}

	public void ClearContent()
	{
		if (ContentControl == null)
			return;

		Children.Remove(ContentControl);
		if (BottomGrid.Parent == null)
		{
			Children.Add(BottomGrid);
		}
	}

	private Grid CreateScrollButtons()
	{
		Grid grid = new()
		{
			ColumnDefinitions = new ColumnDefinitions("Auto"),
			RowDefinitions = new RowDefinitions("*,*"), // Expand, Collapse
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			[ColumnProperty] = 1,
		};

		TabButton buttonScrollRight = new()
		{
			Content = ">",
			[ToolTip.ShowDelayProperty] = 5,
			[ToolTip.TipProperty] = "Scroll Right ( -> )",
			[RowProperty] = 0,
		};
		buttonScrollRight.Click += ButtonExpand_Click;
		grid.Children.Add(buttonScrollRight);

		TabButton buttonScrollLeft = new()
		{
			Content = "<",
			[ToolTip.TipProperty] = "Scroll Left ( <- )",
			[RowProperty] = 1,
		};
		buttonScrollLeft.Click += ButtonCollapse_Click;
		grid.Children.Add(buttonScrollLeft);

		return grid;
	}

	private void ButtonExpand_Click(object? sender, RoutedEventArgs e)
	{
		ScrollRight(DefaultScrollWidth);
	}

	private void ButtonCollapse_Click(object? sender, RoutedEventArgs e)
	{
		ScrollLeft(DefaultScrollWidth);
	}

	private void ScrollLeft(int amount)
	{
		ScrollViewer.Offset = new Vector(Math.Max(0.0, ScrollViewer.Offset.X - amount), ScrollViewer.Offset.Y);
		ContentGrid.MinWidth = 0;
	}

	private void ScrollRight(int amount)
	{
		double minXOffset = ScrollViewer.Offset.X + amount;
		double widthRequired = minXOffset + ScrollViewer.Viewport.Width;
		ContentGrid.MinWidth = widthRequired;

		// Force the ScrollViewer to update it's ViewPort so we can set an offset past the old bounds
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

		ScrollViewer.Offset = new Vector(minXOffset, ScrollViewer.Offset.Y);
	}

	// Load the main Tab Content
	public TabInstance LoadTab(ITab tab)
	{
		TabInstance tabInstance = tab.Create();
		tabInstance.Model.Name = "Start";
		tabInstance.iTab = tab;
		tabInstance.Project = Project;

		if (LoadLinkUri != null)
		{
			// Wait until Bookmarks tab has been created
			if (LinkUri.TryParse(LoadLinkUri, out LinkUri? linkUri))
			{
				Dispatcher.UIThread.Post(async () => await ImportLinkAsync(new Call(), linkUri, false), DispatcherPriority.SystemIdle);
			}
		}
		else if (LoadBookmark != null)
		{
			tabInstance.TabBookmark = LoadBookmark.TabBookmark;
		}
		else if (Project.UserSettings.AutoSelect)
		{
			tabInstance.LoadDefaultBookmark();
		}

		TabView = new TabView(tabInstance);
		TabView.Load();

		ContentGrid.Children.Add(TabView);

		return tabInstance;
	}

	// don't allow the scroll viewer to jump back to the left while we're loading content and the content grid width is fluctuating
	public void SetMinScrollOffset()
	{
		ContentGrid.MinWidth = ScrollViewer.Offset.X + ScrollViewer.Bounds.Size.Width;
	}

	public void SeekBackward()
	{
		if (Project.Navigator.SeekBackward() is Bookmark bookmark)
		{
			TabView!.Instance.SelectBookmark(bookmark.TabBookmark);
		}
	}

	public void SeekForward()
	{
		if (Project.Navigator.SeekForward() is Bookmark bookmark)
		{
			TabView!.Instance.SelectBookmark(bookmark.TabBookmark);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.Key == Key.Left)
		{
			if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
			{
				SeekBackward();
			}
			else
			{
				ScrollLeft(KeyboardScrollWidth);
			}
			e.Handled = true;
			return;
		}

		if (e.Key == Key.Right)
		{
			if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
			{
				SeekForward();
			}
			else
			{
				ScrollRight(KeyboardScrollWidth);
			}
			e.Handled = true;
			return;
		}

		if (e.KeyModifiers == KeyModifiers.Control)
		{
			if (e.Key == Key.R)
			{
				Reload();
				e.Handled = true;
				return;
			}
		}
	}

	internal void TabLoaded(object obj, Control control)
	{
		OnTabLoaded?.Invoke(control, new TabLoadedEventArgs(obj));
	}
}
