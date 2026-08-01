using SideScroll.Collections;
using SideScroll.Logs;

namespace SideScroll.Tasks;

/// <summary>
/// A collection of task instances that automatically limits the number of stored tasks by removing oldest entries when the maximum is exceeded
/// </summary>
public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	/// <summary>
	/// Gets or sets the maximum number of tasks to keep in the collection (default: 10)
	/// </summary>
	public int MaxTasks
	{
		get => _maxTasks;
		set
		{
			_maxTasks = Math.Max(0, value);
			TrimToMaxTasks();
		}
	}
	private int _maxTasks = 10;

	/// <summary>
	/// Initializes a new empty task instance collection
	/// </summary>
	public TaskInstanceCollection() { }

	/// <summary>
	/// Initializes a new task instance collection with the specified items
	/// </summary>
	public TaskInstanceCollection(IEnumerable<TaskInstance> enumerable) :
		base(enumerable)
	{
		TrimToMaxTasks();
	}

	/// <summary>
	/// Inserts a task instance, removing the oldest entries if the collection exceeds MaxTasks
	/// </summary>
	/// <remarks>
	/// Overriding this instead of hiding Add() so the limit still applies to tasks
	/// added through a base class reference
	/// </remarks>
	protected override void InsertItem(int index, TaskInstance taskInstance)
	{
		base.InsertItem(index, taskInstance);

		TrimToMaxTasks();
	}

	/// <summary>
	/// Adds multiple task instances, removing the oldest entries if the collection exceeds MaxTasks
	/// </summary>
	public override void AddRange(IEnumerable<TaskInstance> collection)
	{
		// Adds to Items directly, so it never reaches InsertItem()
		base.AddRange(collection);

		TrimToMaxTasks();
	}

	private void TrimToMaxTasks()
	{
		while (Count > MaxTasks)
		{
			RemoveAt(0);
		}
	}

	/// <summary>
	/// Whether any task in the collection warrants display: it explicitly opted in
	/// (<see cref="TaskInstance.ShowTask"/>), faulted, or logged an error.
	/// </summary>
	public bool ShowTasks => this.Any(task =>
			task.ShowTask ||
			task.TaskStatus == TaskStatus.Faulted ||
			task.Log.Level >= LogLevel.Error);
}
