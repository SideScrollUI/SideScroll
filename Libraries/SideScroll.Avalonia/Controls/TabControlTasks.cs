using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.DataGrids;
using SideScroll.Logs;
using SideScroll.Tabs;
using SideScroll.Tasks;
using System.Collections;
using System.Collections.Specialized;

namespace SideScroll.Avalonia.Controls;

public class TabControlTasks : Grid, IDisposable
{
	private const int LineHeight = 34;

	public TabInstance TabInstance;

	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

	public bool AutoSelectNew { get; set; } = true;

	private bool ShowTasks => TabInstance.Model.Tasks
		.Any(task =>
			task.ShowTask ||
			task.TaskStatus == TaskStatus.Faulted ||
			task.Log.Level >= LogLevel.Error);

	public IList SelectedItems => _tabControlDataGrid.SelectedItems;

	private TabControlDataGrid _tabControlDataGrid;

	public override string ToString() => TabInstance.Model.Name;

	public TabControlTasks(TabInstance tabInstance)
	{
		TabInstance = tabInstance;

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto"); // doesn't work

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		IsVisible = ShowTasks;

		_tabControlDataGrid = new TabControlDataGrid(TabInstance, TabInstance.Model.Tasks, false) // don't autogenerate
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			MaxHeight = 150,
		};

		_tabControlDataGrid.AddButtonColumn(nameof(TaskInstance.Cancel));
		_tabControlDataGrid.AddColumn("Task", nameof(TaskInstance.Label));
		_tabControlDataGrid.AddColumn("   %   ", nameof(TaskInstance.Percent));
		_tabControlDataGrid.AddColumn("Status", nameof(TaskInstance.Status));
		//tabControlDataGrid.AddColumn("Message", nameof(TaskInstance.Message));

		_tabControlDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
		Children.Add(_tabControlDataGrid);

		if (TabInstance.Model.Tasks.Count > 0)
		{
			SelectLastItem();
		}

		if (TabInstance.Model.Tasks is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged += NotifyCollectionChanged_CollectionChanged;
		}
	}

	// not resizing correctly when we add a new item
	private void NotifyCollectionChanged_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		Dispatcher.UIThread.Post(() => CollectionChangedUI(e), DispatcherPriority.SystemIdle);
	}

	private void CollectionChangedUI(NotifyCollectionChangedEventArgs e)
	{
		if (_tabControlDataGrid == null)
			return;

		if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex >= 0)
		{
			SelectLastItem();
		}
	}

	private void SelectLastItem()
	{
		_tabControlDataGrid.MinHeight = _tabControlDataGrid.DesiredSize.Height;
		MinHeight = _tabControlDataGrid.MinHeight;
		_tabControlDataGrid.InvalidateMeasure();
		IsVisible = ShowTasks;

		if (AutoSelectNew && TabInstance.Model.Tasks.Count > 0)
		{
			TaskInstance taskInstance = TabInstance.Model.Tasks.Last();
			if (_tabControlDataGrid.SelectedItem == taskInstance)
			{
				UpdateSelection();
			}
			else
			{
				_tabControlDataGrid.SelectedItem = taskInstance;
			}

			// use lock internally?
			if (taskInstance.Finished)
			{
				TaskCompleted(taskInstance);
			}
			else
			{
				taskInstance.OnComplete = () => Dispatcher.UIThread.Post(() => TaskCompleted(taskInstance), DispatcherPriority.SystemIdle);
			}

			_tabControlDataGrid.MinHeight = Math.Min(TabInstance.Model.Tasks.Count * LineHeight + LineHeight, 6 * LineHeight);
		}
	}

	private void TaskCompleted(TaskInstance taskInstance)
	{
		bool wasVisible = IsVisible;
		IsVisible = ShowTasks;

		// Unselect running if no error
		if (AutoSelectNew && !taskInstance.Errored && _tabControlDataGrid != null)
		{
			IList selectedItems = _tabControlDataGrid.SelectedItems;
			if (selectedItems.Count == 1 && selectedItems[0] == taskInstance)
			{
				_tabControlDataGrid.SelectedItem = null;
			}
		}
		else if (IsVisible != wasVisible)
		{
			UpdateSelection();
		}
	}

	private void TabData_OnSelectionChanged(object? sender, EventArgs e)
	{
		UpdateSelection();
	}

	private void UpdateSelection()
	{
		if (IsVisible)
		{
			OnSelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs());
		}
	}

	public void Dispose()
	{
		_tabControlDataGrid.OnSelectionChanged -= TabData_OnSelectionChanged;
		_tabControlDataGrid.Dispose();

		if (TabInstance.Model.Tasks is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged -= NotifyCollectionChanged_CollectionChanged;
		}
	}
}
