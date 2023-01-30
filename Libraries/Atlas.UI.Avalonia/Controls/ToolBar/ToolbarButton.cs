using System.Windows.Input;
using Atlas.Core;
using Atlas.Tabs;
using Atlas.UI.Avalonia.Themes;
using Atlas.UI.Avalonia.Utilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Atlas.UI.Avalonia.Controls;

public class ToolbarButton : Button, IStyleable, ILayoutable, IDisposable
{
	Type IStyleable.StyleKey => typeof(ToolbarButton);

	public TabControlToolbar Toolbar;
	public string? Label { get; set; }
	public string? Tooltip { get; set; }

	public TaskDelegate.CallAction? CallAction;
	public TaskDelegateAsync.CallActionAsync? CallActionAsync;

	public bool ShowTask;
	public bool IsActive; // Only allow one task at once (modifying IsEnabled doesn't updating elsewhere)

	public TimeSpan MinWaitTime = TimeSpan.FromSeconds(1); // Wait time between clicks

	private DateTime? _lastInvoked;
	private DispatcherTimer? _dispatcherTimer;  // delays auto selection to throttle updates

	public ToolbarButton(TabControlToolbar toolbar, ToolButton toolButton)
	{
		Toolbar = toolbar;
		Label = toolButton.Label;
		Tooltip = toolButton.Tooltip;
		ShowTask = toolButton.ShowTask;

		CallAction = toolButton.Action;
		CallActionAsync = toolButton.ActionAsync;

		Initialize(toolButton.Icon);

		if (toolButton.Default)
			SetDefault();
	}

	public ToolbarButton(TabControlToolbar toolbar, string? label, string tooltip, Stream bitmapStream, ICommand? command = null)
	{
		Toolbar = toolbar;
		Label = label;
		Tooltip = tooltip;

		Initialize(bitmapStream, command);
	}

	private void Initialize(Stream bitmapStream, ICommand? command = null)
	{
		Grid grid = new()
		{
			ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
			RowDefinitions = new RowDefinitions("Auto"),
		};

		IImage sourceImage;
		try
		{
			bitmapStream.Position = 0;
			sourceImage = new Bitmap(bitmapStream);
		}
		catch (Exception)
		{
			sourceImage = SvgUtils.GetSvgImage(bitmapStream);
		}

		Image image = new()
		{
			Source = sourceImage,
			Width = 24,
			Height = 24,
			//MaxWidth = 24,
			//MaxHeight = 24,
			Stretch = Stretch.None,
		};
		grid.Children.Add(image);

		if (Label != null)
		{
			TextBlock textBlock = new()
			{
				Text = Label,
				FontSize = 15,
				Foreground = new SolidColorBrush(Color.Parse("#759eeb")),
				Margin = new Thickness(6),
				[Grid.ColumnProperty] = 1,
			};
			grid.Children.Add(textBlock);
		}

		Content = grid;
		Command = command;
		Background = Theme.ToolbarButtonBackground;
		BorderBrush = Background;
		BorderThickness = new Thickness(0);
		Margin = new Thickness(1);
		//BorderThickness = new Thickness(2),
		//Foreground = new SolidColorBrush(Theme.ButtonForegroundColor),
		//BorderBrush = new SolidColorBrush(Colors.Black),
		ToolTip.SetTip(this, Tooltip);

		BorderBrush = Background;
		Click += ToolbarButton_Click;
	}

	private void ToolbarButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		Invoke();
	}

	public void SetDefault()
	{
		Toolbar.TabInstance!.DefaultAction = () => Invoke();
	}

	public void Invoke(bool canDelay = true)
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
					_dispatcherTimer = new DispatcherTimer()
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

		if (Toolbar.TabInstance == null)
		{
			InvokeAction(new Call());
			return;
		}

		// Only allow one since we don't block for completion of first
		if (StartTaskAsync() == null)
			StartTask();
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
		return Toolbar.TabInstance!.StartTask(taskDelegate, ShowTask);
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
		return Toolbar.TabInstance!.StartTask(taskDelegate, ShowTask);
	}

	public void Add(TaskDelegate.CallAction callAction)
	{
		CallAction = callAction;
	}

	public void AddAsync(TaskDelegateAsync.CallActionAsync callActionAsync)
	{
		CallActionAsync = callActionAsync;
	}

	private void InvokeAction(Call call)
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

	// DefaultTheme.xaml is overriding this currently
	protected override void OnPointerEnter(PointerEventArgs e)
	{
		base.OnPointerEnter(e);
		BorderBrush = new SolidColorBrush(Colors.Black); // can't overwrite hover border :(
		Background = Theme.ToolbarButtonBackgroundHover;
		InvalidateVisual();
	}

	protected override void OnPointerLeave(PointerEventArgs e)
	{
		base.OnPointerLeave(e);
		Background = Theme.ToolbarButtonBackground;
		BorderBrush = Background;
		InvalidateVisual();
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
