using SideScroll.Collections;

namespace SideScroll.Tasks;

public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	public int MaxTasks { get; set; } = 10;

	public new void Add(TaskInstance taskInstance)
	{
		base.Add(taskInstance);

		if (Count > MaxTasks)
		{
			RemoveAt(0);
		}
	}
}
