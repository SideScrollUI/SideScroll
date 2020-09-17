using System;
using System.Collections.Generic;

namespace Atlas.Core
{
	public class TaskDelegateParams : TaskCreator
	{
		public delegate void CallActionParams(Call call, params object[] objects);

		public Call Call;
		public CallActionParams CallAction;
		public object[] Objects;

		public override string ToString() => Label;

		public TaskDelegateParams(Call call, string label, CallActionParams callAction, bool useTask, string description, object[] objects)
		{
			Call = call;
			Label = label;
			CallAction = callAction;
			UseTask = useTask;
			Description = description;
			Objects = objects;
		}

		protected override Action CreateAction(Call call)
		{
			return () => InvokeAction(call);
		}

		private void InvokeAction(Call call)
		{
			try
			{
				CallAction.Invoke(call, Objects);
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}
	}
}
