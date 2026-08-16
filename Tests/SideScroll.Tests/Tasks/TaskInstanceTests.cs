using NUnit.Framework;
using SideScroll.Logs;
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

	/// <summary>
	/// Waits for a failed task to reach its final state before asserting on it
	/// </summary>
	/// <remarks>
	/// The failure state is written from more than one thread. The failure itself sets Errored and
	/// Message, the log entry goes through Log.Settings.Context, and Finished is set by a posted
	/// OnFinished(). With no ambient context TaskCreator falls back to a plain
	/// SynchronizationContext, whose Post() queues to the thread pool, so Finished can be observed
	/// before the rest of the state has landed. Waiting on Finished alone made these tests flaky
	/// </remarks>
	private static void AssertTaskFailed(TaskInstance task, string message)
	{
		bool reachedFinalState = SpinWait.SpinUntil(() =>
			task.Finished &&
			task.Errored &&
			task.Message == message &&
			task.Log.Level >= LogLevel.Error &&
			HasLogEntry(task.Log, message),
			TimeSpan.FromSeconds(10));

		Assert.That(reachedFinalState, Is.True,
			$"Finished={task.Finished}, Errored={task.Errored}, " +
			$"Message={task.Message ?? "(null)"}, Level={task.Log.Level}");
	}

	// Entries are added under a lock this can't take, so enumerating can collide with a posted
	// add. This only runs inside a spin, so treating a collision as "not yet" just retries
	private static bool HasLogEntry(Log log, string message)
	{
		try
		{
			return log.Items.Any(entry => entry.Text == message);
		}
		catch (InvalidOperationException)
		{
			return false;
		}
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

		AssertTaskFailed(task, "Expected");
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

		AssertTaskFailed(task, "Background failure");
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

	// The source is released from a continuation, so it isn't disposed the moment Dispose() returns
	private static bool SpinUntilSourceDisposed(TaskInstance task)
	{
		return SpinWait.SpinUntil(() =>
		{
			try
			{
				_ = task.TokenSource.Token;
				return false;
			}
			catch (ObjectDisposedException)
			{
				return true;
			}
		}, TimeSpan.FromSeconds(5));
	}

	[Test]
	[Description(
		"Disposing the source out from under running work makes Token and Register throw for " +
		"anything still in flight, so an unfinished task has to release it on completion instead")]
	public void RunningTaskDefersDisposingItsCancellationSource()
	{
		using ManualResetEventSlim gate = new();
		TaskInstance task = new()
		{
			Task = Task.Run(() => gate.Wait(TimeSpan.FromSeconds(5))),
		};

		task.Dispose();

		Assert.DoesNotThrow(() => _ = task.TokenSource.Token,
			"The source has to stay usable while the task is still running");

		gate.Set();
		Assert.That(task.Task.Wait(TimeSpan.FromSeconds(5)), Is.True);

		Assert.That(SpinUntilSourceDisposed(task), Is.True,
			"The source should be released once the task completes");
	}

	[Test]
	public void CompletedTaskDisposesItsCancellationSourceImmediately()
	{
		TaskInstance task = new()
		{
			Task = Task.CompletedTask,
		};

		task.Dispose();

		Assert.Throws<ObjectDisposedException>(() => _ = task.TokenSource.Token);
	}

	[Test]
	[Description("Sub-tasks share the root's source, so cancelling one after the root is disposed would throw")]
	public void DisposingARootMakesSubTaskCancelSafe()
	{
		TaskInstance parent = new();
		TaskInstance child = parent.AddSubTask(new Call());

		parent.Dispose();

		Assert.DoesNotThrow(child.Cancel);
	}

	[Test]
	[Description(
		"Work queued before a tab closed still runs afterwards and creates sub-tasks from the " +
		"disposed one, which threw ObjectDisposedException reading the released source's token")]
	public void SubTaskOfADisposedParentIsCreatedCancelled()
	{
		TaskInstance parent = new();
		parent.Dispose();

		TaskInstance child = parent.AddSubTask(new Call());

		Assert.That(child.CancelToken.IsCancellationRequested, Is.True);
		Assert.DoesNotThrow(() => _ = child.TokenSource.Token, "it owns a live source rather than the released one");
		Assert.DoesNotThrow(child.Cancel);
	}

	[Test]
	[Description("Call.Timer() builds a sub-task, which is how a save after the tab closed reached this")]
	public void TimerOnACallWhoseTaskWasDisposedDoesNotThrow()
	{
		Call call = new("outer");
		TaskInstance taskInstance = new("outer");
		call.TaskInstance = taskInstance;

		taskInstance.Dispose();

		Assert.DoesNotThrow(() =>
		{
			using CallTimer timer = call.Timer("Saving object");
		});
	}

	[Test]
	[Description("A sub-task of a live parent still shares its source, so cancelling the parent stops it")]
	public void SubTaskOfALiveParentSharesItsSource()
	{
		TaskInstance parent = new();
		TaskInstance child = parent.AddSubTask(new Call());

		Assert.That(child.CancelToken.IsCancellationRequested, Is.False);

		parent.Cancel();

		Assert.That(child.CancelToken.IsCancellationRequested, Is.True);
	}
}
