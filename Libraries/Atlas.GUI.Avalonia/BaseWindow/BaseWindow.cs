﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Atlas.GUI.Avalonia.Controls;
using Atlas.GUI.Avalonia.View;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Atlas.GUI.Avalonia
{
	public class BaseWindow : Window
	{
		private const int MinWindowSize = 500;
		public static readonly int DefaultIncrementWidth = 1000; // should we also use a max percent?
		public static readonly int KeyboardIncrementWidth = 500;
		public static BaseWindow baseWindow;
		protected Linker linker = new Linker();

		public Project project;

		private bool loadComplete = false;
		private bool loaded = false;
		const string IsLoadingDataKey = "Loading";

		// Controls
		protected Grid containerGrid;
		protected Grid bottomGrid;
		protected BaseWindowToolbar toolbar;
		protected ScrollViewer scrollViewer;
		protected Grid contentGrid;
		public TabView tabView;

		public static string LoadBookmarkUri { get; set; }

		public BaseWindow(Project project) : base()
		{
			baseWindow = this;
			LoadProject(project);
#if DEBUG
			this.AttachDevTools();
#endif
			Closed += BaseWindow_Closed;
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

		public void LoadProject(Project project)
		{
			this.project = project;
			//bool isLoading = project.DataApp.Load<bool>(IsLoadingDataKey, new Call());
			//if (isLoading) // did the previous load succeed?
			//	project.userSettings.AutoLoad = false;

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

			// Toolbar
			// ScrollViewer | Buttons
			containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*"),
				RowDefinitions = new RowDefinitions("Auto,*"),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};

			toolbar = new BaseWindowToolbar(this);
			toolbar.buttonLink.Add(Link);
			toolbar.buttonImport.Add(ImportBookmark);
			toolbar.buttonSnapshot?.Add(Snapshot);
			containerGrid.Children.Add(toolbar);

			bottomGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("*"),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				[Grid.RowProperty] = 1,
			};
			containerGrid.Children.Add(bottomGrid);

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

			Content = containerGrid;

			PositionChanged += BaseWindow_PositionChanged;

			this.GetObservable(ClientSizeProperty).Subscribe(Resize);
		}

		private void Resize(Size size)
		{
			SaveWindowSettings();
		}

		public void Reload()
		{
			//LoadProject(project);
			//tabView.Load();
			tabView.tabInstance.Reload();
		}

		private void Link(Call call)
		{
			Bookmark bookmark = tabView.tabInstance.CreateBookmark();
			string uri = linker.GetLinkUri(bookmark);
			((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).SetTextAsync(uri);
		}

		private void ImportBookmark(Call call)
		{
			string clipboardText = ((IClipboard)AvaloniaLocator.Current.GetService(typeof(IClipboard))).GetTextAsync().GetAwaiter().GetResult();
			Bookmark bookmark = linker.GetBookmark(clipboardText);
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

		private void Snapshot(Call call)
		{
			var screenCapture = new ScreenCapture(scrollViewer)
			{
				[Grid.RowProperty] = 1,
			};

			containerGrid.Children.Add(screenCapture);
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
			};

			Button buttonExpand = new Button()
			{
				Content = ">",
				Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				Foreground = new SolidColorBrush(Theme.ToolbarTextForegroundColor),
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
				Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor),
				Foreground = new SolidColorBrush(Theme.ToolbarTextForegroundColor),
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
			button.Background = new SolidColorBrush(Color.Parse("#4e8ef7"));
		}

		private void Button_PointerLeave(object sender, PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundColor);
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
		protected void AddTab(ITab tab)
		{
			TabInstance tabInstance = tab.Create();
			tabInstance.project = project;
			if (LoadBookmarkUri != null)
				tabInstance.tabBookmark = linker.GetBookmark(LoadBookmarkUri)?.tabBookmark;
			else if (project.userSettings.AutoLoad) // did we load successfully last time?
				tabInstance.LoadDefaultBookmark();

			tabView = new TabView(tabInstance);
			tabView.tabModel.Name = "Start";
			tabView.tabModel.Bookmarks = new BookmarkCollection(project);
			tabView.Load();

			//var tabBookmarks = new TabBookmarks(tabView);
			//contentGrid.Children.Add(tabView);

			//Grid.SetRow(tabView, 1);
			//containerGrid.Children.Add(tabView);

			//scrollViewer.Content = tabView;
			contentGrid.Children.Add(tabView);
		}

		// How to set the main Content?
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
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
			//scrollViewer.MaxWidth = PlatformImpl.MaxClientSize.Width + 10;
			//scrollViewer.MaxHeight = PlatformImpl.MaxClientSize.Height + 10;
		}

		protected WindowSettings WindowSettings
		{
			get
			{
				bool maximized = (this.WindowState == WindowState.Maximized);
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) // Avalonia bug? WindowState doesn't update correctly for MacOS
					maximized = false;
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
				// These are causing the window to be shifted down
				Width = Math.Max(MinWindowSize, value.Width);
				Height = Math.Max(MinWindowSize, value.Height);

				double minLeft = -10; // Left position for windows starts at -10
				double left = Math.Min(Math.Max(minLeft, value.Left), MaxWidth - Width + minLeft); // values can be negative
				double top = Math.Min(Math.Max(0, value.Top), MaxHeight - Height);
				Position = new PixelPoint((int)left, (int)top);
				//Height = Math.Max(MinWindowSize, value.Height + 500); // reproduces black bar problem, not subtracting bottom toolbar for Height
				//Measure(Bounds.Size);
				WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}

		protected void LoadWindowSettings()
		{
			SetMaxBounds();

			WindowSettings windowSettings = project.DataApp.Load<WindowSettings>(true);

			this.WindowSettings = windowSettings;
		}

		// Still saving due to a HandleResized calls after IsActive (loadComplete does nothing)
		private void SaveWindowSettings()
		{
			if (loadComplete)// && IsArrangeValid && IsMeasureValid) // && IsActive (this can be false even after loading)
				project.DataApp.Save(WindowSettings);

			// need a better trigger for when the screen size changes
			SetMaxBounds();
		}

		private void BaseWindow_Closed(object sender, EventArgs e)
		{
			// todo: split saving position out
			//project.DataApp.Save(WindowSettings);
			//SaveWindowSettings();
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

			if (e.KeyModifiers == KeyModifiers.Control)
			{
				if (e.Key == Key.R)
				{
					Reload();
					e.Handled = true;
					return;
				}
			}
			else if (e.Key == Key.Escape)
			{
			}
		}
	}
}