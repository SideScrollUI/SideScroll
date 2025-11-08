using SideScroll.Extensions;

namespace SideScroll.Tasks;

public delegate Task CallActionAsync(Call call);

public class TaskDelegateAsync : TaskCreator
{
	public CallActionAsync CallAction { get; }

	public override string? ToString() => Label;

	// Lists read easier with the label as the first param
	public TaskDelegateAsync(string label, CallActionAsync callAction, bool useBackgroundThread = false, bool showTask = false, string? description = null)
	{
		Label = label;
		CallAction = callAction;
		UseTask = true;
		UseBackgroundThread = useBackgroundThread;
		ShowTask = showTask;
		Description = description;
	}

	public TaskDelegateAsync(CallActionAsync callAction, bool useBackgroundThread = false, bool showTask = false, string? description = null)
	{
		Label = callAction.Method.Name.TrimEnd("Async").WordSpaced();
		CallAction = callAction;
		UseTask = true;
		UseBackgroundThread = useBackgroundThread;
		ShowTask = showTask;
		Description = description;
	}

	public override Task StartTask(Call call)
	{
		if (UseBackgroundThread)
		{
			return Task.Run(async () => await InvokeActionAsync(call));
		}
		else
		{
			return InvokeActionAsync(call);
		}
	}

	public override Action CreateAction(Call call)
	{
		return async () => await InvokeActionAsync(call);
	}

	private async Task InvokeActionAsync(Call call)
	{
		try
		{
			await CallAction.Invoke(call);
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}
}
