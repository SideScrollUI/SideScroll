using NUnit.Framework;
using SideScroll.Logs;
using SideScroll.Tasks;

namespace SideScroll.Tabs.Tests;

[Category("Tabs")]
public class TabInstanceDisposeTests : BaseTest
{
	private class ThrowingAsyncTab : TabInstanceAsync
	{
		public override Task LoadAsync(Call call, TabModel model) =>
			Task.FromException(new InvalidOperationException("Async load failed"));
	}

	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("TabInstanceDispose");
	}

	[Test]
	[Description("TaskInstance owns a CancellationTokenSource and is built to be disposed, but nothing released it")]
	public void DisposeReleasesTheOwnTaskCancellationSource()
	{
		TabInstance tabInstance = new();

		tabInstance.Dispose();

		Assert.Throws<ObjectDisposedException>(() => _ = tabInstance.TaskInstance.TokenSource.Token);
	}

	[Test]
	public void DisposeCancelsAndReleasesModelTasks()
	{
		TabInstance tabInstance = new();
		TaskInstance task = new();
		CancellationToken token = task.CancelToken;
		tabInstance.Model.Tasks.Add(task);

		tabInstance.Dispose();

		Assert.That(token.IsCancellationRequested, Is.True, "The task should still be cancelled first");
		Assert.Throws<ObjectDisposedException>(() => _ = task.TokenSource.Token);
	}

	[Test]
	public void DisposeIsSafeToCallTwice()
	{
		TabInstance tabInstance = new();

		tabInstance.Dispose();

		Assert.DoesNotThrow(tabInstance.Dispose);
	}

	[Test, Description("An async load exception contributes to task failure state as well as the model")]
	public async Task AsyncLoadFailureIsLogged()
	{
		Call call = new();
		var tabInstance = new ThrowingAsyncTab();

		await tabInstance.LoadBackgroundAsync(call);

		Assert.That(call.Log.Level, Is.EqualTo(LogLevel.Error));
		Assert.That(call.Log.EntriesText(), Does.Contain("Async load failed"));
	}
}
