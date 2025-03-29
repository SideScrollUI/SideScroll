using SideScroll.Collections;

namespace SideScroll.Tasks;

public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	public int MaxTasks { get; set; } = 10;

	public TaskInstanceCollection() { }

	public TaskInstanceCollection(IEnumerable<TaskInstance> enumerable) :
		base(enumerable)
	{
	}

	public new void Add(TaskInstance taskInstance)
	{
		base.Add(taskInstance);

		if (Count > MaxTasks)
		{
			RemoveAt(0);
		}
	}
}
