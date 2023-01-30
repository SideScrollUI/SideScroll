using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Atlas.UI.Avalonia;

public class EventTabLoaded : EventArgs
{
	public readonly object Object;

	public EventTabLoaded(object obj)
	{
		Object = obj;
	}
}

public class TabViewer : Grid
{
	public int MaxScrollWidth = 1000; // should we also use a max percent?
	public double ScrollPercent = 0.5;
	public int DefaultScrollWidth => Math.Min(MaxScrollWidth, (int)(ScrollViewer.Viewport.Width * ScrollPercent));
	public int KeyboardScrollWidth = 500;

	public static TabViewer? BaseViewer;
	public static string? LoadBookmarkUri { get; set; }
	public static Bookmark? LoadBookmark { get; set; }

	public Project Project { get; set; }

	// Controls
	public TabViewerToolbar? Toolbar;
	protected Grid BottomGrid;
	public ScrollViewer ScrollViewer;
	public Grid ContentGrid;
	public TabView? TabView;

	public Control? ContentControl;

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
		Background = Theme.TabBackground;

		// Toolbar
		// ScrollViewer | Buttons
		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto,*");

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

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
			RowDefinitions = new RowDefinitions("*"),
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
			Padding = new Thickness(0, 0, 0, 16),
			MaxWidth = 5000,
			MaxHeight = 4000,
			Content = ContentGrid,
		};

		BottomGrid.Children.Add(ScrollViewer);

		Grid scrollButtons = CreateScrollButtons();

		BottomGrid.Children.Add(scrollButtons);
	}

	private void AddToolbar()
	{
		if (Project.ProjectSettings.ShowToolbar == false)
			return;

		Toolbar = new TabViewerToolbar(this);
		Toolbar.ButtonLink?.AddAsync(LinkAsync);
		Toolbar.ButtonImport?.AddAsync(ImportClipboardBookmarkAsync);
		Children.Add(Toolbar);
	}

	public void Reload()
	{
		TabBookmarks.Global = null;
		TabView!.Instance.Reload();
	}

	private async Task LinkAsync(Call call)
	{
		Bookmark bookmark = TabView!.Instance.CreateBookmark();
		TabBookmark? leafNode = bookmark!.TabBookmark.GetLeaf(); // Get the shallowest root node
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

		string? uri = await Project.Linker.GetLinkUriAsync(call, bookmark);
		if (uri == null)
			return;

		await ClipBoardUtils.SetTextAsync(uri);
	}

	private async Task ImportClipboardBookmarkAsync(Call call)
	{
		string clipboardText = await ClipBoardUtils.GetTextAsync();
		await ImportBookmarkAsync(call, clipboardText, true);
	}

	private async Task<Bookmark?> ImportBookmarkAsync(Call call, string linkUri, bool checkVersion)
	{
		Bookmark? bookmark = await Project.Linker.GetBookmarkAsync(call, linkUri, checkVersion);
		if (bookmark == null)
			return null;

		return ImportBookmark(call, bookmark);
	}

	private Bookmark? ImportBookmark(Call call, string linkUri, bool checkVersion)
	{
		Bookmark? bookmark = Task.Run(() => Project.Linker.GetBookmarkAsync(call, linkUri, checkVersion)).GetAwaiter().GetResult();
		if (bookmark == null)
			return null;

		return ImportBookmark(call, bookmark);
	}

	private Bookmark? ImportBookmark(Call call, Bookmark? bookmark)
	{
		if (bookmark == null)
			return null;

		if (TabBookmarks.Global != null)
		{
			// Add Bookmark to bookmark manager
			TabView!.Instance.SelectItem(TabBookmarks.Global); // select bookmarks first so the child tab autoselects the new bookmark
			TabBookmarks.Global.AddBookmark(call, bookmark);
			ScrollViewer.Offset = new Vector(0, 0);
		}
		else if (TabView != null)
		{
			// Load bookmark on top of everything (how navigation works)
			bool reloadBase = true;
			if (reloadBase)
			{
				TabView.Instance.TabBookmark = bookmark.TabBookmark;
				Reload();
			}
			else
			{
				// only if TabBookmarks used, don't need to reload the tab
				TabView.Instance.SelectBookmark(bookmark.TabBookmark);
			}
		}
		return bookmark;
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
			Children.Add(BottomGrid);
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

		TabControlButton buttonExpand = new()
		{
			Content = ">",
			Background = Theme.ToolbarButtonBackground,
			Foreground = Theme.ToolbarLabelForeground,
			BorderBrush = new SolidColorBrush(Colors.Black),
			BorderThickness = new Thickness(1),
			VerticalAlignment = VerticalAlignment.Stretch,
			VerticalContentAlignment = VerticalAlignment.Center,
			[ToolTip.ShowDelayProperty] = 5,
			[ToolTip.TipProperty] = "Scroll Right ( -> )",
			[Grid.RowProperty] = 0,
		};
		buttonExpand.Click += ButtonExpand_Click;
		grid.Children.Add(buttonExpand);

		TabControlButton buttonCollapse = new()
		{
			Content = "<",
			Background = Theme.ToolbarButtonBackground,
			Foreground = Theme.ToolbarLabelForeground,
			BorderBrush = new SolidColorBrush(Colors.Black),
			BorderThickness = new Thickness(1),
			VerticalAlignment = VerticalAlignment.Stretch,
			VerticalContentAlignment = VerticalAlignment.Center,
			[ToolTip.TipProperty] = "Scroll Left ( <- )",
			[Grid.RowProperty] = 1,
		};
		buttonCollapse.Click += ButtonCollapse_Click;
		grid.Children.Add(buttonCollapse);

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
		ContentGrid.Width = widthRequired;

		// Force the ScrollViewer to update it's ViewPort so we can set an offset past the old bounds
		Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

		ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
		ScrollViewer.Offset = new Vector(minXOffset, ScrollViewer.Offset.Y);
	}

	// How to set the main Content
	public TabInstance AddTab(ITab tab)
	{
		TabInstance tabInstance = tab.Create();
		tabInstance.Model.Name = "Start";
		tabInstance.iTab = tab;
		tabInstance.Project = Project;

		if (LoadBookmarkUri != null)
		{
			// Wait until Bookmarks tab has been created
			Dispatcher.UIThread.Post(() => ImportBookmark(new Call(), LoadBookmarkUri, false), DispatcherPriority.SystemIdle);
			//Dispatcher.UIThread.InvokeAsync(() => ImportBookmarkAsync(new Call(), LoadBookmarkUri, false), DispatcherPriority.SystemIdle).GetAwaiter().GetResult();
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
		Bookmark? bookmark = Project.Navigator.SeekBackward();
		if (bookmark != null)
			TabView!.Instance.SelectBookmark(bookmark.TabBookmark);
	}

	public void SeekForward()
	{
		Bookmark? bookmark = Project.Navigator.SeekForward();
		if (bookmark != null)
			TabView!.Instance.SelectBookmark(bookmark.TabBookmark);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		if (e.Key == Key.Left)
		{
			if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
				SeekBackward();
			else
				ScrollLeft(KeyboardScrollWidth);
			e.Handled = true;
			return;
		}

		if (e.Key == Key.Right)
		{
			if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
				SeekForward();
			else
				ScrollRight(KeyboardScrollWidth);
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
