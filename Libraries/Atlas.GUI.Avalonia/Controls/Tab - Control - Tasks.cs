using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Atlas.GUI.Avalonia.Controls
{
	public class TabControlTasks : Grid, IDisposable
	{
		private TabInstance tabInstance;
		private TabControlDataGrid tabControlDataGrid;


		public event EventHandler<EventArgs> OnSelectionChanged;
		private bool autoSelectNew = true;

		private TabControlTasks()
		{
			this.Initialize();
		}

		public TabControlTasks(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			Initialize();
		}

		public override string ToString()
		{
			return tabInstance.tabModel.Name;
		}

		private void Initialize()
		{
			InitializeControls();
		}

		/*protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

		protected override void OnMeasureInvalidated()
		{
			base.OnMeasureInvalidated();
			//Size size = tabDataGrid.dataGrid.DesiredSize;
		}*/

		private bool ShowTasks
		{
			get
			{
				foreach(var task in tabInstance.tabModel.Tasks)
				{
					if (task.ShowTask || task.TaskStatus == System.Threading.Tasks.TaskStatus.Faulted)
						return true;
				}
				return false;
			}
		}

		private void InitializeControls()
		{
			this.ColumnDefinitions = new ColumnDefinitions("*");
			this.RowDefinitions = new RowDefinitions("Auto"); // doesn't work
			//this.IsVisible = (tabInstance.tabModel.Tasks.Count > 0);
			this.IsVisible = ShowTasks;
			//this.Background = new SolidColorBrush(Colors.Blue);
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Stretch;
			//this.Orientation = Orientation.Vertical;

			tabControlDataGrid = new TabControlDataGrid(tabInstance, tabInstance.tabModel.Tasks, false); // don't autogenerate
			tabControlDataGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabControlDataGrid.VerticalAlignment = VerticalAlignment.Stretch;

			tabControlDataGrid.AddColumn("Task", nameof(TaskInstance.Label));
			tabControlDataGrid.AddColumn("   %   ", nameof(TaskInstance.Percent));
			tabControlDataGrid.AddColumn("Status", nameof(TaskInstance.Status));
			//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel)); // todo: No Button Column support

			//tabDataGrid.AutoLoad = tabModel.AutoLoad;
			tabControlDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
			//tabDataGrid.Initialize();
			//bool addSplitter = false;
			//tabParentControls.AddControl(tabDataGrid, true, false);
			this.Children.Add(tabControlDataGrid);
			//this.Content = tabDataGrid;

			INotifyCollectionChanged iNotifyCollectionChanged = tabInstance.tabModel.Tasks as INotifyCollectionChanged;
			if (iNotifyCollectionChanged != null)
				iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
		}

		// not resizing correctly when we add a new item
		private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			tabControlDataGrid.MinHeight = tabControlDataGrid.DesiredSize.Height;
			this.MinHeight = tabControlDataGrid.MinHeight;
			//tabDataGrid.dataGrid._measured = false; doesn't work
			//tabDataGrid.Measure(new Size(2000, 2000));
			tabControlDataGrid.InvalidateMeasure();
			//tabDataGrid.Height
			//InvalidateMeasure();
			//IsVisible = true;
			IsVisible = ShowTasks;

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (autoSelectNew)
				{
					TaskInstance taskInstance = (TaskInstance)tabInstance.tabModel.Tasks[e.NewStartingIndex];
					tabControlDataGrid.SelectedItem = taskInstance;
					taskInstance.OnComplete = () => TaskCompleted(taskInstance);
					int lineHeight = 26;
					tabControlDataGrid.MinHeight = Math.Min(tabInstance.tabModel.Tasks.Count * lineHeight + lineHeight, 6 * lineHeight);
				}

				//this.Visibility = Visibility.Visible;
			}
		}

		private void TaskCompleted(TaskInstance taskInstance)
		{
			if (autoSelectNew)
			{
				IList selectedItems = tabControlDataGrid.SelectedItems;
				if (selectedItems.Count == 1 && selectedItems[0] == taskInstance)
					tabControlDataGrid.SelectedItem = null;
			}
			IsVisible = ShowTasks;
		}

		private void TabData_OnSelectionChanged(object sender, EventArgs e)
		{
			UpdateSelection();
		}

		public IList SelectedItems
		{
			get
			{
				return tabControlDataGrid.SelectedItems;
			}
		}

		private void UpdateSelection()
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		public void Dispose()
		{
			tabControlDataGrid.Dispose();
			tabControlDataGrid.OnSelectionChanged -= TabData_OnSelectionChanged;
		}
	}
}

/* From Atlas.GUI.Wpf
	public partial class TabTasks : UserControl, IDisposable
	{
		public TabTasks()
		{
			InitializeComponent();
		}
		private TabModel tabModel;

		public event EventHandler<EventArgs> OnSelectionChanged;
		private bool autoSelectNew = true;

		public TabTasks(TabModel tabModel)
		{
			this.tabModel = tabModel;

			InitializeComponent();
			this.Visibility = Visibility.Collapsed;
		}

		public void Initialize()
		{
			dataGrid.AutoGenerateColumns = false;
			AddColumn("Task", nameof(TaskInstance.Label));
			AddColumn("   %   ", nameof(TaskInstance.Percent));
			AddColumn("Status", nameof(TaskInstance.Status));

			//DataStore = tabModel.Tasks;
			dataGrid.ItemsSource = tabModel.Tasks;

			dataGrid.SelectionChanged += DataGrid_SelectionChanged;
			dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
			dataGrid.MouseUp += DataGrid_MouseUp;

			tabModel.Tasks.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
		}

		private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DataGridRow dataGridRow = ItemsControl.ContainerFromElement(dataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
			if (dataGridRow == null)
				return;
			if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1)
			{
				if (dataGridRow.IsSelected)
				{
					dataGridRow.IsSelected = false;
					e.Handled = true;
				}
			}
		}

		private void DataGrid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			autoSelectNew = (dataGrid.SelectedCells.Count == 0);
		}

		private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateSelection();
		}

		private void UpdateSelection()
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		private void AddColumn(string label, string propertyName)
		{
			DataGridTextColumn column = new DataGridTextColumn();
			column.Binding = new Binding(propertyName);
			//column.Sortable = true;
			
			column.Header = label;
			dataGrid.Columns.Add(column);
		}

		public IList SelectedItems
		{
			get
			{
				SortedDictionary<int, object> orderedRows = new SortedDictionary<int, object>();
				foreach (object obj in dataGrid.SelectedItems)
				{
					orderedRows[dataGrid.Items.IndexOf(obj)] = obj;
				}
				return orderedRows.Values.ToList();
			}
		}
	}
*/
