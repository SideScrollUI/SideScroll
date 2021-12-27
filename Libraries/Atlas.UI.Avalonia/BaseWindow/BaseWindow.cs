using Atlas.Core;
using Atlas.Resources;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;

namespace Atlas.UI.Avalonia
{
	public class BaseWindow : Window
	{
		private const int MinWindowSize = 700;

		private const int DefaultWindowWidth = 1280;
		private const int DefaultWindowHeight = 800;

		public Project Project;

		public TabViewer TabViewer;

		private bool _loadComplete = false;

		private Rect? _normalSizeBounds = null; // used for saving when maximized

		public BaseWindow(Project project) : base()
		{
			Initialize(project);
		}

		public BaseWindow(ProjectSettings settings) : base()
		{
			Initialize(new Project(settings));
		}

		private void Initialize(Project project)
		{
			AtlasInit.Initialize();

			LoadProject(project);

			Opened += BaseWindow_Opened;
			Closed += BaseWindow_Closed;
		}

		public void LoadProject(Project project)
		{
			Project = project;

			LoadWindowSettings();

			InitializeComponent();

			_loadComplete = true;
		}

		// Load here instead of in xaml for better control
		private void InitializeComponent()
		{
			Title = Project.ProjectSettings.Name ?? "<Name>";

			Background = Theme.TabBackground;

			MinWidth = MinWindowSize;
			MinHeight = MinWindowSize;

			Icon = new WindowIcon(Icons.Streams.Logo);

			Content = TabViewer = new TabViewer(Project);

			PositionChanged += BaseWindow_PositionChanged;

			this.GetObservable(ClientSizeProperty).Subscribe(Resize);
		}

		public virtual void AddTab(ITab tab)
		{
			TabViewer.AddTab(tab);
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
				double workingHeight = screen.WorkingArea.Height;
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !_loadComplete)
				{
					// OSX doesn't resize to smaller correctly if maximized at start
					workingHeight -= 20;
					if (WindowState == WindowState.Maximized)
						maxHeight -= 12; // On windows, the menu header takes up an extra 12 pixels when not maximized
				}
				maxHeight = Math.Max(maxHeight, workingHeight);
			}
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
		}

		protected WindowSettings WindowSettings
		{
			get
			{
				bool maximized = (WindowState == WindowState.Maximized);
				Rect bounds = Bounds;
				if (maximized || bounds.Width > 0.8 * MaxWidth)
				{
					bounds = _normalSizeBounds ?? bounds;
				}
				else
				{
					// Bounds.Position is 0, 0 (Windows & OSX)
					bounds = new Rect(Position.X, Position.Y, bounds.Width, bounds.Height);
					_normalSizeBounds = bounds;
				}

				var windowSettings = new WindowSettings()
				{
					Maximized = maximized,
					Width = bounds.Width,
					Height = bounds.Height,
					Left = bounds.Left,
					Top = bounds.Top,
				};

				if (windowSettings.Width <= 0)
					windowSettings.Width = DefaultWindowWidth;

				if (windowSettings.Height <= 0)
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
				double top = Math.Min(Math.Max(0, value.Top), maxHeight - Height);
				Position = new PixelPoint((int)left, (int)top);
				//Height = Math.Max(MinWindowSize, value.Height + 500); // reproduces black bar problem, not subtracting bottom toolbar for Height
				//Measure(Bounds.Size);

				// Avalonia bug? WindowState doesn't update correctly for MacOS
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}

		protected void LoadWindowSettings()
		{
			SetMaxBounds();

			WindowSettings = Project.DataApp.Load<WindowSettings>(true);
		}

		// Still saving due to a HandleResized calls after IsActive (loadComplete does nothing)
		private void SaveWindowSettings()
		{
			// need a better trigger for when the screen size changes
			SetMaxBounds();

			if (_loadComplete)// && IsArrangeValid && IsMeasureValid) // && IsActive (this can be false even after loading)
				Dispatcher.UIThread.Post(SaveWindowSettingsInternal, DispatcherPriority.SystemIdle);
		}

		private void SaveWindowSettingsInternal()
		{
			Project.DataApp.Save(WindowSettings);
		}

		private void BaseWindow_Opened(object sender, EventArgs e)
		{
			TabViewer.Focus();
		}

		private void BaseWindow_Closed(object sender, EventArgs e)
		{
			// todo: split saving position out
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
