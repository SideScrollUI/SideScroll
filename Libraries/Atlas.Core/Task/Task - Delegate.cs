using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	public class TaskDelegate : TaskCreator
	{
		public delegate void CallAction(Call call);

		private CallAction callAction;

		public TaskDelegate(string label, CallAction callAction, bool useTask = false, bool showTask = false, string description = null)
		{
			this.Label = label;
			this.callAction = callAction;
			this.UseTask = useTask;
			this.ShowTask = showTask;
			this.Description = description;
		}

		public override string ToString()
		{
			return Label;
		}

		protected override Action CreateAction(Call call)
		{
			return () => InvokeAction(call);
		}

		private void InvokeAction(Call call)
		{
			try
			{
				callAction.Invoke(call);
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message, new Tag("Exception", e));
			}
		}
	}
}
