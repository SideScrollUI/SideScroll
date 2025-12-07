using SideScroll.Extensions;

namespace SideScroll.Tasks;

public delegate Task CallActionAsync(Call call);

/// <summary>
/// A task creator that wraps an async CallActionAsync delegate for asynchronous task execution
/// </summary>
public class TaskDelegateAsync : TaskCreator
{
	public CallActionAsync CallAction { get; }

	public override string? ToString() => Label;

	/// <summary>
	/// Initializes a new async task delegate with the specified label and action
	/// </summary>
	public TaskDelegateAsync(string label, CallActionAsync callAction, bool useBackgroundThread = false, bool showTask = false, string? description = null)
	{
		Label = label;
		CallAction = callAction;
		UseTask = true;
		UseBackgroundThread = useBackgroundThread;
		ShowTask = showTask;
		Description = description;
	}

	/// <summary>
	/// Initializes a new async task delegate using the method name as the label
	/// </summary>
	public TaskDelegateAsync(CallActionAsync callAction, bool useBackgroundThread = false, bool showTask = false, string? description = null)
	{
		Label = callAction.Method.Name.TrimEnd("Async").WordSpaced();
		CallAction = callAction;
		UseTask = true;
		UseBackgroundThread = useBackgroundThread;
		ShowTask = showTask;
		Description = description;
	}

	/// <summary>
	/// Starts the async task, optionally running on a background thread
	/// </summary>
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

	/// <summary>
	/// Creates an action that will invoke the async call action with exception handling
	/// </summary>
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
