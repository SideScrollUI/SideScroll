using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Core
{
	public class TaskInstanceCollection : ItemCollection<TaskInstance>
	{
		public new void Add(TaskInstance taskInstance)
		{
			base.Add(taskInstance);
			if (Count > 100) // fixme
				this.RemoveAt(0);
		}
	}
}
