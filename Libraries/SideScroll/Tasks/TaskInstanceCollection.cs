using SideScroll.Collections;

namespace SideScroll.Tasks;

public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	public int MaxTasks { get; set; } = 10;

	public TaskInstanceCollection() { }

	public TaskInstanceCollection(IEnumerable<TaskInstance> iEnumerable) :
		base(iEnumerable)
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
