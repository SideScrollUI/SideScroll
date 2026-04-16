using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.Flyouts;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tasks;

namespace SideScroll.Avalonia.Controls;

/// <summary>
/// An icon-and-label button that dispatches synchronous or async actions via a <see cref="TaskInstance"/>,
/// supports hot keys, flyouts, progress, and theme-aware SVG icon recoloring.
/// </summary>
public class TabImageButton : Button, IDisposable
{
	/// <summary>Gets or sets an optional text label displayed beside the icon.</summary>
	public string? Label { get; set; }

	/// <summary>Gets the tooltip text shown on hover.</summary>
	public string? Tooltip { get; }

	/// <summary>Gets or sets the SVG or bitmap resource used as the button icon.</summary>
	public IResourceView ImageResource { get; set; }

	/// <summary>Gets or sets the icon size in pixels. Defaults to 24.</summary>
	public double IconSize { get; set; } = 24;

	/// <summary>Gets or sets whether the icon colors are updated to match the current theme.</summary>
	public bool UpdateIconColors { get; set; } = true;

	/// <summary>Gets or sets the tab instance used for task context.</summary>
	public TabInstance? TabInstance { get; set; }

	/// <summary>Gets or sets a synchronous action to invoke when the button is clicked.</summary>
	public CallAction? CallAction { get; set; }

	/// <summary>Gets or sets an asynchronous action to invoke when the button is clicked.</summary>
	public CallActionAsync? CallActionAsync { get; set; }

	/// <summary>Gets or sets whether the running task is displayed in the tab task list.</summary>
	public bool ShowTask { get; set; }

	/// <summary>Gets or sets whether a task is currently active; prevents re-entry while a task is running.</summary>
	public bool IsActive { get; set; }

	/// <summary>Gets or sets whether the async action runs on a background thread instead of the UI thread.</summary>
	public bool UseBackgroundThread { get; set; }

	/// <summary>Gets or sets an optional keyboard shortcut that triggers the button's action.</summary>
	public KeyGesture? KeyGesture { get; set; }

	/// <summary>Gets or sets the minimum time between repeated invocations when the button is clicked rapidly. Defaults to 1 second.</summary>
	public TimeSpan MinWaitTime { get; set; } = TimeSpan.FromSeconds(1);

	private DateTime? _lastInvoked;
	private DispatcherTimer? _dispatcherTimer;  // Delays auto selection to throttle updates

	private readonly Image _imageControl;
	private IImage? _defaultImage;
	private bool _disposed;

	protected virtual Color Color => (ImageResource as ImageColorView)?.Color ?? SideScrollTheme.IconForeground.Color;

	protected virtual Color? HighlightColor => (ImageResource as ImageColorView)?.HighlightColor ?? SideScrollTheme.IconForegroundHighlight.Color;
	protected virtual Color? DisabledColor => SideScrollTheme.IconForegroundDisabled.Color;

	protected IImage? HighlightImage => _highlightImage ??= UpdateIconColors ? SvgUtils.TryGetSvgColorImage(ImageResource, HighlightColor) : _defaultImage;
	private IImage? _highlightImage;

	protected IImage? DisabledImage => _disabledImage ??= SvgUtils.TryGetSvgColorImage(ImageResource, DisabledColor);
	private IImage? _disabledImage;

	public new bool IsEnabled
	{
		get => base.IsEnabled;
		set
		{
			base.IsEnabled = value;
			UpdateImage();
		}
	}

	/// <summary>Returns the button's tooltip text.</summary>
	public override string? ToString() => Tooltip;

	public TabImageButton(string tooltip, IResourceView imageResource, string? label = null, double? iconSize = null, bool updateIconColors = true)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Label = label;
		IconSize = iconSize ?? IconSize;
		UpdateIconColors = updateIconColors;

		Grid grid = new()
		{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			RowDefinitions = new RowDefinitions("*"),
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			Background = Brushes.Transparent, // Catch clicks
		};

