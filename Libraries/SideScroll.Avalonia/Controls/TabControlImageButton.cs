using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SideScroll.Avalonia.Themes;
using SideScroll.Avalonia.Utilities;
using SideScroll.Resources;
using SideScroll.Tabs;
using SideScroll.Tasks;
using System.Windows.Input;

namespace SideScroll.Avalonia.Controls;

public class TabControlImageButton : Button, IDisposable
{
	public string? Label { get; set; }
	public string? Tooltip { get; set; }

	public IResourceView ImageResource { get; set; }
	public double IconSize { get; set; } = 24;

	public TabInstance? TabInstance { get; set; }

	public CallAction? CallAction { get; set; }
	public CallActionAsync? CallActionAsync { get; set; }

	public bool ShowTask { get; set; }
	public bool IsActive { get; set; } // Only allow one task at once (modifying IsEnabled doesn't update elsewhere)

	public KeyGesture? KeyGesture { get; set; }

	public TimeSpan MinWaitTime { get; set; } = TimeSpan.FromSeconds(1); // Wait time between clicks

	private DateTime? _lastInvoked;
	private DispatcherTimer? _dispatcherTimer;  // delays auto selection to throttle updates

	private Image _imageControl;
	private IImage? _defaultImage;

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

	public TabControlImageButton(string tooltip, IResourceView imageResource, string? label = null, double? iconSize = null, ICommand? command = null)
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
				Foreground = new SolidColorBrush(Color),
				Margin = new Thickness(6),
				[Grid.ColumnProperty] = 1,
			};
			grid.Children.Add(textBlock);
		}

		Content = grid;
		Command = command;
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

	private void ToolbarButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		Invoke();
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
			TabInstance.DefaultAction = () => Invoke();
		}
	}

	public virtual void Invoke(bool canDelay = true)
	{
		if (!IsEnabled || IsActive)
			return;

		if (_lastInvoked != null)
		{
			TimeSpan timeSpan = DateTime.UtcNow.Subtract(_lastInvoked.Value);
			if (canDelay && timeSpan < MinWaitTime)
			{
				// Rate limiting can delay these
				if (_dispatcherTimer == null)
				{
					_dispatcherTimer = new DispatcherTimer
					{
						Interval = TimeSpan.FromSeconds(1),
					};
					_dispatcherTimer.Tick += DispatcherTimer_Tick;
				}
				if (!_dispatcherTimer.IsEnabled)
					_dispatcherTimer.Start();
				return;
			}
		}
		_lastInvoked = DateTime.UtcNow;

		if (Flyout != null)
		{
			Flyout.ShowAt(this);
			return;
		}

		if (TabInstance == null)
		{
			InvokeAction(new Call());
			return;
		}

		// Only allow one since we don't block for completion of first
		if (StartTaskAsync() == null)
		{
			StartTask();
		}
	}

	private TaskInstance? StartTaskAsync()
	{
		if (CallActionAsync == null)
			return null;

		IsActive = true;
		var taskDelegate = new TaskDelegateAsync(CallActionAsync, true)
		{
			OnComplete = () => IsActive = false,
		};

		if (TabInstance != null)
		{
			return TabInstance.StartTask(taskDelegate, ShowTask);
		}
		else
		{
			var call = new Call(taskDelegate.Label);
			TaskInstance taskInstance = taskDelegate.Start(call);
			return taskInstance;
		}
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

		if (TabInstance != null)
		{
			return TabInstance!.StartTask(taskDelegate, ShowTask);
		}
		else
		{
			var call = new Call(taskDelegate.Label);
			TaskInstance taskInstance = taskDelegate.Start(call);
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

	private void DispatcherTimer_Tick(object? sender, EventArgs e)
	{
		_dispatcherTimer!.Stop();
		Invoke(false);
	}

	public void Dispose()
	{
		if (_dispatcherTimer != null)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Tick -= DispatcherTimer_Tick;
			_dispatcherTimer = null;
		}
	}
}
