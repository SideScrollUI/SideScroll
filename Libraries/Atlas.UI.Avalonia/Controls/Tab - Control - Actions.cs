using Atlas.Core;
using Atlas.Tabs;
using Avalonia;
using Avalonia.Controls;
using System.Collections.Generic;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlActions : UserControl
	{
		private TabInstance tabInstance;
		private TabModel tabModel;
		
		private ItemCollection<TaskCreator> taskItems;
		public bool GridInitialized { get; private set; } = false;
		private Dictionary<Button, TaskCreator> taskCreators = new Dictionary<Button, TaskCreator>();

		public TabControlActions(TabInstance tabInstance, TabModel tabModel, ItemCollection<TaskCreator> taskItems)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabModel;
			this.taskItems = taskItems;

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
			/*StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Vertical;

			TextBlock labelTitle = new TextBlock()
			{
				Text = "Actions",
				//Margin = new Thickness(10, 0, 0, 0), // needs Padding so Border not required
				Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				Foreground = new SolidColorBrush(Theme.GridColumnHeaderForegroundColor),
			};
			//stackPanel.Children.Add(labelTitle);

			Border borderTitle = new Border()
			{
				BorderThickness = new Thickness(5, 2, 2, 2),
				//Background = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
				BorderBrush = new SolidColorBrush(Theme.GridColumnHeaderBackgroundColor),
			};
			borderTitle.Child = labelTitle;
			stackPanel.Children.Add(borderTitle);*/

			Grid containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,*"),
				//RowDefinitions = new RowDefinitions("*"),
				//HorizontalAlignment = HorizontalAlignment.Stretch
			};

			int rowIndex = 0;
			foreach (TaskCreator taskCreator in tabModel.Actions)
			{
				RowDefinition gridRow = new RowDefinition();
				gridRow.Height = new GridLength(1, GridUnitType.Auto);
				containerGrid.RowDefinitions.Add(gridRow);

				Button button = new TabControlButton(taskCreator.Label);
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
				taskCreators[button] = taskCreator;
				//stackPanel.Children.Add(button);
				Grid.SetRow(button, rowIndex++);
				containerGrid.Children.Add(button);
			}
			//containerGrid.RowDefinitions[0]

			//containerStackPanel.Children.Add(containerStackPanel);
			Content = containerGrid;


			/*Grid containerGrid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
				RowDefinitions = new RowDefinitions("*"),
				//HorizontalAlignment = HorizontalAlignment.Stretch
			};

			Grid leftGrid = new Grid()
			{
				[Grid.ColumnProperty] = 0,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};
			containerGrid.Children.Add(leftGrid);

			TextBlock title = new TextBlock()
			{
				Text = tabModel.Name,
				Background = new SolidColorBrush(Color.Parse("#4f52bb")),
				Foreground = new SolidColorBrush(Color.Parse("#ffffff")),
				[Grid.RowProperty] = 0,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};
			//leftGrid.Children.Add(title);
			AddParentControl(title, false, false);

			/*GridSplitter gridSplitter = new GridSplitter { [Grid.ColumnProperty] = 1, Background = Brushes.Black };
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

			TextBlock child = new TextBlock()
			{
				Text = "Child",
			};
			rightGrid.Children.Add(child);*/
		}

		private void Button_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e)
		{
			Button button = (Button)sender;
			TaskCreator taskCreator = taskCreators[button];
			tabInstance.StartTask(taskCreator, taskCreator.ShowTask);
			//this.UnselectAll();
		}

		/*public IList SelectedItemsOrdered
		{
			get
			{
				SortedDictionary<int, object> orderedItems = new SortedDictionary<int, object>();
				foreach (DataGridViewCell cell in dataGridView.SelectedCells)
				{
					orderedItems.Add(cell.RowIndex, cell.OwningRow.DataBoundItem);
				}
				return orderedItems.Values.ToList();
			}
		}

		private void dataGridView_SelectionChanged(object sender, EventArgs e)
		{
			OnSelectionChanged?.Invoke(this, null);
		}
		
		*/
	}
}

/*
This needs to get converted to a Grid, but there's no button column yet
*/
