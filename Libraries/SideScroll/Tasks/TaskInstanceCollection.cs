using SideScroll.Collections;

namespace SideScroll.Tasks;

/// <summary>
/// A collection of task instances that automatically limits the number of stored tasks by removing oldest entries when the maximum is exceeded
/// </summary>
public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	public int MaxTasks { get; set; } = 10;

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
	}

	/// <summary>
	/// Adds a task instance and automatically removes the oldest entry if the collection exceeds MaxTasks
	/// </summary>
	public new void Add(TaskInstance taskInstance)
	{
		base.Add(taskInstance);

		if (Count > MaxTasks)
		{
			RemoveAt(0);
		}
	}
}
