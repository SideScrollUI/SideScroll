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

namespace Atlas.GUI.Avalonia
{
	public class BaseWindow : Window
	{
		private const int MinWindowSize = 500;
		public static readonly int DefaultIncrementWidth = 1000; // should we also use a max percent?
		public static BaseWindow baseWindow;

		public Project project;

		private bool loadComplete = false;
		private bool loaded = false;
		const string IsLoadingDataKey = "Loading";

		// Controls
		private Grid containerGrid;
		private ScrollViewer scrollViewer;
		private Grid contentGrid;
		public TabView tabView;

		public BaseWindow() : base()
		{
			baseWindow = this;
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
				project.projectSettings.AutoLoad = false;

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

			using (Stream stream = Assets.Streams.Logo)
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

			BaseWindowToolbar toolbar = new BaseWindowToolbar(this);

			Grid.SetRow(toolbar, 0);
			containerGrid.Children.Add(toolbar);

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

			SetMaxBounds();

			scrollViewer.Content = contentGrid;

			containerGrid.Children.Add(scrollViewer);

			Grid scrollButtons = CreateScrollButtons();

			containerGrid.Children.Add(scrollButtons);

			Content = containerGrid;

			this.PositionChanged += BaseWindow_PositionChanged;
		}

		private void SetMaxBounds()
		{
			double maxWidth = 0;
			double maxHeight = 0;
			foreach (var screen in Screens.All)
			{
				maxWidth += screen.Bounds.Width;
				maxHeight = Math.Max(maxHeight, screen.Bounds.Height);
			}
			this.MaxWidth = maxWidth + 10;
			this.MaxHeight = maxHeight + 10;
			//scrollViewer.MaxWidth = PlatformImpl.MaxClientSize.Width + 10;
			//scrollViewer.MaxHeight = PlatformImpl.MaxClientSize.Height + 10;
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
				[Grid.RowProperty] = 1,
			};
			grid.Children.Add(buttonCollapse);
			buttonCollapse.Click += ButtonCollapse_Click;
			buttonCollapse.PointerEnter += Button_PointerEnter;
			buttonCollapse.PointerLeave += Button_PointerLeave;

			return grid;
		}

		private void Button_PointerEnter(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
			button.Background = new SolidColorBrush(Theme.ToolbarButtonBackgroundHoverColor);
		}

		private void Button_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e)
		{
			Button button = (Button)sender;
			button.Background = new SolidColorBrush(Theme.NotesButtonBackgroundColor);
			button.BorderBrush = button.Background;
		}

		private void ButtonExpand_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			//if (scrollViewer.Width == double.NaN)
			//	scrollViewer.Viewport.Width
			/*if (double.IsNaN(contentGrid.Width))
				contentGrid.Width = contentGrid.DesiredSize.Width + DefaultIncrementWidth;
			else
				contentGrid.Width += DefaultIncrementWidth;*/

			if (double.IsNaN(contentGrid.Width))
				contentGrid.MinWidth = contentGrid.DesiredSize.Width + DefaultIncrementWidth;
			else
				contentGrid.MinWidth += DefaultIncrementWidth;
			scrollViewer.Offset = new Vector(scrollViewer.Offset.X + DefaultIncrementWidth, scrollViewer.Offset.Y);
			scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
			scrollViewer.InvalidateArrange();
			scrollViewer.InvalidateMeasure();
		}

		private void ButtonCollapse_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			//scrollViewer.Offset = new Vector(Math.Max(0.0, scrollViewer.Offset.X - 500), scrollViewer.Offset.Y);
			contentGrid.MinWidth = 0;
		}

		// How to set the main Content
		protected void AddTabView(TabInstance tabInstance)
		{
			tabView = new TabView(tabInstance);
			tabView.Load();

			//Grid.SetRow(tabView, 1);
			//containerGrid.Children.Add(tabView);

			//scrollViewer.Content = tabView;
			contentGrid.Children.Add(tabView);
		}

		protected WindowSettings WindowSettings
		{
			get
			{
				WindowSettings windowSettings = new WindowSettings()
				{
					Maximized = (this.WindowState == WindowState.Maximized),
					Width = this.Width,
					Height = this.Height,
					Left = this.Position.X,
					Top = this.Position.Y,
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
			if (loadComplete && IsActive && IsArrangeValid && IsMeasureValid)
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
			//SaveWindowSettings();
		}

		// don't allow the scroll viewer to jump back to the left while we're loading content and the content grid width is fluctuating
		public void SetMinScrollOffset()
		{
			contentGrid.MinWidth = scrollViewer.Offset.X + scrollViewer.Bounds.Size.Width;
		}
	}
}

/*
https://github.com/AvaloniaUI/Avalonia/wiki/Hide-console-window-for-self-contained-.NET-Core-application


*/
