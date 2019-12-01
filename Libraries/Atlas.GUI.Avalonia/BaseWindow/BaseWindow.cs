using Avalonia;
using Avalonia.Controls;
using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.GUI.Avalonia.View;
using System;
using System.IO;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Atlas.GUI.Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;

namespace Atlas.GUI.Avalonia
{
	public class BaseWindow : Window
	{
		private const int MinWindowSize = 500;
		public static readonly int DefaultIncrementWidth = 1000; // should we also use a max percent?
		public static readonly int KeyboardIncrementWidth = 500; // should we also use a max percent?
		public static BaseWindow baseWindow;
		protected Linker linker = new Linker();

		public Project project;

		private bool loadComplete = false;
		private bool loaded = false;
		const string IsLoadingDataKey = "Loading";

		// Controls
		protected Grid containerGrid;
		protected BaseWindowToolbar toolbar;
		protected ScrollViewer scrollViewer;
		protected Grid contentGrid;
		public TabView tabView;

		public BaseWindow(Project project) : base()
		{
			baseWindow = this;
			LoadProject(project);
#if DEBUG
			this.AttachDevTools();
#endif
			this.Initialized += BaseWindow_Initialized;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride(availableSize); // can freeze
			if (loaded == false)
			{
				bool isLoading1 = project.DataApp.Load<bool>(IsLoadingDataKey, new Call());
				//project.DataApp.Save(name, tabView.);
				project.DataApp.Save(IsLoadingDataKey, false);
				bool isLoading2 = project.DataApp.Load<bool>(IsLoadingDataKey, new Call());
				//project.projectSettings.AutoLoad = true;
				loaded = true;
			}
			return size;
		}

		private void BaseWindow_Initialized(object sender, EventArgs e)
		{
		}

		public void LoadProject(Project project)
		{
			this.project = project;
			bool isLoading = project.DataApp.Load<bool>(IsLoadingDataKey, new Call());
			if (isLoading) // did the previous load succeed?
				project.userSettings.AutoLoad = false;

			project.DataApp.Save(IsLoadingDataKey, true);

			LoadWindowSettings();

			InitializeComponent();

			loadComplete = true;
		}

		// Load here instead of in xaml for better control
		private void InitializeComponent()
		{
			Title = project.projectSettings.Name ?? "<Name>";

			Background = new SolidColorBrush(Theme.BackgroundColor);

			Resources["FontSizeSmall"] = 14; // stop DatePicker using a small font size

			using (Stream stream = Icons.Streams.Logo)
			{
				Icon = new WindowIcon(stream);
			}

			// Toolbar      | Toolbar
			// ScrollViewer | Buttons
			containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("Auto,*"),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};

			toolbar = new BaseWindowToolbar(this);
			Grid.SetRow(toolbar, 0);
			containerGrid.Children.Add(toolbar);

			scrollViewer = new ScrollViewer()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
				VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
				//Background = new SolidColorBrush(Colors.Red),
				MaxWidth = 5000,
				MaxHeight = 4000,
				[Grid.RowProperty] = 1,
			};
			//scrollViewer.horizontalScrollBar

			//containerGrid.Children.Add(scrollViewer);

