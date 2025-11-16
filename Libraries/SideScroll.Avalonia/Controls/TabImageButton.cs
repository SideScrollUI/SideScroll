using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
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

public class TabImageButton : Button, IDisposable
{
	public string? Label { get; set; }
	public string? Tooltip { get; }

	public IResourceView ImageResource { get; set; }
	public double IconSize { get; set; } = 24;

	public TabInstance? TabInstance { get; set; }

	public CallAction? CallAction { get; set; }
	public CallActionAsync? CallActionAsync { get; set; }

	public bool ShowTask { get; set; }
	public bool IsActive { get; set; } // Only allow one task at once (modifying IsEnabled doesn't update elsewhere)
	public bool UseBackgroundThread { get; set; }

	public KeyGesture? KeyGesture { get; set; }

	public TimeSpan MinWaitTime { get; set; } = TimeSpan.FromSeconds(1); // Wait time between clicks

	private DateTime? _lastInvoked;
	private DispatcherTimer? _dispatcherTimer;  // Delays auto selection to throttle updates

	private readonly Image _imageControl;
	private IImage? _defaultImage;
	private bool _disposed;

	protected virtual Color Color => (ImageResource as ImageColorView)?.Color ?? SideScrollTheme.IconForeground.Color;

	protected virtual Color? HighlightColor => (ImageResource as ImageColorView)?.HighlightColor ?? SideScrollTheme.IconForegroundHighlight.Color;
	protected virtual Color? DisabledColor => SideScrollTheme.IconForegroundDisabled.Color;

	protected IImage? HighlightImage => _highlightImage ??= SvgUtils.TryGetSvgColorImage(ImageResource, HighlightColor);
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

	public override string? ToString() => Tooltip;

	public TabImageButton(string tooltip, IResourceView imageResource, string? label = null, double? iconSize = null)
	{
		Tooltip = tooltip;
		ImageResource = imageResource;
		Label = label;
		IconSize = iconSize ?? IconSize;

		Grid grid = new()
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
			RowDefinitions = new RowDefinitions("Auto"),
		};

		if (ImageResource.ResourceType == "svg")
		{
			_defaultImage = SvgUtils.TryGetSvgColorImage(ImageResource);
		}
		else
		{
			Stream stream = ImageResource.Stream;
			_defaultImage = new Bitmap(stream);
		}

		_imageControl = new()
		{
			Source = _defaultImage,
			Width = IconSize,
			Height = IconSize,
			Stretch = ImageResource.ResourceType == "svg" ? Stretch.Uniform : Stretch.None,
		};
		grid.Children.Add(_imageControl);

		if (Label != null)
		{
			TextBlock textBlock = new()
			{
				Text = Label,
				FontSize = 15,
				Foreground = SideScrollTheme.ToolbarLabelForeground,
				Margin = new Thickness(6),
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
		if (ImageResource.ResourceType != "svg") return;

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

	public void BindIsEnabled(string path, object? source)
	{
		Bind(IsEnabledProperty, new Binding
		{
			Path = path,
			Source = source,
			Mode = BindingMode.OneWay,
		});
	}

	public void SetDefault()
	{
		if (TabInstance != null)
		{
			TabInstance.DefaultAction = async () => await InvokeAsync();
		}
	}

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
		var taskDelegate = new TaskDelegateAsync(CallActionAsync)
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

	public void Add(CallAction callAction)
	{
		CallAction = callAction;
	}

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

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
