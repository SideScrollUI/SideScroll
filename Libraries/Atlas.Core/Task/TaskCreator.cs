using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Core;

public abstract class TaskCreator : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;
	public Action OnComplete;

	[HiddenColumn]
	public string Label { get; set; } // used for Button Label

	[HiddenColumn]
	public string Description { get; set; } // Button hint text

	public string Info { get { return Description != null ? ">" : null; } } // Button hint text

	[HiddenColumn]
	public bool ShowTask { get; set; }

	[HiddenColumn]
	public bool UseTask { get; set; } // Blocks, Action uses UI thread if false

	public int TimesRun { get; set; }

	[HiddenColumn]
	public SynchronizationContext Context;

	protected abstract Action CreateAction(Call call);

	public override string ToString() => Label;

	public void Run(Call call)
	{
		TaskInstance taskInstance = Start(call);
		taskInstance.Task.GetAwaiter().GetResult();
	}

	// Creates, Starts, and returns a new Task
	public TaskInstance Start(Call call)
	{
		TimesRun++;
		Context = SynchronizationContext.Current ?? new SynchronizationContext();
		call.Log.Settings.Context = Context;

		var taskInstance = new TaskInstance()
		{
			Call = call,
			Creator = this,
		};
		call.TaskInstance = taskInstance;

		Action action = CreateAction(call);
		if (UseTask)
		{
			taskInstance.Task = new Task(action);
			//currentTask.CreationOptions = TaskCreationOptions.
			taskInstance.Task.ContinueWith(task => taskInstance.SetFinished());
			taskInstance.Task.Start();
		}
		else
		{
			action.Invoke();
			taskInstance.SetFinished();
		}

		return taskInstance;
	}

	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		Context.Post(new SendOrPostCallback(NotifyPropertyChangedContext), propertyName);
		//PropertyChanged?.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), EndAsyncEvent, null);
		//PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void NotifyPropertyChangedContext(object state)
	{
		string propertyName = state as string;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		//PropertyChanged?.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), EndAsyncEvent, null);
	}

	/*protected void EndAsyncEvent(IAsyncResult iar)
	{
		var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
		var invokedMethod = (PropertyChangedEventHandler)ar.AsyncDelegate;

		try
		{
			invokedMethod.EndInvoke(iar);
		}
		catch
		{
			// Handle any exceptions that were thrown by the invoked method
			Console.WriteLine("An event listener went kaboom!");
		}
	}*/
}

/*
Recreate task?
Cancellation class
Special Task Class
Add Cancel() to call class

class CancelTask
{

}

Fix current databinding?

	do we need to copy values over?

	Synchronization context

New Databinding?
	
	BindingSource.ResetBindings()

	Still need to be on UI thread

	bindingSource.SuspendBinding()
	bindingSource.ResumeBinding()

Task Factory
	Requires a new Action each time?
	Can run different types of Actions (bad?)
*/
