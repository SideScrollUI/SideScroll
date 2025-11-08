namespace SideScroll.Tasks;

public delegate void CallAction(Call call);

public class TaskDelegate : TaskCreator
{
	public CallAction Action { get; }

	public override string? ToString() => Label;

	// Lists read easier with a label as the first param
	public TaskDelegate(string label, CallAction callAction, bool useTask = false, bool showTask = false, string? description = null)
	{
		Label = label;
		Action = callAction;
		UseTask = useTask;
		UseBackgroundThread = useTask;
		ShowTask = showTask;
		Description = description;
	}

	public TaskDelegate(CallAction callAction, bool useTask = false, bool showTask = false, string? description = null)
	{
		Label = callAction.Method.Name;
		Action = callAction;
		UseTask = useTask;
		UseBackgroundThread = useTask;
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

	/*private void InvokeAction2(Call call)
	{
		try
		{
			Task.Run(() => Action.Invoke(call)).GetAwaiter().GetResult();
		}
		catch (Exception e)
		{
			call.Log.Add(e);
		}
	}*/
}
