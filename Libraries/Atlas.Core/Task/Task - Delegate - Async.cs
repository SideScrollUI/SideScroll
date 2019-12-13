using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskDelegateAsync : TaskCreator
	{
		public delegate Task CallActionAsync(Call call);

		private CallActionAsync callAction;

		public TaskDelegateAsync(string label, CallActionAsync callAction, bool showTask = false, string description = null)
		{
			this.Label = label;
			this.callAction = callAction;
			this.UseTask = true;
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
				// BeginInvoke() doesn't work for .NET Core
				//callAction.Invoke(call);
				//InvokeActionAsync(call);
				Task.Run(() => InvokeActionAsync(call)).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message, new Tag("Exception", e));
			}
		}

		private async Task InvokeActionAsync(Call call)
		{
			try
			{
				// BeginInvoke() doesn't work for .NET Core
				//return await callAction.BeginInvoke(call);
				await callAction.Invoke(call);
				//await taskInstance;
			}
			catch (Exception e)
			{
				call.log.AddError(e.Message, new Tag("Exception", e));
			}
		}
	}
}
