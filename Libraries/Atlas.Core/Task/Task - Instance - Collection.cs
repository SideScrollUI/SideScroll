using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	public class TaskInstanceCollection : ItemCollection<TaskInstance>
	{
		public new void Add(TaskInstance taskInstance)
		{
			base.Add(taskInstance);
			if (Count > 10) // fixme
				this.RemoveAt(0);
		}
	}
}
