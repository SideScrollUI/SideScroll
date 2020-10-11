using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Controls;
using Atlas.UI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace Atlas.UI.Avalonia
{
	public class TabViewer : Grid
	{
		public int DefaultIncrementWidth = 1000; // should we also use a max percent?
		public int KeyboardIncrementWidth = 500;

		public static TabViewer BaseViewer;
		public static string LoadBookmarkUri { get; set; }

		public Project Project { get; set; }

		public Linker Linker { get; set; } = new Linker();

		// Controls
		public TabViewerToolbar toolbar;
		protected Grid bottomGrid;
		protected ScrollViewer scrollViewer;
		protected Grid contentGrid;
		public TabView TabView;
		private ScreenCapture screenCapture;

		public TabViewer(Project project) : base()
		{
			BaseViewer = this;
			LoadProject(project);
		}

		public void LoadProject(Project project)
		{
			Project = project;

			InitializeComponent();
		}

		// Load here instead of in xaml for better control
		private void InitializeComponent()
		{
			Background = Theme.TabBackground;

			// Toolbar
			// ScrollViewer | Buttons
			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("Auto,*");
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			toolbar = new TabViewerToolbar(this);
			toolbar.ButtonLink.AddAsync(LinkAsync);
			toolbar.ButtonImport.AddAsync(ImportBookmarkAsync);
			toolbar.ButtonSnapshot?.Add(Snapshot);
			toolbar.ButtonSnapshotCancel?.Add(CloseSnapshot);
			Children.Add(toolbar);

			bottomGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("*"),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.RowProperty] = 1,
			};
			Children.Add(bottomGrid);

			// Placed inside scroll viewer
			contentGrid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Stretch,
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("*"),
				MaxWidth = 10000,
				MaxHeight = 5000,
			};

			scrollViewer = new ScrollViewer()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
				MaxWidth = 5000,
				MaxHeight = 4000,
				Content = contentGrid,
			};

			bottomGrid.Children.Add(scrollViewer);

			Grid scrollButtons = CreateScrollButtons();

			bottomGrid.Children.Add(scrollButtons);
		}

		public void Reload()
		{
			TabBookmarks.Global = null;
			TabView.Instance.Reload();
		}

		private async Task LinkAsync(Call call)
		{
			Bookmark bookmark = TabView.Instance.CreateBookmark();
			bookmark.TabBookmark = bookmark.TabBookmark.GetLeaf(); // Get the shallowest root node
			string uri = Linker.GetLinkUri(call, bookmark);
			await ClipBoardUtils.SetTextAsync(uri);
		}

		private async Task ImportBookmarkAsync(Call call)
		{
			string clipboardText = await ClipBoardUtils.GetTextAsync();
			ImportBookmark(call, clipboardText, true);
		}

		private Bookmark ImportBookmark(Call call, string linkUri, bool checkVersion)
		{
			Bookmark bookmark = Linker.GetBookmark(call, linkUri, checkVersion);
			if (bookmark == null)
				return null;

			if (TabBookmarks.Global != null)
			{
				// Add Bookmark to bookmark manager
				TabView.Instance.SelectItem(TabBookmarks.Global); // select bookmarks first so the child tab autoselects the new bookmark
				TabBookmarks.Global.AddBookmark(call, bookmark);
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

		private void Snapshot(Call call)
		{
			screenCapture = new ScreenCapture(scrollViewer)
			{
				[Grid.RowProperty] = 1,
			};
			toolbar.SetSnapshotVisible(true);

			Children.Remove(bottomGrid);
			Children.Add(screenCapture);
		}

		private void CloseSnapshot(Call call)
		{
			toolbar.SetSnapshotVisible(false);

			Children.Remove(screenCapture);
			Children.Add(bottomGrid);
		}

		private Grid CreateScrollButtons()
		{
			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("*,*"), // Expand, Collapse
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
			};

			var buttonExpand = new Button()
			{
				Content = ">",
				Background = Theme.ToolbarButtonBackground,
				Foreground = Theme.ToolbarTextForeground,
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				[ToolTip.ShowDelayProperty] = 5,
				[ToolTip.TipProperty] = "Scroll Right ( -> )",
				[Grid.RowProperty] = 0,
			};
			grid.Children.Add(buttonExpand);
			buttonExpand.Click += ButtonExpand_Click;
			buttonExpand.PointerEnter += Button_PointerEnter;
			buttonExpand.PointerLeave += Button_PointerLeave;

			var buttonCollapse = new Button()
			{
				Content = "<",
				Background = Theme.ToolbarButtonBackground,
				Foreground = Theme.ToolbarTextForeground,
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				[ToolTip.TipProperty] = "Scroll Left ( <- )",
				[Grid.RowProperty] = 1,
			};
			grid.Children.Add(buttonCollapse);
			buttonCollapse.Click += ButtonCollapse_Click;
			buttonCollapse.PointerEnter += Button_PointerEnter;
			buttonCollapse.PointerLeave += Button_PointerLeave;

			return grid;
		}

		private void Button_PointerEnter(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = Theme.ToolbarButtonBackgroundHover;
		}

		private void Button_PointerLeave(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = Theme.ToolbarButtonBackground;
		}

		private void ButtonExpand_Click(object sender, RoutedEventArgs e)
		{
			ScrollRight(DefaultIncrementWidth);
		}

		private void ButtonCollapse_Click(object sender, RoutedEventArgs e)
		{
			ScrollLeft(DefaultIncrementWidth);
		}

		private void ScrollLeft(int amount)
		{
			scrollViewer.Offset = new Vector(Math.Max(0.0, scrollViewer.Offset.X - amount), scrollViewer.Offset.Y);
			contentGrid.MinWidth = 0;
		}

		private void ScrollRight(int amount)
		{
			double minXOffset = scrollViewer.Offset.X + amount;
			double widthRequired = minXOffset + scrollViewer.Viewport.Width;
			contentGrid.MinWidth = widthRequired;
			contentGrid.Width = widthRequired;

			// Force the ScrollViewer to update it's ViewPort so we can set an offset past the old bounds
			Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);

			scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
			scrollViewer.Offset = new Vector(minXOffset, scrollViewer.Offset.Y);
		}

		// How to set the main Content
		public void AddTab(ITab tab)
		{
			TabInstance tabInstance = tab.Create();
			tabInstance.Model.Name = "Start";
			tabInstance.iTab = tab;
			tabInstance.Project = Project;
			if (LoadBookmarkUri != null)
			{
				// Wait until Bookmarks tab has been created
				Dispatcher.UIThread.Post(() => ImportBookmark(new Call(), LoadBookmarkUri, false), DispatcherPriority.SystemIdle);
			}
			else if (Project.UserSettings.AutoLoad) // did we load successfully last time?
			{
				tabInstance.LoadDefaultBookmark();
			}

			TabView = new TabView(tabInstance);
			TabView.Load();

			//scrollViewer.Content = tabView;
			contentGrid.Children.Add(TabView);
		}

		// don't allow the scroll viewer to jump back to the left while we're loading content and the content grid width is fluctuating
		public void SetMinScrollOffset()
		{
			contentGrid.MinWidth = scrollViewer.Offset.X + scrollViewer.Bounds.Size.Width;
		}

		public void SeekBackward()
		{
			Bookmark bookmark = Project.Navigator.SeekBackward();
			if (bookmark != null)
				TabView.Instance.SelectBookmark(bookmark.TabBookmark);
		}

		public void SeekForward()
		{
			Bookmark bookmark = Project.Navigator.SeekForward();
			if (bookmark != null)
				TabView.Instance.SelectBookmark(bookmark.TabBookmark);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Key == Key.Left)
			{
				if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
					SeekBackward();
				else
					ScrollLeft(KeyboardIncrementWidth);
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Right)
			{
				if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
					SeekForward();
				else
					ScrollRight(KeyboardIncrementWidth);
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
	}
}