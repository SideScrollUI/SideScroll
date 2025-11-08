namespace SideScroll.Tasks;

public delegate void CallActionParams(Call call, params object[] objects);

public class TaskDelegateParams : TaskCreator
{
	public Call Call { get; set; }
	public CallActionParams CallAction { get; }
	public object[] Objects { get; set; }

	public override string? ToString() => Label;

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
