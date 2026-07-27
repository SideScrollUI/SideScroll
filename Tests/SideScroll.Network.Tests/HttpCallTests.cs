using NUnit.Framework;
using SideScroll.Network.Http;
using SideScroll.Tasks;
using System.Net;

namespace SideScroll.Network.Tests;

[Category("HTTP")]
public class HttpCallTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("HTTP");
	}

	[Test, Description("Cancelling the Call aborts an in-flight HttpCall request")]
	public void GetBytesAsyncObservesCallCancellation()
	{
		var taskInstance = new TaskInstance();
		Call call = new() { TaskInstance = taskInstance };
		using var client = new HttpClient(new StubHandler(async (_, cancelToken) =>
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancelToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}));
		var httpCall = new TestHttpCall(call, client);

		Task<byte[]> request = httpCall.GetBytesAsync("http://example.com/value");
		taskInstance.Cancel();

		Assert.ThrowsAsync<TaskCanceledException>(async () =>
			await request.WaitAsync(TimeSpan.FromSeconds(1)));
	}

	[Test, Description("Cancelling the Call aborts the wait before an HttpCall retry")]
	public void RetryDelayObservesCallCancellation()
	{
		var taskInstance = new TaskInstance();
		Call call = new() { TaskInstance = taskInstance };
		using var client = new HttpClient(new StubHandler((request, _) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
			{
				RequestMessage = request,
			})));
		var httpCall = new TestHttpCall(call, client);

		Task<byte[]> request = httpCall.GetBytesAsync("http://example.com/value");
		taskInstance.Cancel();

		Assert.ThrowsAsync<TaskCanceledException>(async () =>
			await request.WaitAsync(TimeSpan.FromSeconds(1)));
	}

	private sealed class TestHttpCall(Call call, HttpClient client) : HttpCall(call)
	{
		protected override HttpClient GetClient(HttpClientConfig clientConfig) => client;
	}

	private sealed class StubHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken) =>
			sendAsync(request, cancellationToken);
	}
}
