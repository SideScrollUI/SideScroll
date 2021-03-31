using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlActions : UserControl
	{
		public TabInstance TabInstance;
		public TabModel TabModel;
		public ItemCollection<TaskCreator> TaskItems;

		public bool GridInitialized { get; private set; }

		private readonly Dictionary<Button, TaskCreator> _taskCreators = new Dictionary<Button, TaskCreator>();

		public TabControlActions(TabInstance tabInstance, TabModel tabModel, ItemCollection<TaskCreator> taskItems)
		{
			TabInstance = tabInstance;
			TabModel = tabModel;
			TaskItems = taskItems;

			InitializeControls();
		}

		private void InitializeControls()
		{
			//HorizontalContentAlignment = HorizontalAlignment.Stretch;
			//VerticalContentAlignment = VerticalAlignment.Stretch;
			//BackgroundColor = Theme.BackgroundColor;

			//AddColumn("Action", "Label");

			//GridLines = GridLines.Both;
			//DataStore = (IEnumerable<object>)tabModel.Actions;

			// we don't need to resize so don't use Grid?
			/*var stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Vertical;

			var labelTitle = new TextBlock()
			{
				Text = "Actions",
				//Margin = new Thickness(10, 0, 0, 0), // needs Padding so Border not required
				Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				Foreground = new SolidColorBrush(Theme.GridColumnHeaderForegroundColor),
			};
			//stackPanel.Children.Add(labelTitle);

			var borderTitle = new Border()
			{
				BorderThickness = new Thickness(5, 2, 2, 2),
				//Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				BorderBrush = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
			};
			borderTitle.Child = labelTitle;
			stackPanel.Children.Add(borderTitle);*/

			var containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,*"),
				//RowDefinitions = new RowDefinitions("*"),
				//HorizontalAlignment = HorizontalAlignment.Stretch
			};

			int rowIndex = 0;
			foreach (TaskCreator taskCreator in TabModel.Actions)
			{
				var rowDefinition = new RowDefinition()
				{
					Height = new GridLength(1, GridUnitType.Auto),
				};
				containerGrid.RowDefinitions.Add(rowDefinition);

				var button = new TabControlButton(taskCreator.Label);
				//button.Styles.Add(new Style
				/*
				
  <Style Selector="Button:pointerover /template/ ContentPresenter">
	<Setter Property="BorderBrush" Value="#FF000000"/>
	<Setter Property="Background" Value="#FF7827d4"/>
  </Style>
				*/

				/*button.Styles.Add(new Style(x => x.OfType<Button>()) // .Class("ContentPresenter").Class("bar").Class("baz")
				{
					Setters = new[]
					{
						new Setter(Button.BackgroundProperty, new SolidColorBrush(Theme.ButtonBackgroundColor)),
					}
				});*/
				button.Margin = new Thickness(4, 2);
				button.Click += Button_Click;
				_taskCreators[button] = taskCreator;
				//stackPanel.Children.Add(button);
				Grid.SetRow(button, rowIndex++);
				containerGrid.Children.Add(button);
			}
			//containerGrid.RowDefinitions[0]

			//containerStackPanel.Children.Add(containerStackPanel);
			Content = containerGrid;


			/*var containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
				RowDefinitions = new RowDefinitions("*"),
				//HorizontalAlignment = HorizontalAlignment.Stretch
			};

			var leftGrid = new Grid()
			{
				[Grid.ColumnProperty] = 0,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};
			containerGrid.Children.Add(leftGrid);

			var title = new TextBlock()
			{
				Text = tabModel.Name,
				Background = new SolidColorBrush(Color.Parse("#4f52bb")),
				Foreground = new SolidColorBrush(Color.Parse("#ffffff")),
				[Grid.RowProperty] = 0,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};
			//leftGrid.Children.Add(title);
			AddParentControl(title, false, false);

			/*var gridSplitter = new GridSplitter { [Grid.ColumnProperty] = 1, Background = Brushes.Black };
			if (tabConfiguration.SplitterDistance != null)
				leftGrid.Width = (double)tabConfiguration.SplitterDistance;
			containerGrid.Children.Add(gridSplitter);
			gridSplitter.DragCompleted += GridSplitter_DragCompleted;

			rightGrid = new Grid()
			{
				[Grid.ColumnProperty] = 2,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};
			containerGrid.Children.Add(rightGrid);

			var child = new TextBlock()
			{
				Text = "Child",
			};
			rightGrid.Children.Add(child);*/
		}

		private void Button_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Button button = (Button)sender;
			TaskCreator taskCreator = _taskCreators[button];
			TabInstance.StartTask(taskCreator, taskCreator.ShowTask);
			//this.UnselectAll();
		}
	}
}

/*
This needs to get converted to a Grid, but there's no button column yet
*/
