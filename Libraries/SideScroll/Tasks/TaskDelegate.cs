namespace SideScroll.Tasks;

/// <summary>
/// Represents a method that receives a Call parameter for logging and cancellation support
/// </summary>
public delegate void CallAction(Call call);

/// <summary>
/// A task creator that wraps a CallAction delegate which receives a Call parameter for logging and cancellation support
/// </summary>
public class TaskDelegate : TaskCreator
{
	/// <summary>
	/// Gets the action delegate to execute
	/// </summary>
	public CallAction Action { get; }

	public override string? ToString() => Label;

	/// <summary>
	/// Initializes a new task delegate with the specified label and action
	/// </summary>
	public TaskDelegate(string label, CallAction callAction, bool useTask = false, bool showTask = false, string? description = null)
	{
		Label = label;
		Action = callAction;
		UseTask = useTask;
		UseBackgroundThread = useTask;
		ShowTask = showTask;
		Description = description;
	}

	/// <summary>
	/// Initializes a new task delegate using the method name as the label
	/// </summary>
	public TaskDelegate(CallAction callAction, bool useTask = false, bool showTask = false, string? description = null)
	{
		Label = callAction.Method.Name;
		Action = callAction;
		UseTask = useTask;
		UseBackgroundThread = useTask;
		ShowTask = showTask;
		Description = description;
	}

	/// <summary>
	/// Creates an action that will invoke the call action with exception handling
	/// </summary>
	public override Action CreateAction(Call call)
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
			if (call.TaskInstance is TaskInstance taskInstance)
			{
				taskInstance.Errored = true;
				taskInstance.Message = e.Message;
			}
			call.Log.Add(e);
		}
	}
}
