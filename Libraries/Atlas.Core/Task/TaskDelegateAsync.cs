using System;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskDelegateAsync : TaskCreator
	{
		public delegate Task CallActionAsync(Call call);

		public CallActionAsync CallAction;

		public override string ToString() => Label;

		public TaskDelegateAsync(string label, CallActionAsync callAction, bool showTask = false, string description = null)
		{
			Label = label;
			CallAction = callAction;
			UseTask = true;
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
				Task.Run(() => InvokeActionAsync(call)).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}

		private async Task InvokeActionAsync(Call call)
		{
			try
			{
				await CallAction.Invoke(call);
				//await taskInstance;
			}
			catch (Exception e)
			{
				call.Log.Add(e);
			}
		}
	}
}
