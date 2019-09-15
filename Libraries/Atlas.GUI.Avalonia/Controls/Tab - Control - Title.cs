using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.IO;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlTitle : UserControl
	{
		public string Label { get; set; }
		private TextBlock title;
		private TabInstance tabInstance;
		private CheckBox checkBox;

		public TabControlTitle(TabInstance tabInstance, string name = null)
		{
			this.tabInstance = tabInstance;
			this.Label = name ?? tabInstance.Label;
			this.Label = new StringReader(Label).ReadLine();

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
			Background = new SolidColorBrush(Theme.TitleBackgroundColor);

			Grid containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("*,Auto"),
				RowDefinitions = new RowDefinitions("Auto"), // Header, Body
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				//Background = new SolidColorBrush(Theme.BackgroundColor),
			};

			// Add a wrapper class with a border?
			// Need to make the desired size for this a constant x
			title = new TextBlock()
			{
				Text = Label,
				FontSize = 14,
				//Margin = new Thickness(2), // Shows as black, Need Padding so Border not needed
				Background = new SolidColorBrush(Theme.TitleBackgroundColor),
				Foreground = new SolidColorBrush(Theme.TitleForegroundColor),
				HorizontalAlignment = HorizontalAlignment.Stretch,
				//HorizontalAlignment = HorizontalAlignment.Left,
				//[Grid.RowProperty] = 0,
				//[ToolTip.TipProperty] = Label,
			};

			Border borderPaddingTitle = new Border()
			{
				BorderThickness = new Thickness(5, 2, 2, 2),
				BorderBrush = new SolidColorBrush(Theme.TitleBackgroundColor),
				Child = title,
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

			if (tabInstance.tabModel.Notes != null && tabInstance.tabModel.Notes.Length > 0)
			{
				Button button = new Button();
				//button.St
				Image image = AvaloniaAssets.Images.Info;
				image.Height = 20;
				Grid.SetColumn(image, 1);
				//image.
				containerGrid.Children.Add(image); // always enable so they can add notes? (future thing)
				//containerGrid.Children.Add(checkBox); // always enable so they can add notes? (future thing)
			}


			Border borderContent = new Border()
			{
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
			};
			borderContent.Child = containerGrid;

			this.Content = borderContent;
		}

		private void CheckBox_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			tabInstance.tabViewSettings.NotesVisible = (bool)checkBox.IsChecked;
			tabInstance.SaveTabSettings();
			tabInstance.Reload();
		}

		public string Text
		{
			get
			{
				return Label;
			}
			set
			{
				Label = value;
				title.Text = value;
			}
		}
	}
}
