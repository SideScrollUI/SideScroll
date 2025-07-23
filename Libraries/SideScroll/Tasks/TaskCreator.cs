using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tasks;

public enum AccentType
{
	Default,
	Warning
}

public interface IFlyoutConfig
{
}

public class ConfirmationFlyoutConfig(string text, string confirmText, string cancelText = "Cancel") : IFlyoutConfig
{
	public string Text => text;
	public string ConfirmText => confirmText;
	public string? CancelText => cancelText;
}


public abstract class TaskCreator : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged; // Used only for INotifyPropertyChanged memory leak fix?

	public Action? OnComplete;

	[HiddenColumn]
	public string? Label { get; set; } // used for Button Label

	[HiddenColumn]
	public string? Description { get; set; } // Button hint text

	[HiddenColumn]
	public bool ShowTask { get; set; }

	[HiddenColumn]
	public bool UseTask { get; set; } // Blocks, Action uses UI thread if false

	[HiddenColumn]
	public AccentType AccentType { get; set; }

	[HiddenColumn]
	public IFlyoutConfig? Flyout { get; set; }

	[HiddenColumn]
	public SynchronizationContext? Context { get; set; }

	protected abstract Action CreateAction(Call call);

	public override string? ToString() => Label;

	public void Run(Call call)
	{
		TaskInstance taskInstance = Start(call);
		taskInstance.Task!.GetAwaiter().GetResult();
	}

	// Creates, Starts, and returns a new Task
	public TaskInstance Start(Call call)
	{
		Context ??= SynchronizationContext.Current ?? new();
		call.Log.Settings!.Context = Context;

		TaskInstance taskInstance = new()
		{
			Call = call,
			Creator = this,
		};
		call.TaskInstance = taskInstance;

		Action action = CreateAction(call);
		if (UseTask)
		{
			taskInstance.Task = new Task(action);
			taskInstance.Task.ContinueWith(_ => taskInstance.SetFinished());
			taskInstance.Task.Start();
		}
		else
		{
			action.Invoke();
			taskInstance.SetFinished();
		}

		return taskInstance;
	}
}
