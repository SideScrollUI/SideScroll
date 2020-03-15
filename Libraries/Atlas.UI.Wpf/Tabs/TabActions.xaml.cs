using Atlas.Core;
using Atlas.Tabs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Atlas.UI.Wpf
{
	public partial class TabActions : UserControl, IDisposable
	{
		private TabInstance tabInstance;
		private TabModel tabModel;

		private ItemCollection<TaskCreator> taskItems;

		public TabActions(TabInstance tabInstance, TabModel tabModel, ItemCollection<TaskCreator> taskItems)
		{
			this.tabInstance = tabInstance;
			this.tabModel = tabModel;
			this.taskItems = taskItems;
			InitializeComponent();
		}

		public void Initialize()
		{
			dataGrid.AutoGenerateColumns = false;
			dataGrid.ItemsSource = (IEnumerable<object>)tabModel.Actions;
			AddColumn("?", nameof(TaskCreator.Info));
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

		private void ButtonClicked(object sender, RoutedEventArgs e)
		{
			TaskCreator taskCreator = (sender as Button).DataContext as TaskCreator;
			Call call = new Call(taskCreator.Label);
			TaskInstance taskInstance = taskCreator.Start(call);
			tabModel.Tasks.Add(taskInstance);
		}

		public void Dispose()
		{
			dataGrid.ItemsSource = null;
		}
	}
}
