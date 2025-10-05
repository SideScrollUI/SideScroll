using SideScroll.Extensions;

namespace SideScroll.Tasks;

public delegate Task CallActionAsync(Call call);

public class TaskDelegateAsync : TaskCreator
{
	public CallActionAsync CallAction { get; }

	public override string? ToString() => Label;

	// Lists read easier with the label as the first param
	public TaskDelegateAsync(string label, CallActionAsync callAction, bool showTask = false, string? description = null)
	{
		Label = label;
		CallAction = callAction;
		UseTask = true;
		ShowTask = showTask;
		Description = description;
	}

	public TaskDelegateAsync(CallActionAsync callAction, bool showTask = false, string? description = null)
	{
		Label = callAction.Method.Name.TrimEnd("Async").WordSpaced();
		CallAction = callAction;
		UseTask = true;
		ShowTask = showTask;
		Description = description;
	}

	public override Action CreateAction(Call call)
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
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}
}
