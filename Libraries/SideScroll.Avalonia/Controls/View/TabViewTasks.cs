using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.DataGrids;
using SideScroll.Logs;
using SideScroll.Tabs;
using SideScroll.Tasks;
using System.Collections;
using System.Collections.Specialized;

namespace SideScroll.Avalonia.Controls.View;

public class TabViewTasks : Grid, IDisposable
{
	public static int LineHeight { get; set; } = 34;

	public TabInstance TabInstance { get; }

	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

	public bool AutoSelectNew { get; set; } = true;

	private bool ShowTasks => TabInstance.Model.Tasks
		.Any(task =>
			task.ShowTask ||
			task.TaskStatus == TaskStatus.Faulted ||
			task.Log.Level >= LogLevel.Error);

	public IList SelectedItems => _tabDataGrid.SelectedItems;

	private readonly TabDataGrid _tabDataGrid;

	public override string ToString() => TabInstance.Model.Name;

	public TabViewTasks(TabInstance tabInstance)
	{
		TabInstance = tabInstance;

		ColumnDefinitions = new ColumnDefinitions("*");
		RowDefinitions = new RowDefinitions("Auto"); // doesn't work

		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		IsVisible = ShowTasks;

		_tabDataGrid = new TabDataGrid(TabInstance, TabInstance.Model.Tasks, false) // don't autogenerate
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Stretch,
			MaxHeight = 150,
		};

		_tabDataGrid.AddButtonColumn(nameof(TaskInstance.Cancel));
		_tabDataGrid.AddColumn("Task", nameof(TaskInstance.Label));
		_tabDataGrid.AddColumn("   %   ", nameof(TaskInstance.Percent));
		_tabDataGrid.AddColumn("Status", nameof(TaskInstance.Status));
		_tabDataGrid.AddColumn("Duration", nameof(TaskInstance.Duration));
		//_tabDataGrid.AddColumn("Message", nameof(TaskInstance.Message));

		_tabDataGrid.OnSelectionChanged += TabData_OnSelectionChanged;
		Children.Add(_tabDataGrid);

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
		if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex >= 0)
		{
			SelectLastItem();
		}
	}

	private void SelectLastItem()
	{
		_tabDataGrid.MinHeight = _tabDataGrid.DesiredSize.Height;
		MinHeight = _tabDataGrid.MinHeight;
		_tabDataGrid.InvalidateMeasure();
		IsVisible = ShowTasks;

		if (AutoSelectNew && TabInstance.Model.Tasks.Count > 0)
		{
			TaskInstance taskInstance = TabInstance.Model.Tasks.Last();
			if (_tabDataGrid.SelectedItem == taskInstance)
			{
				UpdateSelection();
			}
			else
			{
				_tabDataGrid.SelectedItem = taskInstance;
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

			_tabDataGrid.MinHeight = Math.Min(TabInstance.Model.Tasks.Count * LineHeight + LineHeight, 6 * LineHeight);
		}
	}

	private void TaskCompleted(TaskInstance taskInstance)
	{
		bool wasVisible = IsVisible;
		IsVisible = ShowTasks;

		// Unselect running if no error
		if (AutoSelectNew && !taskInstance.Errored)
		{
			IList selectedItems = _tabDataGrid.SelectedItems;
			if (selectedItems.Count == 1 && selectedItems[0] == taskInstance)
			{
				_tabDataGrid.SelectedItem = null;
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
		_tabDataGrid.OnSelectionChanged -= TabData_OnSelectionChanged;
		_tabDataGrid.Dispose();

		if (TabInstance.Model.Tasks is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged -= NotifyCollectionChanged_CollectionChanged;
		}
	}
}
