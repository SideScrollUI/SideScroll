namespace SideScroll.Tasks;

/// <summary>
/// A task creator that wraps a simple Action delegate for execution
/// </summary>
public class TaskAction : TaskCreator
{
	public Action Action { get; }

	public override string? ToString() => Label;

	/// <summary>
	/// Initializes a new task action with the specified label and action
	/// </summary>
	public TaskAction(string label, Action action, bool useTask = false)
	{
		Label = label;
		Action = action;
		UseTask = useTask;
		UseBackgroundThread = useTask;
	}

	/// <summary>
	/// Creates an action that will invoke the wrapped action
	/// </summary>
	public override Action CreateAction(Call call)
	{
		return InvokeAction;
	}

	private void InvokeAction()
	{
		Action.Invoke();
	}
}
