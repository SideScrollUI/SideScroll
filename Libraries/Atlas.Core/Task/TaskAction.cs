namespace Atlas.Core;

public class TaskAction : TaskCreator
{
	public Action Action;

	public override string? ToString() => Label;

	public TaskAction(string label, Action action, bool useTask = false)
	{
		Label = label;
		Action = action;
		UseTask = useTask;
	}

	protected override Action CreateAction(Call call)
	{
		return InvokeAction;
	}

	private void InvokeAction()
	{
		Action.Invoke();
	}
}