			// contains scroll viewer
			contentGrid = new Grid()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				//HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("*"),
				//Background = new SolidColorBrush(Colors.Blue),
				MaxWidth = 10000,
				MaxHeight = 5000,
			};

			scrollViewer.Content = contentGrid;

			SetMaxBounds();

			containerGrid.Children.Add(scrollViewer);

			Grid scrollButtons = CreateScrollButtons();

			containerGrid.Children.Add(scrollButtons);

			Content = containerGrid;

			this.PositionChanged += BaseWindow_PositionChanged;
		}

		public void Reload()
		{
			//LoadProject(project);
			//tabView.Load();
			tabView.tabInstance.Reload();
		}

		public void AddClipBoardButtons()
		{
			toolbar.AddClipBoardButtons();
			toolbar.buttonLink.Add(Link);
			toolbar.buttonImport.Add(ImportBookmark);
		}

		private void Link(Call call)
		{
			Bookmark bookmark = tabView.tabInstance.CreateBookmark();
			string uri = linker.GetLinkUri(bookmark);
			((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(uri);
		}

		private void ImportBookmark(Call call)
		{
			string clipboardText = ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync().Result;
			string data = linker.GetLinkData(clipboardText);
			if (data == null)
				return;
			Bookmark bookmark = Bookmark.Create(data);
			bool reloadBase = true;
			if (reloadBase)
			{
				tabView.tabInstance.tabBookmark = bookmark.tabBookmark;
				Reload();
			}
			else
			{
				// only if TabBookmarks used, don't need to reload the tab
				baseWindow.tabView.tabInstance.SelectBookmark(bookmark.tabBookmark);
			}
		}

		private Grid CreateScrollButtons()
		{
			Grid grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto"),
				RowDefinitions = new RowDefinitions("*,*"), // Expand, Collapse
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.ColumnProperty] = 1,
				[Grid.RowSpanProperty] = 2,
				//[Grid.RowProperty] = 1, // need to add a dummy rectangle otherwise?
			};

			Button buttonExpand = new Button()
			{
				Content = ">",
				Foreground = new SolidColorBrush(Theme.NotesButtonForegroundColor),
				Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				[ToolTip.ShowDelayProperty] = 5,
				[ToolTip.TipProperty] = "Scroll Right\n(-> button)",
				[Grid.RowProperty] = 0,
			};
			grid.Children.Add(buttonExpand);
			buttonExpand.Click += ButtonExpand_Click;
			buttonExpand.PointerEnter += Button_PointerEnter;
			buttonExpand.PointerLeave += Button_PointerLeave;

			Button buttonCollapse = new Button()
			{
				Content = "<",
				Foreground = new SolidColorBrush(Theme.NotesButtonForegroundColor),
				Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor),
				BorderBrush = new SolidColorBrush(Colors.Black),
				BorderThickness = new Thickness(1),
				[ToolTip.TipProperty] = "Scroll Left\n(<- button)",
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
			button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor);
			button.BorderBrush = button.Background;
		}

		private void ButtonExpand_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			ScrollRight(DefaultIncrementWidth);
		}

		private void ButtonCollapse_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			ScrollLeft(DefaultIncrementWidth);
		}

		private void ScrollLeft(int amount)
		{
			scrollViewer.Offset = new Vector(Math.Max(0.0, scrollViewer.Offset.X - amount), scrollViewer.Offset.Y);
			contentGrid.MinWidth = 0;
			//contentGrid.Width = 0;
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
		protected void AddTab(ITab tab)
		{
			TabInstance tabInstance = tab.Create();
			tabInstance.project = project;
			if (project.userSettings.AutoLoad) // did we load successfully last time?
				tabInstance.LoadDefaultBookmark();

			tabView = new TabView(tabInstance);
			tabView.tabModel.Name = project.Name;
			tabView.tabModel.Bookmarks = new BookmarkCollection(project);
			tabView.Load();

			//var tabBookmarks = new TabBookmarks(tabView);
			//contentGrid.Children.Add(tabView);

			//Grid.SetRow(tabView, 1);
			//containerGrid.Children.Add(tabView);

			//scrollViewer.Content = tabView;
			contentGrid.Children.Add(tabView);
		}

		// How to set the main Content
		protected void AddTabBookmarks(ITab iTab)
		{
			var tabBookmarks = new TabBookmarks(project, iTab, linker);
			AddTab(tabBookmarks);

			//contentGrid.Children.Add(tabBookmarks);

			//Grid.SetRow(tabView, 1);
			//containerGrid.Children.Add(tabView);

			//scrollViewer.Content = tabView;
			//contentGrid.Children.Add(tabView);
		}

		private void SetMaxBounds()
		{
			double maxWidth = 0;
			double maxHeight = 0;
			foreach (var screen in Screens.All)
			{
				maxWidth += screen.Bounds.Width;
				maxHeight = Math.Max(maxHeight, screen.WorkingArea.Height - 20);
			}
			this.MaxWidth = maxWidth;
			this.MaxHeight = maxHeight;
			//scrollViewer.MaxWidth = PlatformImpl.MaxClientSize.Width + 10;
			//scrollViewer.MaxHeight = PlatformImpl.MaxClientSize.Height + 10;
		}

		protected WindowSettings WindowSettings
		{
			get
			{
				bool maximized = (this.WindowState == WindowState.Maximized);
				Rect bounds = this.Bounds;
				if (maximized && this.TransformedBounds != null)
					bounds = this.TransformedBounds.Value.Bounds;
				WindowSettings windowSettings = new WindowSettings()
				{
					Maximized = maximized,
					Width = bounds.Width,
					Height = bounds.Height,
					Left = maximized ? bounds.Position.X : Position.X,
					Top = maximized ? bounds.Position.Y : Position.Y,
				};

				return windowSettings;
			}
			set
			{
				double left = Math.Max(-10, value.Left); // values can be negative
				double top = Math.Max(0, value.Top);

				// These are causing the window to be shifted down
				this.Position = new PixelPoint((int)left, (int)top);
				this.Width = Math.Max(MinWindowSize, value.Width);
				this.Height = Math.Max(MinWindowSize, value.Height);
				//this.Height = Math.Max(MinWindowSize, value.Height + 500); // reproduces black bar problem, not subtracting bottom toolbar for Height
				//Measure(Bounds.Size);
				this.WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
				//InvalidateArrange(); // these don't restore well and need another pass
				//InvalidateMeasure();
			}
		}

		protected void LoadWindowSettings()
		{
			WindowSettings windowSettings = project.DataApp.Load<WindowSettings>(true);

			this.WindowSettings = windowSettings;
		}

		// Still saving due to a HandleResized calls after IsActive (loadComplete does nothing)
		private void SaveWindowSettings()
		{
			if (loadComplete && IsArrangeValid && IsMeasureValid) // && IsActive (this can be false even after loading)
				project.DataApp.Save(this.WindowSettings);

			// need a better trigger for when the screen size changes
			SetMaxBounds();
		}

		// Avalonia missing Window move event or override so moving window doesn't update save
		protected override void HandleResized(Size clientSize)
		{
			base.HandleResized(clientSize);
			SaveWindowSettings();
		}

		protected override void HandleWindowStateChanged(WindowState state)
		{
			base.HandleWindowStateChanged(state);
			SaveWindowSettings();
		}

		// this fires too often, could attach a dispatch timer, or add an override method
		private void BaseWindow_PositionChanged(object sender, PixelPointEventArgs e)
		{
			SaveWindowSettings();
		}

		// don't allow the scroll viewer to jump back to the left while we're loading content and the content grid width is fluctuating
		public void SetMinScrollOffset()
		{
			contentGrid.MinWidth = scrollViewer.Offset.X + scrollViewer.Bounds.Size.Width;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			//if (e.Key == Key.F5)
			//	SelectSavedItems();

			if (e.Key == Key.Left)
			{
				ScrollLeft(KeyboardIncrementWidth);
				e.Handled = true;
				return;
			}

			if (e.Key == Key.Right)
			{
				ScrollRight(KeyboardIncrementWidth);
				e.Handled = true;
				return;
			}

			if (e.Key == Key.F5)
			{
				Reload();
				e.Handled = true;
				return;
			}

			if (e.Modifiers == InputModifiers.Control)
			{
			}
			else if (e.Key == Key.Escape)
			{
			}
		}
	}
}

/*
https://github.com/AvaloniaUI/Avalonia/wiki/Hide-console-window-for-self-contained-.NET-Core-application


*/
