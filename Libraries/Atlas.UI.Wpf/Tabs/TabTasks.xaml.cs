using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Atlas.UI.Wpf
{
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
			//AddButtonColumn("<>", nameof(TaskInstance.Cancel));

			//DataStore = tabModel.Tasks;
			dataGrid.ItemsSource = tabModel.Tasks;

			//dataGrid.SelectionChanged += DataGrid_SelectionChanged; // doesn't catch cell selection, only row selections
			dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
			dataGrid.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
			dataGrid.MouseUp += DataGrid_MouseUp;

			tabModel.Tasks.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
		}

		private void AddColumn(string label, string propertyName)
		{
			DataGridTextColumn column = new DataGridTextColumn();
			column.Binding = new Binding(propertyName);
			//column.Sortable = true;

			column.Header = label;
			dataGrid.Columns.Add(column);
		}

		private void AddButtonColumn(string label, string methodName)
		{
			DataGridTemplateColumn column = new DataGridTemplateColumn();
			//column.CellTemplate.

			//column.Binding = new Binding(methodName);
			//column.Sortable = false;

			column.Header = label;
			dataGrid.Columns.Add(column);
		}

		// Override default behavior and unselect single cells if re-selected on a click
		private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependency = (DependencyObject)e.OriginalSource;

			DataGridRow dataGridRow = ItemsControl.ContainerFromElement(dataGrid, dependency) as DataGridRow;
			if (dataGridRow == null)
				return;
			/*if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1)
			{
				TextBlock textBlock = e.OriginalSource as TextBlock;
				if (textBlock != null && textBlock.DataContext is string && (string)textBlock.DataContext == "Stop")
					return;
				if (dataGridRow.IsSelected)
				{
					dataGridRow.IsSelected = false;
					e.Handled = true;
				}
			}*/


			FrameworkElement frameworkElement = dependency as FrameworkElement;
			if (frameworkElement == null)
				return;

			DataGridCell dataGridCell = ((FrameworkElement)dependency).Parent as DataGridCell;
			if (dataGridCell == null)
				return;

			//if (dataGridCell.Column is DataGridCheckBoxColumn)
			//	return;

			if (dataGrid.SelectedCells != null && dataGrid.SelectedCells.Count == 1)
			{
				//DataGridRow dataGridRow = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
				if (dataGridCell.IsSelected)
				{
					dataGridCell.IsSelected = false;
					e.Handled = true;
				}
			}
		}

		private void DataGrid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			autoSelectNew = (dataGrid.SelectedCells.Count == 0);
		}

		private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (autoSelectNew)
				{
					TaskInstance taskInstance = (TaskInstance)dataGrid.Items[e.NewStartingIndex];
					SelectedItem = taskInstance;
					taskInstance.OnComplete = () => TaskCompleted(taskInstance);
				}

				this.Visibility = Visibility.Visible;
			}
		}

		private void TaskCompleted(TaskInstance taskInstance)
		{
			if (autoSelectNew && dataGrid.SelectedItem == taskInstance)
				dataGrid.SelectedItem = null;
		}

		/*private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateSelection();
		}*/

		private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
		{
			UpdateSelection();
		}

		private void UpdateSelection()
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		/*public IList SelectedItems
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
		}*/

		public object SelectedItem
		{
			set
			{
				if (dataGrid.SelectedCells.Count != 1 || dataGrid.SelectedCells[0].Item != value)
				{
					dataGrid.SelectedCells.Clear();
					dataGrid.SelectedItem = value;
				}
				if (value != null)
					dataGrid.ScrollIntoView(value);
			}
		}

		// Returns list of items from selected rows
		public IList SelectedItems
		{
			get
			{
				SortedDictionary<int, object> orderedRows = new SortedDictionary<int, object>();
				foreach (DataGridCellInfo cellInfo in dataGrid.SelectedCells)
				{
					orderedRows[dataGrid.Items.IndexOf(cellInfo.Item)] = cellInfo.Item;
				}
				return orderedRows.Values.ToList();
			}
			set
			{
				foreach (object obj in value)
					dataGrid.SelectedItems.Add(obj);
				/*
				HashSet<object> idxSelected = new HashSet<object>();
				foreach (object obj in value)
					idxSelected.Add(obj);

				dataGrid.SelectedItems.Clear();
				foreach (DataGridViewRow row in dataGrid.Rows)
				{
					if (idxSelected.Contains(row.DataBoundItem))
						row.Cells[0].Selected = true;
				}*/
			}
		}

		public void Dispose()
		{
			dataGrid.ItemsSource = null;

			dataGrid.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
			//dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
			dataGrid.PreviewMouseLeftButtonDown -= DataGrid_PreviewMouseLeftButtonDown;
			dataGrid.MouseUp -= DataGrid_MouseUp;

			tabModel.Tasks.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
		}

		private void ButtonStopClicked(object sender, RoutedEventArgs e)
		{
			TaskInstance taskInstance = (sender as Button).DataContext as TaskInstance;
			taskInstance.Cancel();
		}
	}
}
