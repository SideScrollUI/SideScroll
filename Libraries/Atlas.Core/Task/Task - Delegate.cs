using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskDelegate : TaskCreator
	{
		public delegate void CallAction(Call call);

		private CallAction callAction;

		public override string ToString() => Label;

		public TaskDelegate(string label, CallAction callAction, bool useTask = false, bool showTask = false, string description = null)
		{
			this.Label = label;
			this.callAction = callAction;
			this.UseTask = useTask;
			this.ShowTask = showTask;
			this.Description = description;
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
				callAction.Invoke(call); // any await in the Invoked call will make this return and finish the task
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message, new Tag("Exception", e));
			}
		}

		private void InvokeAction2(Call call)
		{
			try
			{
				Task.Run(() => callAction.Invoke(call)).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message, new Tag("Exception", e));
			}
		}
	}
}
