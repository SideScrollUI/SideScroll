using Atlas.Resources;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia
{
	public class BaseWindow : Window
	{
		private const int MinWindowSize = 700;
		private const int DefaultWindowWidth = 1280;
		private const int DefaultWindowHeight = 800;

		public Project project;

		private bool loadComplete = false;

		public TabViewer tabViewer;

		public BaseWindow(Project project) : base()
		{
			LoadProject(project);
#if DEBUG
			this.AttachDevTools();
#endif
			Closed += BaseWindow_Closed;
		}

		public void LoadProject(Project project)
		{
			this.project = project;

			LoadWindowSettings();

			InitializeComponent();

			loadComplete = true;
		}

		// Load here instead of in xaml for better control
		private void InitializeComponent()
		{
			Title = project.ProjectSettings.Name ?? "<Name>";

			Background = Theme.TabBackground;

			MinWidth = MinWindowSize;
			MinHeight = MinWindowSize;

			Resources["FontSizeSmall"] = 14; // stop DatePicker using a small font size

			Icon = new WindowIcon(Icons.Streams.Logo);

			Content = tabViewer = new TabViewer(project);

			PositionChanged += BaseWindow_PositionChanged;

			this.GetObservable(ClientSizeProperty).Subscribe(Resize);
		}

		public void AddTab(ITab tab)
		{
			tabViewer.AddTab(tab);
		}

		private void Resize(Size size)
		{
			SaveWindowSettings();
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
				bool maximized = IsMaximized();
				Rect bounds = Bounds;
				if (maximized && TransformedBounds != null)
					bounds = TransformedBounds.Value.Bounds;
				var windowSettings = new WindowSettings()
				{
					Maximized = maximized,
					Width = bounds.Width,
					Height = bounds.Height,
					Left = maximized ? bounds.Position.X : Position.X,
					Top = maximized ? bounds.Position.Y : Position.Y,
				};
				if (windowSettings.Width == 0)
					windowSettings.Width = DefaultWindowWidth;
				if (windowSettings.Height == 0)
					windowSettings.Height = DefaultWindowHeight;

				return windowSettings;
			}
			set
			{
				// These are causing the window to be shifted down
				Width = Math.Max(MinWindowSize, value.Width);
				Height = Math.Max(MinWindowSize, value.Height);

				double minLeft = -10; // Left position for windows starts at -10
				double left = Math.Min(Math.Max(minLeft, value.Left), MaxWidth - Width + minLeft); // values can be negative
				double maxHeight = MaxHeight;
				if (!IsMaximized())
					maxHeight -= 12; // On windows, the menu header takes up an extra 12 pixels when maximized
				double top = Math.Min(Math.Max(0, value.Top), maxHeight - Height);
				Position = new PixelPoint((int)left, (int)top);
				//Height = Math.Max(MinWindowSize, value.Height + 500); // reproduces black bar problem, not subtracting bottom toolbar for Height
				//Measure(Bounds.Size);
				WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}

		private bool IsMaximized()
		{
			bool maximized = (WindowState == WindowState.Maximized);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) // Avalonia bug? WindowState doesn't update correctly for MacOS
				maximized = false;
			return maximized;
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
	}
}