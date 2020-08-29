﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskDelegate : TaskCreator
	{
		public delegate void CallAction(Call call);

		private CallAction Action;

		public override string ToString() => Label;

		public TaskDelegate(string label, CallAction callAction, bool useTask = false, bool showTask = false, string description = null)
		{
			Label = label;
			Action = callAction;
			UseTask = useTask;
			ShowTask = showTask;
			Description = description;
		}

		protected override Action CreateAction(Call call)
		{
			return () => InvokeAction(call);
		}

		private void InvokeAction(Call call)
		{
			try
			{
				// BeginInvoke() doesn't work for .NET Core
				Action.Invoke(call); // any await in the Invoked call will make this return and finish the task
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}

		private void InvokeAction2(Call call)
		{
			try
			{
				Task.Run(() => Action.Invoke(call)).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}
	}
}
