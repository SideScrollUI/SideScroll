using System;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskDelegateAsync : TaskCreator
	{
		public delegate Task CallActionAsync(Call call);

		private CallActionAsync callAction;

		public override string ToString() => Label;

		public TaskDelegateAsync(string label, CallActionAsync callAction, bool showTask = false, string description = null)
		{
			this.Label = label;
			this.callAction = callAction;
			this.UseTask = true;
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
				Task.Run(() => InvokeActionAsync(call)).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				call.Log.AddError(e.Message, new Tag("Exception", e));
			}
		}

		private async Task InvokeActionAsync(Call call)
		{
			try
			{
				await callAction.Invoke(call);
				//await taskInstance;
			}
			catch (Exception e)
			{
				call.Log.AddError(e.Message, new Tag("Exception", e));
			}
		}
	}
}
