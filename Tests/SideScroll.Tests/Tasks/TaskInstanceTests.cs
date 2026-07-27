using NUnit.Framework;
using SideScroll.Tasks;

namespace SideScroll.Tests.Tasks;

[Category("Core")]
public class TaskInstanceTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Core");
	}

	// Queues callbacks instead of running them, so Finished stays false until it's drained
	private class QueuedContext : SynchronizationContext
	{
		private readonly List<(SendOrPostCallback Callback, object? State)> _queued = [];

		public override void Post(SendOrPostCallback callback, object? state)
		{
			_queued.Add((callback, state));
		}

		public void Drain()
		{
			foreach ((SendOrPostCallback callback, object? state) in _queued.ToList())
			{
				callback(state);
			}
			_queued.Clear();
		}
	}

	private class TestTaskCreator : TaskCreator
	{
		public override Action CreateAction(Call call) => () => { };
	}

	[Test, Description("Finishing twice only runs the completion logic once")]
	public void SetFinishedIsOnlyCalledOnce()
	{
		TaskInstance taskInstance = new();
		int completed = 0;
		taskInstance.OnComplete = () => completed++;

		taskInstance.SetFinished();
		taskInstance.SetFinished();

		Assert.That(completed, Is.EqualTo(1));
		Assert.That(taskInstance.Finished, Is.True);
	}

	[Test, Description("A posted OnFinished() doesn't set Finished until it runs, so it can't guard the second call")]
	public void SetFinishedIsOnlyPostedOnce()
	{
		QueuedContext context = new();
		TaskInstance taskInstance = new()
		{
			Creator = new TestTaskCreator { Context = context },
		};
		int completed = 0;
		taskInstance.OnComplete = () => completed++;

		// Finished is still false between these, the completion is only queued
		taskInstance.SetFinished();
		taskInstance.SetFinished();
		Assert.That(taskInstance.Finished, Is.False);

		context.Drain();

		Assert.That(completed, Is.EqualTo(1));
		Assert.That(taskInstance.Finished, Is.True);
	}

	[Test, Description("Finishing from multiple threads still only completes once")]
	public void SetFinishedIsThreadSafe()
	{
		TaskInstance taskInstance = new();
		int completed = 0;
		taskInstance.OnComplete = () => Interlocked.Increment(ref completed);

		Parallel.For(0, 32, _ => taskInstance.SetFinished());

		Assert.That(completed, Is.EqualTo(1));
	}

	[Test]
	public void FirstSubTask_ReportsProgressToParent()
	{
		TaskInstance parent = new();
		TaskInstance child = parent.AddSubTask(new Call());

		child.Progress = 50;

		Assert.That(child.ProgressMax, Is.EqualTo(100));
		Assert.That(child.Percent, Is.EqualTo(50));
		Assert.That(parent.Percent, Is.EqualTo(50));
	}

	[Test, Description("A negative task limit is treated as zero instead of removing from an empty collection")]
	public void TaskCollection_NegativeMaxTasks_IsClampedToZero()
	{
		TaskInstanceCollection tasks = [new TaskInstance(), new TaskInstance()];

		Assert.DoesNotThrow(() => tasks.MaxTasks = -1);
		Assert.That(tasks.MaxTasks, Is.Zero);
		Assert.That(tasks, Is.Empty);
		Assert.DoesNotThrow(() => tasks.Add(new TaskInstance()));
		Assert.That(tasks, Is.Empty);
	}

	[Test, Description("A synchronous action failure is logged and the task still finishes")]
	public void SynchronousTaskFailureFinishes()
	{
		var creator = new TaskAction("Fail", () => throw new InvalidOperationException("Expected"));

		TaskInstance task = creator.Start(new Call());

		Assert.That(SpinWait.SpinUntil(() => task.Finished, TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(task.Errored, Is.True);
		Assert.That(task.Message, Is.EqualTo("Expected"));
		Assert.That(task.Log.Level, Is.GreaterThanOrEqualTo(SideScroll.Logs.LogLevel.Error));
	}

	[Test]
	public void TaskCreatorRun_SynchronousActionCompletesWithoutTask()
	{
		bool invoked = false;
		var creator = new TaskAction("Run", () => invoked = true);

		Assert.DoesNotThrow(() => creator.Run(new Call()));
		Assert.That(invoked, Is.True);
	}

	[Test]
	public void BackgroundTaskFailureIsLogged()
	{
		var creator = new TaskAction(
			"Fail",
			() => throw new InvalidOperationException("Background failure"),
			useTask: true);

		TaskInstance task = creator.Start(new Call());

		Assert.That(SpinWait.SpinUntil(() => task.Finished, TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(task.Errored, Is.True);
		Assert.That(task.Message, Is.EqualTo("Background failure"));
		Assert.That(task.Log.Level, Is.GreaterThanOrEqualTo(SideScroll.Logs.LogLevel.Error));
		Assert.That(task.Log.Items.Any(entry => entry.Text == "Background failure"), Is.True);
	}

	[Test]
	public void SubTaskCallPointsToSubTask()
	{
		TaskInstance parent = new();
		Call childCall = new("Child");

		TaskInstance child = parent.AddSubTask(childCall);

		Assert.That(childCall.TaskInstance, Is.SameAs(child));
	}

	[Test]
	public void CompletedZeroItemTaskReportsOneHundredPercent()
	{
		TaskInstance task = new() { TaskCount = 0 };

		task.SetFinished();

		Assert.That(task.Finished, Is.True);
		Assert.That(task.Percent, Is.EqualTo(100));
	}

	[Test]
	public void RootDisposeReleasesOwnedCancellationSource()
	{
		TaskInstance task = new();
		CancellationToken token = task.CancelToken;

		task.SetFinished();
		Assert.DoesNotThrow(() => task.AddSubTask(new Call()));

		task.Dispose();

		Assert.DoesNotThrow(() => _ = token.IsCancellationRequested);
		Assert.Throws<ObjectDisposedException>(() => _ = task.TokenSource.Token);
		Assert.DoesNotThrow(task.Cancel);
	}

	[Test]
	public void SubTaskDisposeDoesNotDisposeSharedCancellationSource()
	{
		TaskInstance parent = new();
		TaskInstance child = parent.AddSubTask(new Call());

		child.Dispose();

		Assert.DoesNotThrow(parent.Cancel);
	}
}
