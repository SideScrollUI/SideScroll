using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tasks;

public enum AccentType
{
	Default,
	Warning
}

public interface IFlyoutConfig;

public class ConfirmationFlyoutConfig(string text, string confirmText, string cancelText = "Cancel") : IFlyoutConfig
{
	public string Text => text;
	public string ConfirmText => confirmText;
	public string CancelText => cancelText;
}


/// <summary>
/// Base class for creating and managing task execution with support for threading, logging, and progress tracking
/// </summary>
public abstract class TaskCreator : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged; // Used only for INotifyPropertyChanged memory leak fix?

	[HiddenColumn]
	public Action? OnComplete { get; set; }

	[HiddenColumn]
	public string? Label { get; set; } // used for Button Label

	[HiddenColumn]
	public string? Description { get; set; } // Button hint text

	[HiddenColumn]
	public bool ShowTask { get; set; } // Shows TaskInstance, Actions will still create a TaskInstance even if a UseTask isn't enabled

	[HiddenColumn]
	public bool UseTask { get; set; } // Blocks, Action uses UI thread if false

	[HiddenColumn]
	public bool UseBackgroundThread { get; set; }

	[HiddenColumn]
	public AccentType AccentType { get; set; }

	[HiddenColumn]
	public IFlyoutConfig? Flyout { get; set; }

	[HiddenColumn]
	public SynchronizationContext? Context { get; set; }

	public override string? ToString() => Label;

	/// <summary>
	/// Synchronously runs the task and waits for completion
	/// </summary>
	public void Run(Call call)
	{
		TaskInstance taskInstance = Start(call);
		taskInstance.Task!.GetAwaiter().GetResult();
	}

	/// <summary>
	/// Creates a new task instance without starting it
	/// </summary>
	public TaskInstance Create(Call call)
	{
		Context ??= SynchronizationContext.Current ?? new();
		call.Log.Settings!.Context = Context;

		TaskInstance taskInstance = new()
		{
			Call = call,
			Creator = this,
		};
		call.TaskInstance = taskInstance;
		return taskInstance;
	}

	/// <summary>
	/// Creates and starts a new task instance
	/// </summary>
	/// <remarks>
	/// If UseTask is not enabled, will wait for action completion before returning
	/// </remarks>
	public TaskInstance Start(Call call)
	{
		TaskInstance taskInstance = Create(call);
		taskInstance.Start();
		return taskInstance;
	}

	/// <summary>
	/// Creates an action delegate that will execute the task logic
	/// </summary>
	public abstract Action CreateAction(Call call);

	/// <summary>
	/// Starts the task asynchronously and returns the Task
	/// </summary>
	public virtual Task StartTask(Call call)
	{
		Action action = CreateAction(call);
		return Task.Run(action);
	}
}
