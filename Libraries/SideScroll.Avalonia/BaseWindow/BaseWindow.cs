using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using SideScroll.Avalonia.Tabs;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Viewer;
using SideScroll.Tabs;
using SideScroll.Tabs.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SideScroll.Avalonia;

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

	public BaseWindow(Project project)
	{
		Initialize(project);
	}

	public BaseWindow(ProjectSettings settings)
	{
		Initialize(Project.Load(settings));
	}

	[MemberNotNull(nameof(Project), nameof(TabViewer))]
	protected void Initialize(Project project)
	{
		Instance = this;

		FontTheme.FontFamilies =
			new List<FontFamily>
			{
				SideScrollTheme.ContentControlThemeFontFamily, // Inter Font
				SideScrollTheme.SourceCodeProFont,
			}
			.Concat(FontManager.Current.SystemFonts);

		SideScrollInit.Initialize();

		TabFileImage.Register();

		LoadProject(project);

		Opened += BaseWindow_Opened;
		Closed += BaseWindow_Closed;
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

	// Load here instead of in xaml for better control
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
			Width = Math.Min(MaxWidth, Math.Max(MinWindowWidth, value.Width));
			Height = Math.Min(MaxHeight, Math.Max(MinWindowHeight, value.Height));

			double minLeft = -10; // Left position for windows starts at -10
			double left = Math.Min(Math.Max(minLeft, value.Left), MaxWidth - Width + minLeft); // values can be negative
			double maxHeight = MaxHeight;
			double top = Math.Min(Math.Max(0, value.Top), maxHeight - Height);
			Position = new PixelPoint((int)left, (int)top);
			//Height = Math.Max(MinWindowSize, value.Height + 500); // reproduces black bar problem, not subtracting bottom toolbar for Height
			//Measure(Bounds.Size);

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

		var settings = Project.DataApp.Load<WindowSettings>(true);
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
		Project.DataApp.Save(WindowSettings);
	}

	private void BaseWindow_Opened(object? sender, EventArgs e)
	{
		TabViewer.Focus();
	}

	private void BaseWindow_Closed(object? sender, EventArgs e)
	{
		// todo: split saving position out
		//SaveWindowSettings();
	}

	// Broken with Avalonia 11 update, unclear if this is still needed or not
	/*protected override void HandleWindowStateChanged(WindowState state)
	{
		base.HandleWindowStateChanged(state);
		SaveWindowSettings();
	}*/

	// this fires too often, could attach a dispatch timer, or add an override method
	private void BaseWindow_PositionChanged(object? sender, PixelPointEventArgs e)
	{
		SaveWindowSettings();
	}
}
