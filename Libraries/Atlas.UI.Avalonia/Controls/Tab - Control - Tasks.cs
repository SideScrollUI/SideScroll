using Atlas.Core;
using Atlas.Tabs;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Specialized;

namespace Atlas.UI.Avalonia.Controls
{
	public class TabControlTasks : Grid, IDisposable
	{
		private TabInstance tabInstance;
		private TabControlDataGrid tabControlDataGrid;

		public event EventHandler<EventArgs> OnSelectionChanged;
		private bool autoSelectNew = true;

		private TabControlTasks()
		{
			Initialize();
		}

		public TabControlTasks(TabInstance tabInstance)
		{
			this.tabInstance = tabInstance;
			Initialize();
		}

		public override string ToString() => tabInstance.Model.Name;

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
				foreach(var task in tabInstance.Model.Tasks)
				{
					if (task.ShowTask || task.TaskStatus == System.Threading.Tasks.TaskStatus.Faulted)
						return true;
				}
				return false;
			}
		}

		private void InitializeControls()
		{
			ColumnDefinitions = new ColumnDefinitions("*");
			RowDefinitions = new RowDefinitions("Auto"); // doesn't work
			IsVisible = ShowTasks;
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;

			tabControlDataGrid = new TabControlDataGrid(tabInstance, tabInstance.Model.Tasks, false); // don't autogenerate
			tabControlDataGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
			tabControlDataGrid.VerticalAlignment = VerticalAlignment.Stretch;

			tabControlDataGrid.AddColumn("Task", nameof(TaskInstance.Label));
			tabControlDataGrid.AddColumn("   %   ", nameof(TaskInstance.Percent));
			tabControlDataGrid.AddColumn("Status", nameof(TaskInstance.Status));
			//tabControlDataGrid.AddColumn("Message", nameof(TaskInstance.Message));
			//tabDataGrid.AddButtonColumn("<>", nameof(TaskInstance.Cancel)); // todo: No Button Column support

			//tabDataGrid.AutoLoad = tabModel.AutoLoad;
			tabControlDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
			//tabDataGrid.Initialize();
			Children.Add(tabControlDataGrid);

			if (tabInstance.Model.Tasks is INotifyCollectionChanged iNotifyCollectionChanged)
				iNotifyCollectionChanged.CollectionChanged += INotifyCollectionChanged_CollectionChanged;
		}

		// not resizing correctly when we add a new item
		private void INotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Dispatcher.UIThread.Post(() => CollectionChangedUI(e));
		}

		private void CollectionChangedUI(NotifyCollectionChangedEventArgs e)
		{
			tabControlDataGrid.MinHeight = tabControlDataGrid.DesiredSize.Height;
			MinHeight = tabControlDataGrid.MinHeight;
			//tabDataGrid.dataGrid._measured = false; doesn't work
			//tabDataGrid.Measure(new Size(2000, 2000));
			tabControlDataGrid.InvalidateMeasure();
			//tabDataGrid.Height
			//InvalidateMeasure();
			//IsVisible = true;
			IsVisible = ShowTasks;

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (autoSelectNew && e.NewStartingIndex >= 0)
				{
					TaskInstance taskInstance = tabInstance.Model.Tasks[e.NewStartingIndex];
					tabControlDataGrid.SelectedItem = taskInstance;
					taskInstance.OnComplete = () => TaskCompleted(taskInstance);
					int lineHeight = 26;
					tabControlDataGrid.MinHeight = Math.Min(tabInstance.Model.Tasks.Count * lineHeight + lineHeight, 6 * lineHeight);
				}

				//this.Visibility = Visibility.Visible;
			}
		}

		private void TaskCompleted(TaskInstance taskInstance)
		{
			bool wasVisible = IsVisible;
			IsVisible = ShowTasks;

			// unselect running if no error
			if (autoSelectNew && !taskInstance.Errored)
			{
				IList selectedItems = tabControlDataGrid.SelectedItems;
				if (selectedItems.Count == 1 && selectedItems[0] == taskInstance)
					tabControlDataGrid.SelectedItem = null;
			}
			else if (IsVisible != wasVisible)
			{
				UpdateSelection();
			}
		}

		private void TabData_OnSelectionChanged(object sender, EventArgs e)
		{
			if (IsVisible)
				UpdateSelection();
		}

		public IList SelectedItems => tabControlDataGrid.SelectedItems;

		private void UpdateSelection()
		{
			OnSelectionChanged?.Invoke(this, null);
		}

		public void Dispose()
		{
			tabControlDataGrid.OnSelectionChanged -= TabData_OnSelectionChanged;
			tabControlDataGrid.Dispose();

			if (tabInstance.Model.Tasks is INotifyCollectionChanged iNotifyCollectionChanged)
				iNotifyCollectionChanged.CollectionChanged -= INotifyCollectionChanged_CollectionChanged;
		}
	}
}
