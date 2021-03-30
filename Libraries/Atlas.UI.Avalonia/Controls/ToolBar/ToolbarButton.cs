using System;
using System.IO;
using System.Windows.Input;
using Atlas.Core;
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

		public ToolbarButton(TabControlToolbar toolbar, string label, string tooltip, Stream bitmapStream, ICommand command = null) : base()
		{
			Toolbar = toolbar;
			Label = label;
			Tooltip = tooltip;

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

			if (label != null)
			{
				var textBlock = new TextBlock()
				{
					Text = label,
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
			ToolTip.SetTip(this, tooltip);

			BorderBrush = Background;
			Click += ToolbarButton_Click;
		}

		private void ToolbarButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (Toolbar.TabInstance == null)
			{
				InvokeAction(new Call());
				return;
			}

			if (CallActionAsync != null)
				Toolbar.TabInstance.StartAsync(CallActionAsync, null, ShowTask);

			if (CallAction != null)
				Toolbar.TabInstance.StartTask(CallAction, ShowTask, ShowTask);
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