using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	public class TaskDelegateParams : TaskCreator
	{
		public delegate void CallActionParams(Call call, params object[] objects);

		private CallActionParams callAction;
		private object[] objects;

		public TaskDelegateParams(string label, CallActionParams callAction, bool useTask, string description, object[] objects)
		{
			this.Label = label;
			this.callAction = callAction;
			this.UseTask = useTask;
			this.Description = description;
			this.objects = objects;
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
				callAction.Invoke(call, objects);
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message);
			}
		}
	}
}

/*
*/
