using NUnit.Framework;
using SideScroll.Logs;

namespace SideScroll.Tests;

[Category("Call")]
public class CallTimerTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Call");
	}

	private static int CountEntries(Log log, string text) => log.Items.Count(entry => entry.Text == text);

	[Test, Description("Stopping early then disposing shouldn't log the duration twice")]
	public void StopIsIdempotent()
	{
		Call call = new();
		CallTimer timer = call.Timer("Test");

		timer.Stop();
		timer.Stop();
		timer.Dispose();

		Assert.That(CountEntries(timer.Log, "Finished"), Is.EqualTo(1));
	}

	[Test]
	public void DisposeStopsTheTimer()
	{
		Call call = new();
		CallTimer timer = call.Timer("Test");

		timer.Dispose();

		Assert.That(CountEntries(timer.Log, "Finished"), Is.EqualTo(1));
	}

	[Test, Description("Stopping early then disposing only finishes the task once")]
	public void StopOnlyFinishesTaskOnce()
	{
		Call call = new();
		using CallTimer timer = call.StartTask("Test");

		int completed = 0;
		timer.TaskInstance!.OnComplete = () => completed++;

		timer.Stop();
		timer.Stop();

		Assert.That(completed, Is.EqualTo(1));
	}
}
