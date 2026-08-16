using NUnit.Framework;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Tests;

/// <summary>
/// A tab cancels the tasks it started when it closes, which it tracks separately from the ones
/// it displays
/// </summary>
public class TabInstanceTaskDisposeTests
{
	[Test, Description(
		"A task that opted out of being shown was never added to Model.Tasks, which is the only " +
		"collection Dispose() cancelled, so it kept running against the closed tab")]
	public void HiddenTaskIsCancelledOnDispose()
	{
		var tabInstance = new TabInstance();
		var taskInstance = new TaskInstance();

		tabInstance.AddTask(taskInstance, showTask: false);

		Assert.That(tabInstance.Model.Tasks, Does.Not.Contain(taskInstance), "precondition: not displayed");

		CancellationToken cancelToken = taskInstance.CancelToken;
		tabInstance.Dispose();

		Assert.That(cancelToken.IsCancellationRequested, Is.True);
	}

	[Test, Description(
		"Model.Tasks caps at MaxTasks and drops the oldest, so a shown task was evicted from the " +
		"collection Dispose() cancelled once the tab had started more than that many")]
	public void ShownTaskEvictedByTheDisplayCapIsCancelledOnDispose()
	{
		var tabInstance = new TabInstance();
		var taskInstance = new TaskInstance();

		tabInstance.AddTask(taskInstance, showTask: true);
		CancellationToken cancelToken = taskInstance.CancelToken;

		for (int i = 0; i <= tabInstance.Model.Tasks.MaxTasks; i++)
		{
			tabInstance.AddTask(new TaskInstance(), showTask: true);
		}

		Assert.That(tabInstance.Model.Tasks, Does.Not.Contain(taskInstance), "precondition: evicted");

		tabInstance.Dispose();

		Assert.That(cancelToken.IsCancellationRequested, Is.True);
	}

	[Test, Description("A displayed task is still cancelled, the case that already worked")]
	public void ShownTaskIsCancelledOnDispose()
	{
		var tabInstance = new TabInstance();
		var taskInstance = new TaskInstance();

		tabInstance.AddTask(taskInstance, showTask: true);

		Assert.That(tabInstance.Model.Tasks, Does.Contain(taskInstance));

		CancellationToken cancelToken = taskInstance.CancelToken;
		tabInstance.Dispose();

		Assert.That(cancelToken.IsCancellationRequested, Is.True);
	}

	[Test, Description("Tracking every task for cancellation doesn't put it in front of the user")]
	public void TrackingATaskDoesNotDisplayIt()
	{
		var tabInstance = new TabInstance();

		tabInstance.AddTask(new TaskInstance(), showTask: false);

		Assert.That(tabInstance.Model.Tasks, Is.Empty);
		Assert.That(tabInstance.TasksVisible, Is.False);
	}

	[Test, Description(
		"Tasks are tracked until the tab closes, so a finished one is dropped rather than held " +
		"for as long as the tab is open")]
	public void FinishedTasksDoNotAccumulate()
	{
		var tabInstance = new TabInstance();

		var finished = new TaskInstance { Finished = true };
		tabInstance.AddTask(finished, showTask: false);

		var running = new TaskInstance();
		tabInstance.AddTask(running, showTask: false);

		CancellationToken finishedToken = finished.CancelToken;
		CancellationToken runningToken = running.CancelToken;
		tabInstance.Dispose();

		Assert.That(runningToken.IsCancellationRequested, Is.True, "the running task is still cancelled");
		Assert.That(finishedToken.IsCancellationRequested, Is.False, "the finished one was dropped on the next add");
	}
}
