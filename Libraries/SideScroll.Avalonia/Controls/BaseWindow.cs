using Avalonia;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.Controls;

public class BaseWindow : Window
{
	public static int MinWindowWidth { get; set; } = 700;
	public static int MinWindowHeight { get; set; } = 500;

	public static int DefaultWindowWidth { get; set; } = 1280;
	public static int DefaultWindowHeight { get; set; } = 800;

	public static BaseWindow? Instance { get; set; }

	public Project Project { get; protected set; }

	public TabViewer TabViewer { get; protected set; }

	private bool _loadComplete;

	private Rect? _normalSizeBounds; // used for saving when maximized

	private readonly DispatcherTimer _dispatcherTimer;

	public BaseWindow(Project project)
	{
		Instance = this;

		SideScrollInit.Initialize();
		SideScrollTheme.InitializeFonts();

		TabFileImage.Register();

		LoadProject(project);

		Opened += BaseWindow_Opened;

		_dispatcherTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMinutes(10), // Won't trigger initially
		};
		_dispatcherTimer.Tick += DispatcherTimer_Tick;
		_dispatcherTimer.Start();
	}

	public BaseWindow(ProjectSettings settings) : 
		this(Project.Load(settings))
	{
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	public void LoadProject(Project project)
	{
		project.Initialize();
		Project = project;

		LoadWindowSettings();

		ThemeManager.Initialize(project);

		InitializeComponent();

		_loadComplete = true;
	}

	[MemberNotNull(nameof(TabViewer))]
	private void InitializeComponent()
	{
		Title = Project.ProjectSettings.Name ?? "<Name>";

		Background = SideScrollTheme.TabBackground;

		MinWidth = MinWindowWidth;
		MinHeight = MinWindowHeight;

		Content = TabViewer = new TabViewer(Project);

		PositionChanged += BaseWindow_PositionChanged;

		this.GetObservable(ClientSizeProperty).Subscribe(new AnonymousObserver<Size>(Resize));
	}

	public virtual void LoadTab(ITab tab)
	{
		TabViewer.LoadTab(tab);
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
				{
					maxHeight -= 12; // On windows, the menu header takes up an extra 12 pixels when not maximized
				}
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
			bool maximized = WindowState == WindowState.Maximized;
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

			var windowSettings = new WindowSettings
			{
				Maximized = maximized,
				Width = bounds.Width,
				Height = bounds.Height,
				Left = bounds.Left,
				Top = bounds.Top,
			};

			if (windowSettings.Width <= 0)
			{
				windowSettings.Width = DefaultWindowWidth;
			}

			if (windowSettings.Height <= 0)
			{
				windowSettings.Height = DefaultWindowHeight;
			}

			return windowSettings;
		}
		set
		{
			// These are causing the window to be shifted down
			Width = Math.Clamp(value.Width, MinWindowWidth, MaxWidth);
			Height = Math.Clamp(value.Height, MinWindowHeight, MaxHeight);

			double minLeft = -10; // Left position for windows starts at -10
			double left = Math.Clamp(value.Left, minLeft, MaxWidth - Width + minLeft); // values can be negative

			double maxHeight = MaxHeight;
			double top = Math.Clamp(value.Top, 0, maxHeight - Height);
			Position = new PixelPoint((int)left, (int)top);

			// Avalonia bug? WindowState doesn't update correctly for MacOS
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}
	}

	protected void LoadWindowSettings()
	{
		SetMaxBounds();

		var settings = Project.Data.App.Load<WindowSettings>(true);
		if (settings != null)
		{
			WindowSettings = settings;
		}
	}

	// Still saving due to a HandleResized calls after IsActive (loadComplete does nothing)
	private void SaveWindowSettings()
	{
		// need a better trigger for when the screen size changes
		SetMaxBounds();

		if (_loadComplete)// && IsArrangeValid && IsMeasureValid) // && IsActive (this can be false even after loading)
		{
			Dispatcher.UIThread.Post(SaveWindowSettingsInternal, DispatcherPriority.SystemIdle);
		}
	}

	private void SaveWindowSettingsInternal()
	{
		Project.Data.App.Save(WindowSettings);
	}

	private void BaseWindow_Opened(object? sender, EventArgs e)
	{
		TabViewer.Focus();
	}

	// If we want to throttle this, we could attach a dispatch timer, or add an override method
	private void BaseWindow_PositionChanged(object? sender, PixelPointEventArgs e)
	{
		SaveWindowSettings();
	}

	private void DispatcherTimer_Tick(object? sender, EventArgs e)
	{
		Project.Data.Cache.CleanupCache(new(), TimeSpan.FromDays(Project.DataSettings.CacheDurationDays));
	}
}
