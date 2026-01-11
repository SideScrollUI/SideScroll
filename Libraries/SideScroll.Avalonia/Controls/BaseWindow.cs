using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.Viewer;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Themes;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using SideScroll.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia.Controls;

public class BaseWindow : Window
{
	public static int WindowsBorderWidth = 7;

	public static int DefaultMinWidth { get; set; } = 700;
	public static int DefaultMinHeight { get; set; } = 500;

	public static int DefaultWidth { get; set; } = 1280;
	public static int DefaultHeight { get; set; } = 800;

	public static TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);

	public static BaseWindow? Instance { get; set; }

	public Project Project { get; protected set; }

	public TabViewer TabViewer { get; protected set; }
	public Border? Border { get; protected set; } // Windows 10 only for now since it has no border without chrome enabled

	private bool _loadComplete;

	private Rect? _normalSizeBounds; // Used for saving when maximized

	private readonly DispatcherTimer _cleanupDispatcherTimer;

	public BaseWindow(Project project)
	{
		Instance = this;

		if (project.UserSettings.EnableCustomTitleBar == true)
		{
			ExtendClientAreaToDecorationsHint = true;
			ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
			ExtendClientAreaTitleBarHeightHint = 50; // Increased from default ~32 to match custom title bar height (34px + margins)
		}

		SideScrollInit.Initialize();
		SideScrollTheme.InitializeFonts();

		TabFileImage.Register();

		LoadProject(project);

		Opened += BaseWindow_Opened;

		_cleanupDispatcherTimer = new DispatcherTimer
		{
			Interval = CleanupInterval, // Won't trigger initially
		};
		_cleanupDispatcherTimer.Tick += CleanupDispatcherTimer_Tick;
		_cleanupDispatcherTimer.Start();
	}

	public BaseWindow(ProjectSettings settings) : 
		this(Project.Load(settings))
	{
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	private void LoadProject(Project project)
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

		MinWidth = DefaultMinWidth;
		MinHeight = DefaultMinHeight;

		TabViewer = new TabViewer(Project);

		if (Project.UserSettings.EnableCustomTitleBar == true && ProcessUtils.IsWindows10OrBelow())
		{
			// Windows 10 and below won't display a border or drop shadow for custom title bars with the mode we need
			Border = new()
			{
				BorderBrush = SideScrollTheme.TabBackgroundBorder,
				BorderThickness = new(1),
				Child = TabViewer,
			};
			Content = Border;

			ActualThemeVariantChanged += Border_ActualThemeVariantChanged;
		}
		else
		{
			Content = TabViewer;
		}

		PositionChanged += BaseWindow_PositionChanged;

		this.GetObservable(ClientSizeProperty).Subscribe(new AnonymousObserver<Size>(Resize));
		this.GetObservable(WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(WindowStateChanged));
	}

	private void Border_ActualThemeVariantChanged(object? sender, EventArgs e)
	{
		if (Border != null)
		{
			Border.BorderBrush = SideScrollTheme.TabBackgroundBorder;
		}
	}

	private void WindowStateChanged(WindowState state)
	{
		if (Project.UserSettings.EnableCustomTitleBar == true && WindowState == WindowState.Maximized)
		{
			Padding = new Thickness(7);
		}
		else
		{
			Padding = new Thickness(0);
		}
	}

	public virtual void LoadTab(ITab tab)
	{
		TabViewer.LoadTab(tab);
	}

	private void Resize(Size size)
	{
		SaveWindowSettings();
	}

	// Modifying the actual MaxWidth or MaxHeight breaks double click restoring the previous Width and Height
	private Size GetMaxBounds()
	{
		double maxWidth = 0;
		double maxHeight = 0;
		foreach (var screen in Screens.All)
		{
			maxWidth += screen.Bounds.Width;
			double workingHeight = screen.WorkingArea.Height;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !_loadComplete)
			{
				// macOS doesn't resize to smaller correctly if maximized at start
				workingHeight -= 20;
				if (WindowState == WindowState.Maximized)
				{
					maxHeight -= 12; // On Windows, the menu header takes up an extra 12 pixels when not maximized
				}
			}
			maxHeight = Math.Max(maxHeight, workingHeight);
		}
		return new Size(maxWidth, maxHeight);
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
				// Bounds.Position is 0, 0 (Windows & macOS)
				bounds = new Rect(Position.X, Position.Y, bounds.Width, bounds.Height);
				_normalSizeBounds = bounds;
			}

			WindowSettings windowSettings = new()
			{
				Maximized = maximized,
				Width = bounds.Width,
				Height = bounds.Height,
				Left = bounds.Left,
				Top = bounds.Top,
			};

			if (windowSettings.Width <= 0)
			{
				windowSettings.Width = DefaultWidth;
			}

			if (windowSettings.Height <= 0)
			{
				windowSettings.Height = DefaultHeight;
			}

			return windowSettings;
		}
		set
		{
			Size maxBounds = GetMaxBounds();
			// These are causing the window to be shifted down
			Width = Math.Clamp(value.Width, MinWidth, maxBounds.Width);
			Height = Math.Clamp(value.Height, MinHeight, MaxHeight);

			double minLeft = 0;
			if (Project.UserSettings.EnableCustomTitleBar == false)
			{
				minLeft = -WindowsBorderWidth;
			}
			double left = Math.Clamp(value.Left, minLeft, maxBounds.Width - Width + minLeft); // Values can be negative

			double maxHeight = MaxHeight;
			double top = Math.Clamp(value.Top, 0, maxHeight - Height);
			Position = new PixelPoint((int)left, (int)top);

			// Avalonia bug? WindowState doesn't update correctly for macOS
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				WindowState = value.Maximized ? WindowState.Maximized : WindowState.Normal;
			}
		}
	}

	protected void LoadWindowSettings()
	{
		var settings = Project.Data.App.Load<WindowSettings>();
		if (settings == null)
		{
			WindowStartupLocation = WindowStartupLocation.CenterScreen;
			settings = new();
			Width = settings.Width;
			Height = settings.Height;
		}
		else
		{
			WindowStartupLocation = WindowStartupLocation.Manual;
			WindowSettings = settings;
		}
	}

	// Still saving due to a HandleResized calls after IsActive (loadComplete does nothing)
	private void SaveWindowSettings()
	{
		if (_loadComplete)
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

	private void CleanupDispatcherTimer_Tick(object? sender, EventArgs e)
	{
		Project.Data.Cache.CleanupCache(new(), TimeSpan.FromDays(Project.DataSettings.CacheDurationDays));
	}
}
