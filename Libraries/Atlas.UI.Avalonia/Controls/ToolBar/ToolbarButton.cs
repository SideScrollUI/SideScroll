using System;
using System.IO;
using System.Windows.Input;
using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace Atlas.UI.Avalonia.Tabs
{
	public class ToolbarButton : Button, IStyleable, ILayoutable
	{
		Type IStyleable.StyleKey => typeof(Button);

		public TabControlToolbar Toolbar;
		public string Label { get; set; }
		public string Tooltip { get; set; }

		public TaskDelegate.CallAction CallAction;
		public TaskDelegateAsync.CallActionAsync CallActionAsync;

		public bool ShowTask;

		public TimeSpan MinWaitTime = TimeSpan.FromSeconds(1); // Wait time between clicks

		private DateTime _lastInvoked;

		public ToolbarButton(TabControlToolbar toolbar, ToolButton toolButton) : base()
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

		public ToolbarButton(TabControlToolbar toolbar, string label, string tooltip, Stream bitmapStream, ICommand command = null) : base()
		{
			Toolbar = toolbar;
			Label = label;
			Tooltip = tooltip;

			Initialize(bitmapStream, command);
		}

		private void Initialize(Stream bitmapStream, ICommand command = null)
		{
			bitmapStream.Position = 0;
			var bitmap = new Bitmap(bitmapStream);

			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto"),
				RowDefinitions = new RowDefinitions("Auto"),
			};

			var image = new Image()
			{
				Source = bitmap,
				//MaxWidth = 24,
				//MaxHeight = 24,
				Stretch = Stretch.None,
			};
			grid.Children.Add(image);

			if (Label != null)
			{
				var textBlock = new TextBlock()
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

		private void ToolbarButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Invoke();
		}

		public void SetDefault()
		{
			Toolbar.TabInstance.DefaultAction = () => Invoke();
		}

		private void Invoke()
		{
			if (!IsEnabled)
				return;

			TimeSpan timeSpan = DateTime.UtcNow.Subtract(_lastInvoked);
			if (timeSpan < MinWaitTime)
				return;

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

		private TaskInstance StartTaskAsync()
		{
			if (CallActionAsync == null)
				return null;
			
			IsEnabled = false;
			var taskDelegate = new TaskDelegateAsync(CallActionAsync, true)
			{
				OnComplete = () => IsEnabled = true,
			};
			return Toolbar.TabInstance.StartTask(taskDelegate, ShowTask);
		}

		private TaskInstance StartTask()
		{
			if (CallAction == null)
				return null;
			
			IsEnabled = false;
			var taskDelegate = new TaskDelegate(CallAction, ShowTask)
			{
				OnComplete = () => IsEnabled = true,
			};
			return Toolbar.TabInstance.StartTask(taskDelegate, ShowTask);
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
		}

		protected override void OnPointerLeave(PointerEventArgs e)
		{
			base.OnPointerLeave(e);
			Background = Theme.ToolbarButtonBackground;
			BorderBrush = Background;
		}
	}
}