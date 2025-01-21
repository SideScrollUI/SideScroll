using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Utilities;
using SideScroll.Avalonia.View;
using SideScroll.Tabs;
using SideScroll.Tabs.Bookmarks;
using System.Diagnostics.CodeAnalysis;

namespace SideScroll.Avalonia.Viewer;

public class EventTabLoaded(object obj) : EventArgs
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

	public event EventHandler<EventTabLoaded>? OnTabLoaded;

	public TabViewer(Project project)
	{
		BaseViewer = this;
		LoadProject(project);
	}

	[MemberNotNull(nameof(Project)), MemberNotNull(nameof(BottomGrid)), MemberNotNull(nameof(ScrollViewer)), MemberNotNull(nameof(ContentGrid))]
	public void LoadProject(Project project)
	{
		Project = project;

		InitializeComponent();
	}

	[MemberNotNull(nameof(BottomGrid)), MemberNotNull(nameof(ScrollViewer)), MemberNotNull(nameof(ContentGrid))]
	private void InitializeComponent()
	{
		// Toolbar
		// ScrollViewer | Buttons
		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto,*");

		AddToolbar();

		BottomGrid = new Grid
		{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			RowDefinitions = new RowDefinitions("*"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			[Grid.RowProperty] = 1,
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

		Plugins.ForEach(plugin => plugin.Initialize(this));
	}

	private void AddToolbar()
	{
		if (Project.ProjectSettings.ShowToolbar == false)
			return;

		Toolbar = new TabViewerToolbar(this);
		Toolbar.ButtonLink?.AddAsync(LinkAsync);
		Toolbar.ButtonImport?.AddAsync(ImportClipboardLinkAsync);
		Children.Add(Toolbar);
	}

	public void Reload(Call? call = null)
	{
		call ??= new();
		BookmarkManager.Instance?.Reload(call);
		TabView!.Instance.Reload();
	}

	private async Task LinkAsync(Call call)
	{
		Flyout flyout = new()
		{
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		AvaloniaUtils.ShowFlyout(Toolbar!.ButtonLink!, flyout, "Creating Link ...");

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
			string? uri = await Project.Linker.AddLinkAsync(call, bookmark);
			if (uri == null)
				return;

			BookmarkManager.Instance?.Created.AddNew(call, bookmark);
			await ClipboardUtils.SetTextAsync(this, uri);
			AvaloniaUtils.ShowFlyout(Toolbar!.ButtonLink!, flyout, "Link copied to clipboard");
		}
		catch (Exception ex)
		{
			AvaloniaUtils.ShowFlyout(Toolbar!.ButtonLink!, flyout, ex.Message);
		}
	}

	private async Task ImportClipboardLinkAsync(Call call)
	{
		string? clipboardText = await ClipboardUtils.GetTextAsync(this);
		if (clipboardText == null) return;

		if (LinkUri.TryParse(clipboardText, out LinkUri? linkUri))
		{
			await ImportLinkAsync(call, linkUri, true);
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

			ImportBookmark(call, bookmark);

			AvaloniaUtils.ShowFlyout(buttonImport, flyout, "Link imported");

			return bookmark;
		}
		catch (Exception ex)
		{
			AvaloniaUtils.ShowFlyout(buttonImport, flyout, ex.Message);
			return null;
		}
	}

	public Bookmark? ImportLink(Call call, LinkUri linkUri, bool checkVersion)
	{
		Flyout flyout = new()
		{
			Content = "Importing Link ...",
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		flyout.ShowAt(Toolbar!.ButtonImport!);

		try
		{
			Bookmark bookmark = Task.Run(() => Project.Linker.GetLinkAsync(call, linkUri, checkVersion)).GetAwaiter().GetResult();
			if (bookmark == null)
				return null;

			flyout.Content = "Link retrieved, importing";

			ImportBookmark(call, bookmark);

			flyout.Content = "Link imported";

			return bookmark;
		}
		catch (Exception ex)
		{
			flyout.Content = ex.Message;
			return null;
		}
	}

	private void ImportBookmark(Call call, Bookmark bookmark)
	{
		if (bookmark == null)
			return;

		if (BookmarkManager.Instance != null && bookmark.Imported)
		{
			// Add Bookmark to bookmark manager
			TabView!.Instance.SelectPath("Links", "Imported"); // Select path first so the child tab autoselects the new bookmark
			BookmarkManager.Instance.Imported.AddNew(call, bookmark);
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

		Grid.SetRowSpan(control, 2);

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
			[Grid.ColumnProperty] = 1,
		};

		TabControlButton buttonScrollRight = new()
		{
			Content = ">",
			[ToolTip.ShowDelayProperty] = 5,
			[ToolTip.TipProperty] = "Scroll Right ( -> )",
			[Grid.RowProperty] = 0,
		};
		buttonScrollRight.Click += ButtonExpand_Click;
		grid.Children.Add(buttonScrollRight);

		TabControlButton buttonScrollLeft = new()
		{
			Content = "<",
			[ToolTip.TipProperty] = "Scroll Left ( <- )",
			[Grid.RowProperty] = 1,
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

	// How to set the main Content
	public TabInstance AddTab(ITab tab)
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
				Dispatcher.UIThread.Post(() => ImportLink(new Call(), linkUri, false), DispatcherPriority.SystemIdle);
				//Dispatcher.UIThread.InvokeAsync(() => ImportLinkAsync(new Call(), LoadLinkUri, false), DispatcherPriority.SystemIdle).GetAwaiter().GetResult();
			}
		}
		else if (LoadBookmark != null)
		{
			tabInstance.TabBookmark = LoadBookmark.TabBookmark;
		}
		else if (Project.UserSettings.AutoLoad) // did we load successfully last time?
		{
			tabInstance.LoadDefaultBookmark();
		}

		TabView = new TabView(tabInstance);
		TabView.Load();

		//scrollViewer.Content = tabView;
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
		OnTabLoaded?.Invoke(control, new EventTabLoaded(obj));
	}
}
