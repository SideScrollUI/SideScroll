using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlTitle : UserControl
	{
		public TabInstance TabInstance;
		public string Label { get; set; }

		public TextBlock TextBlock;
		//private CheckBox checkBox;

		public TabControlTitle(TabInstance tabInstance, string name = null)
		{
			TabInstance = tabInstance;
			Label = name ?? tabInstance.Label;
			Label = new StringReader(Label).ReadLine();

			InitializeControl();
		}

		protected override Size MeasureCore(Size availableSize)
		{
			Size measured = base.MeasureCore(availableSize);
			Size maxSize = new Size(Math.Min(50, measured.Width), measured.Height);
			return maxSize;
		}

		public void InitializeControl()
		{
			Background = Theme.TitleBackground;

			var containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};

			// Add a wrapper class with a border?
			// Need to make the desired size for this a constant x
			TextBlock = new TextBlock()
			{
				Text = Label,
				FontSize = 15,
				//Margin = new Thickness(2), // Shows as black, Need Padding so Border not needed
				//Background = new SolidColorBrush(Theme.TitleBackgroundColor),
				Foreground = Theme.TitleForeground,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				//HorizontalAlignment = HorizontalAlignment.Left,
				//[ToolTip.TipProperty] = Label, // re-enable when foreground fixed
				//[ToolTip.ForegroundProperty] = Brushes.Black, // this overrides the TextBlock Foreground property
			};
			AvaloniaUtils.AddContextMenu(TextBlock);

			var borderPaddingTitle = new Border()
			{
				BorderThickness = new Thickness(5, 2, 2, 2),
				BorderBrush = Theme.TitleBackground,
				Child = TextBlock,
			};
			containerGrid.Children.Add(borderPaddingTitle);

			// Notes
			// Add checkbox here for tabModel.Notes
			/*checkBox = new CheckBox()
			{
				IsChecked = (tabInstance.tabModel.Notes != null && tabInstance.tabViewSettings.NotesVisible),
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.White),
				Foreground = new SolidColorBrush(Theme.TitleForegroundColor),
				[Grid.ColumnProperty] = 1,
			};
			checkBox.Click += CheckBox_Click;*/

			if (TabInstance.Model.Notes != null && TabInstance.Model.Notes.Length > 0)
			{
				//Button button = new Button();
				Image image = AvaloniaAssets.Images.Info;
				image.Height = 20;
				Grid.SetColumn(image, 1);
				containerGrid.Children.Add(image); // always enable so they can add notes? (future thing)
				//containerGrid.Children.Add(checkBox); // always enable so they can add notes? (future thing)
			}

			var borderContent = new Border()
			{
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
			};
			borderContent.Child = containerGrid;

			Content = borderContent;
		}

		/*private void CheckBox_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tabInstance.tabViewSettings.NotesVisible = (bool)checkBox.IsChecked;
			tabInstance.SaveTabSettings();
			tabInstance.Reload();
		}*/

		public string Text
		{
			get
			{
				return Label;
			}
			set
			{
				Label = value;
				TextBlock.Text = value;
			}
		}
	}
}