		if (ImageResource.ResourceType == "svg")
		{
			if (updateIconColors)
			{
				_defaultImage = SvgUtils.TryGetSvgColorImage(ImageResource);
			}
			else
			{
				_defaultImage = SvgUtils.GetSvgImage(ImageResource);
			}
		}
		else
		{
			Stream stream = ImageResource.Stream;
			// For .ico files with multiple resolutions, decode to the target size
			// This selects the closest matching resolution from the icon
			int targetSize = (int)IconSize;
			_defaultImage = Bitmap.DecodeToWidth(stream, targetSize);
		}

		_imageControl = new()
		{
			Source = _defaultImage,
			Width = IconSize,
			Height = IconSize,
			Stretch = ImageResource.ResourceType == "svg" ? Stretch.Uniform : Stretch.None,
			Margin = new Thickness(4),
		};
		grid.Children.Add(_imageControl);

		if (Label != null)
		{
			TextBlock textBlock = new()
			{
				Text = Label,
				FontSize = 15,
				Foreground = SideScrollTheme.ToolbarLabelForeground,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(2, 4, 6, 4),
				[Grid.ColumnProperty] = 1,
			};
			grid.Children.Add(textBlock);
		}

		Content = grid;
		ToolTip.SetTip(this, Tooltip);

		Click += ToolbarButton_Click;
		ActualThemeVariantChanged += ToolbarButton_ActualThemeVariantChanged;
	}

	private void ToolbarButton_ActualThemeVariantChanged(object? sender, EventArgs e)
	{
		SetImage(ImageResource);
	}

	/// <summary>Replaces the button icon with the given resource and clears any cached recolored image variants.</summary>
	public void SetImage(IResourceView imageResource)
	{
		ImageResource = imageResource;

		_defaultImage = null;
		_highlightImage = null;
		_disabledImage = null;

		UpdateImage();
	}

	protected void UpdateImage()
	{
		if (ImageResource.ResourceType != "svg" || !UpdateIconColors) return;

		_defaultImage ??= SvgUtils.TryGetSvgColorImage(ImageResource);
		var source = IsEnabled ? _defaultImage : (DisabledImage ?? _defaultImage);
		if (source != _imageControl.Source)
		{
			_imageControl.Source = source;
		}
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property.Name == nameof(IsEnabled))
		{
			UpdateImage();
		}
	}

	private async void ToolbarButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		await InvokeAsync();
	}

	/// <summary>Binds <see cref="Avalonia.Controls.Control.IsEnabled"/> one-way to the specified property path on <paramref name="source"/>.</summary>
	public void BindIsEnabled(string path, object? source)
	{
		Bind(IsEnabledProperty, new Binding
		{
			Path = path,
			Source = source,
			Mode = BindingMode.OneWay,
		});
	}

	/// <summary>Registers this button's invocation as the default action on the owning <see cref="TabInstance"/>.</summary>
	public void SetDefault()
	{
		if (TabInstance != null)
		{
			TabInstance.DefaultAction = async () => await InvokeAsync();
		}
	}

	/// <summary>Invokes the button action, optionally rate-limiting rapid repeated clicks by <see cref="MinWaitTime"/>.</summary>
	public virtual async Task InvokeAsync(bool canDelay = true)
	{
		if (!IsEnabled || IsActive)
			return;

		if (_lastInvoked != null)
		{
			TimeSpan timeSpan = DateTime.UtcNow.Subtract(_lastInvoked.Value);
			if (canDelay && timeSpan < MinWaitTime)
			{
				// Rate limit request
				if (_dispatcherTimer == null)
				{
					_dispatcherTimer = new DispatcherTimer
					{
						Interval = TimeSpan.FromSeconds(1),
					};
					_dispatcherTimer.Tick += DispatcherTimer_Tick;
				}
				if (!_dispatcherTimer.IsEnabled)
				{
					_dispatcherTimer.Start();
				}
				return;
			}
		}
		_lastInvoked = DateTime.UtcNow;

		if (Flyout != null)
		{
			Flyout.ShowAt(this);
		}
		else
		{
			await InvokeTaskAsync();
		}
	}

	/// <summary>Starts the configured synchronous or asynchronous action as a managed task and shows a flyout message on error.</summary>
	public async Task InvokeTaskAsync()
	{
		if (TabInstance == null)
		{
			InvokeAction(new Call());
			return;
		}

		TaskInstance? taskInstance;
		if (CallActionAsync != null)
		{
			taskInstance = StartTaskAsync();
			if (!UseBackgroundThread && taskInstance?.Task is Task task)
			{
				await task;
			}
		}
		else
		{
			taskInstance = StartTask();
		}

		if (taskInstance?.Errored == true)
		{
			ShowFlyout(taskInstance.Message ?? $"{Name} Failed");
		}
	}

	private TaskInstance? StartTaskAsync()
	{
		if (CallActionAsync == null)
			return null;

		IsActive = true;
		var taskDelegate = new TaskDelegateAsync(CallActionAsync, UseBackgroundThread)
		{
			OnComplete = () => IsActive = false,
		};
		return StartTask(taskDelegate);
	}

	private TaskInstance? StartTask()
	{
		if (CallAction == null)
			return null;

		IsActive = true;
		var taskDelegate = new TaskDelegate(CallAction, ShowTask)
		{
			OnComplete = () => IsActive = false,
		};
		return StartTask(taskDelegate);
	}

	private TaskInstance StartTask(TaskCreator taskCreator)
	{
		if (TabInstance != null)
		{
			TaskInstance taskInstance = TabInstance.CreateTask(taskCreator, ShowTask);
			taskInstance.OnShowMessage += (_, e) => ShowFlyout(e.Message);
			taskInstance.Start();
			return taskInstance;
		}
		else
		{
			var call = new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			return taskInstance;
		}
	}

	/// <summary>Registers a synchronous call action to invoke when the button is clicked.</summary>
	public void Add(CallAction callAction)
	{
		CallAction = callAction;
	}

	/// <summary>Registers an asynchronous call action to invoke when the button is clicked.</summary>
	public void AddAsync(CallActionAsync callActionAsync)
	{
		CallActionAsync = callActionAsync;
	}

	protected void InvokeAction(Call call)
	{
		try
		{
			CallActionAsync?.Invoke(call);
			CallAction?.Invoke(call);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}

	protected void ShowFlyout(string message)
	{
		MessageFlyout flyout = new(message)
		{
			Placement = PlacementMode.BottomEdgeAlignedLeft,
		};
		flyout.ShowAt(this);
	}

	protected override void OnPointerEntered(PointerEventArgs e)
	{
		base.OnPointerEntered(e);

		if (HighlightImage != null)
		{
			_imageControl.Source = HighlightImage;
		}
	}

	protected override void OnPointerExited(PointerEventArgs e)
	{
		base.OnPointerExited(e);

		UpdateImage();
	}

	private async void DispatcherTimer_Tick(object? sender, EventArgs e)
	{
		_dispatcherTimer!.Stop();
		await InvokeAsync(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
			return;

		if (disposing)
		{
			// Dispose managed resources
			if (_dispatcherTimer != null)
			{
				_dispatcherTimer.Stop();
				_dispatcherTimer.Tick -= DispatcherTimer_Tick;
				_dispatcherTimer = null;
			}

			// Unsubscribe from events
			Click -= ToolbarButton_Click;
			ActualThemeVariantChanged -= ToolbarButton_ActualThemeVariantChanged;

			// Dispose images if they're disposable
			(_defaultImage as IDisposable)?.Dispose();
			(_highlightImage as IDisposable)?.Dispose();
			(_disabledImage as IDisposable)?.Dispose();
		}

		_disposed = true;
	}

	/// <summary>Releases event subscriptions, stops the rate-limit timer, and disposes cached image resources.</summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
