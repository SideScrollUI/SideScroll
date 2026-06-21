using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using SideScroll.Avalonia.Controls.DataGrids;
using SideScroll.Avalonia.Tabs;
using SideScroll.Tabs;
using SideScroll.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace SideScroll.Avalonia.Controls.View;

/// <summary>
/// A grid panel that displays the background tasks associated with a <see cref="TabInstance"/> in a data grid,
/// auto-selecting new tasks and raising selection-changed events so child tabs update accordingly.
/// </summary>
public class TabViewTasks : Grid, IDisposable
{
	/// <summary>Gets or sets the row height in pixels used for each task entry.</summary>
	public static int LineHeight { get; set; } = 34;

	/// <summary>Gets the tab instance whose tasks are displayed.</summary>
	public TabInstance TabInstance { get; }

	/// <summary>Raised when the task selection changes.</summary>
	public event EventHandler<TabSelectionChangedEventArgs>? OnSelectionChanged;

	/// <summary>Gets or sets whether newly added tasks are automatically selected.</summary>
	public bool AutoSelectNew { get; set; } = true;

	private bool ShowTasks => TabInstance.TasksVisible;

	/// <summary>
	/// Gets the currently selected tasks wrapped as <see cref="TabTaskInstance"/> tabs, which display the
	/// task's log and a toolbar button to copy the Call.Log as JSON. Wrappers are cached per task so the
	/// child controls can be reused across selection changes.
	/// </summary>
	public IList SelectedItems =>
		_tabDataGrid.SelectedItems
			.OfType<TaskInstance>()
			.Select(taskInstance => _tabTaskInstances.GetValue(taskInstance, ti => new TabTaskInstance(ti)))
			.ToList();

	private readonly ConditionalWeakTable<TaskInstance, TabTaskInstance> _tabTaskInstances = new();

	private readonly TabDataGrid _tabDataGrid;

	public override string ToString() => TabInstance.Model.Name;

	/// <summary>Initializes the tasks panel for the given tab instance.</summary>
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
