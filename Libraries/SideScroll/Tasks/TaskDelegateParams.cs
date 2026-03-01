namespace SideScroll.Tasks;

/// <summary>
/// Represents a method that receives a Call parameter and additional object parameters
/// </summary>
public delegate void CallActionParams(Call call, params object[] objects);

/// <summary>
/// A task creator that wraps a CallActionParams delegate which receives a Call parameter and additional object parameters
/// </summary>
public class TaskDelegateParams : TaskCreator
{
	/// <summary>
	/// Gets or sets the Call context to use for the task
	/// </summary>
	public Call Call { get; set; }

	/// <summary>
	/// Gets the action delegate to execute
	/// </summary>
	public CallActionParams CallAction { get; }

	/// <summary>
	/// Gets the additional parameters to pass to the action
	/// </summary>
	public object[] Objects { get; }

	public override string? ToString() => Label;

	/// <summary>
	/// Initializes a new task delegate with parameters
	/// </summary>
	/// <param name="call">The call context to use, or null to create a new one</param>
	/// <param name="label">The label for the task</param>
	/// <param name="callAction">The action to execute</param>
	/// <param name="useTask">Whether to execute on a background task</param>
	/// <param name="description">Optional description for the task</param>
	/// <param name="objects">Additional parameters to pass to the action</param>
	public TaskDelegateParams(Call? call, string label, CallActionParams callAction, bool useTask, string? description, object[] objects)
	{
		Call = call ?? new();
		Label = label;
		CallAction = callAction;
		UseTask = useTask;
		UseBackgroundThread = useTask;
		Description = description;
		Objects = objects;
	}

	/// <summary>
	/// Creates an action that will invoke the call action with the stored parameters and exception handling
	/// </summary>
	public override Action CreateAction(Call call)
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
			if (call.TaskInstance is TaskInstance taskInstance)
			{
				taskInstance.Errored = true;
				taskInstance.Message = e.Message;
			}
			call.Log.Add(e);
		}
	}
}
