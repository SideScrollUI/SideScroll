using NUnit.Framework;
using SideScroll.Tasks;

namespace SideScroll.Tests;

[Category("Call")]
public class CallTests : BaseTest
{
	private readonly List<int?> _input = [null, 0, 1];

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("Call");
	}

	private async Task<int?> EchoAsync(Call call, int? index)
	{
		await Task.Delay(1);
		return index;
	}

	[Test]
	public async Task FirstNonNullAsync()
	{
		var result = await Call.FirstNonNullAsync(EchoAsync, _input);

		Assert.That(result, Is.EqualTo(0));
	}

	[Test]
	public async Task SelectNonNullAsync()
	{
		var result = await Call.SelectNonNullAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(2).Items);
		Assert.That(result, Is.EquivalentTo([0, 1]));
	}

	[Test]
	public async Task RunAsync()
	{
		var result = await Call.RunAsync(EchoAsync, _input);

		Assert.That(result, Has.Exactly(3).Items);
		Assert.That(result.Keys, Is.EquivalentTo(_input));
		Assert.That(result.Values, Is.EquivalentTo(_input));
		Assert.That(result.NonNullValues, Is.EquivalentTo([0, 1]));
	}

	[Test, Description("Cancelling returns only the items that ran, not empty placeholders")]
	public async Task RunAsyncCancelled()
	{
		Call call = new()
		{
			TaskInstance = new TaskInstance(),
		};
		call.TaskInstance!.Cancel();

		var result = await call.RunAsync(EchoAsync, _input);

		Assert.That(result, Is.Empty);
	}

	[Test, Description("Cancelling partway keeps the results that already started")]
	public async Task RunAsyncCancelledPartway()
	{
		Call call = new()
		{
			TaskInstance = new TaskInstance(),
		};

		List<int?> items = [1, 2, 3, 4];

		// One at a time so the second item can't start until the first has already cancelled
		var result = await call.RunAsync(async (c, item) =>
		{
			call.TaskInstance!.Cancel();
			await Task.Delay(1);
			return item;
		}, items, maxConcurrentRequests: 1);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result.Keys, Is.EqualTo(new int?[] { 1 }));
	}

	[Test, Description(
		"Cancelling takes effect while every slot is still held. Waiting for a slot has to pass the " +
		"cancel token in, or work that never finishes would keep the cancellation from being noticed")]
	[CancelAfter(30_000)]
	public async Task RunAsyncCancelledWaitingForSlot()
	{
		Call call = new()
		{
			TaskInstance = new TaskInstance(),
		};

		TaskCompletionSource cancelLogged = new(TaskCreationOptions.RunContinuationsAsynchronously);
		call.Log.OnMessage += (_, e) =>
		{
			if (e.Entries.Any(entry => entry.Text == "Cancelled"))
			{
				cancelLogged.TrySetResult();
			}
		};

		// The only slot stays held until the gate opens, so the second item has to wait for it
		TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
		TaskCompletionSource holding = new(TaskCreationOptions.RunContinuationsAsynchronously);

		Task<ItemResultCollection<int?, int?>> run = call.RunAsync(async (c, item) =>
		{
			holding.TrySetResult();
			await gate.Task;
			return item;
		}, new int?[] { 1, 2 }, maxConcurrentRequests: 1);

		await holding.Task;
		call.TaskInstance!.Cancel();

		// Would only complete once the gate opened if the wait ignored the token
		try
		{
			await cancelLogged.Task.WaitAsync(TimeSpan.FromSeconds(2));
		}
		finally
		{
			// Always release the running item, including when the assertion times out
			gate.TrySetResult();
		}

		ItemResultCollection<int?, int?> result = await run;

		Assert.That(result.Keys, Is.EqualTo(new int?[] { 1 }), "The second item never started.");
	}
}
