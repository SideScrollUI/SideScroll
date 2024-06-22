namespace SideScroll.Core.Tasks;

public class TaskInstanceCollection : ItemCollection<TaskInstance>
{
	public new void Add(TaskInstance taskInstance)
	{
		base.Add(taskInstance);

		if (Count > 10) // fixme
			RemoveAt(0);
	}
}
