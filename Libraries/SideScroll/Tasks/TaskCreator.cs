using SideScroll.Attributes;
using System.ComponentModel;

namespace SideScroll.Tasks;

/// <summary>
/// Defines the visual accent style for a task action
/// </summary>
public enum AccentType
{
	/// <summary>
	/// Default visual style
	/// </summary>
	Default,
	
	/// <summary>
	/// Warning visual style to indicate caution
	/// </summary>
	Warning
}

/// <summary>
/// Marker interface for flyout configuration
/// </summary>
public interface IFlyoutConfig;

/// <summary>
/// Configuration for a confirmation flyout dialog
/// </summary>
public class ConfirmationFlyoutConfig(string text, string confirmText, string cancelText = "Cancel") : IFlyoutConfig
{
	/// <summary>
	/// Gets the confirmation message text
	/// </summary>
	public string Text => text;
	
	/// <summary>
	/// Gets the text for the confirmation button
	/// </summary>
	public string ConfirmText => confirmText;
	
	/// <summary>
	/// Gets the text for the cancel button
	/// </summary>
	public string CancelText => cancelText;
}

/// <summary>
/// Base class for creating and managing task execution with support for threading, logging, and progress tracking
/// </summary>
public abstract class TaskCreator : INotifyPropertyChanged
{
	/// <summary>
	/// Occurs when a property value changes
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged; // Used only for INotifyPropertyChanged memory leak fix?

	/// <summary>
	/// Gets or sets the action to execute when the task completes
	/// </summary>
	[HiddenColumn]
	public Action? OnComplete { get; set; }

	/// <summary>
	/// Gets or sets the label text for the task button
	/// </summary>
	[HiddenColumn]
	public string? Label { get; set; }

	/// <summary>
	/// Gets or sets the description hint text for the task button
	/// </summary>
	[HiddenColumn]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets whether to show the task instance in the UI
	/// Actions will still create a TaskInstance even if a UseTask isn't enabled
	/// </summary>
	[HiddenColumn]
	public bool ShowTask { get; set; }

	/// <summary>
	/// Gets or sets whether to run the task asynchronously on a background thread
	/// Blocks if false, Action uses UI thread if false
	/// </summary>
	[HiddenColumn]
	public bool UseTask { get; set; }

	/// <summary>
	/// Gets or sets whether to use a background thread for task execution
	/// </summary>
	[HiddenColumn]
	public bool UseBackgroundThread { get; set; }

	/// <summary>
	/// Gets or sets the visual accent type for the task action
	/// </summary>
	[HiddenColumn]
	public AccentType AccentType { get; set; }

	/// <summary>
	/// Gets or sets the flyout configuration for confirmation dialogs
	/// </summary>
	[HiddenColumn]
	public IFlyoutConfig? Flyout { get; set; }

	/// <summary>
	/// Gets or sets the synchronization context for thread marshalling
	/// </summary>
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
