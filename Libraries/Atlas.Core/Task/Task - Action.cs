using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	public class TaskAction : TaskCreator
	{
		private Action action;

		public override string ToString() => Label;

		public TaskAction(string label, Action action, bool useTask = false)
		{
			this.Label = label;
			this.action = action;
			this.UseTask = useTask;
		}

		protected override Action CreateAction(Call call)
		{
			return () => InvokeAction();
		}

		private void InvokeAction()
		{
			action.Invoke();
		}
	}
}
