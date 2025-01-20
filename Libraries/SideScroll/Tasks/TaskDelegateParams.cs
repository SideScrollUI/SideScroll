namespace SideScroll.Tasks;

public delegate void CallActionParams(Call call, params object[] objects);

public class TaskDelegateParams : TaskCreator
{
	public Call Call { get; set; }
	public CallActionParams CallAction { get; init; }
	public object[] Objects { get; set; }

	public override string? ToString() => Label;

	public TaskDelegateParams(Call? call, string label, CallActionParams callAction, bool useTask, string? description, object[] objects)
	{
		Call = call ?? new();
		Label = label;
		CallAction = callAction;
		UseTask = useTask;
		Description = description;
		Objects = objects;
	}

	protected override Action CreateAction(Call call)
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
			call.Log.Add(e);
		}
	}
}
