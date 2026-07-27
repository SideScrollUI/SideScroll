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
}
