using NUnit.Framework;
using SideScroll.Utilities;
using System.Diagnostics;

namespace SideScroll.Tests.Utilities;

[Category("Core")]
public class ConcurrentRateLimiterTests : BaseTest
{
	[OneTimeSetUp]
	public void BaseSetup()
	{
		Initialize("ConcurrentRateLimiter");
	}

	[Test]
	public async Task RateLimiter_DoesNotExceedMaxRate()
	{
		int rps = 50;
		using var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 100, maxRequestsPerSecond: rps);

		var stopwatch = Stopwatch.StartNew();
		int completedRequests = 0;

		// Try to make 60 requests in parallel
		var tasks = new List<Task>();
		for (int i = 0; i < 60; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				using var release = await limiter.WaitAsync();
				Interlocked.Increment(ref completedRequests);
			}));
		}

		// Wait a short time: less than 1/rps (20ms).
		// Since RPS is 50, at most 50 requests should be allowed immediately (burst of max capacity),
		// and any further requests must wait.
		// Within 20ms, only the first batch of 50 should have proceeded.
		await Task.Delay(20);

		int currentCompleted = Volatile.Read(ref completedRequests);
		Assert.That(currentCompleted, Is.LessThanOrEqualTo(rps + 2), $"Should not allow more than {rps} requests immediately");

		// Wait for all to complete
		await Task.WhenAll(tasks);
		stopwatch.Stop();

		// To complete 60 requests at 50 RPS (with 50 initial tokens), it must take at least 10 tokens / 50 RPS = 0.2 seconds
		Assert.That(stopwatch.Elapsed.TotalSeconds, Is.GreaterThanOrEqualTo(0.15));
	}

	[Test]
	public async Task RateLimiter_MaintainThroughputUnderContinuousLoad()
	{
		int rps = 20; // 50ms per token
		using var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 5, maxRequestsPerSecond: rps);

		// Make 10 requests, with a small delay between them to simulate jitter/continuous load
		var stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < 10; i++)
		{
			using var release = await limiter.WaitAsync();
			await Task.Delay(10); // simulate work
		}
		stopwatch.Stop();

		// 10 requests at 20 RPS with max concurrent 5:
		// Initial burst allows 5 immediately.
		// The remaining 5 requests must wait for refills (50ms per refill).
		// Total expected wait is ~250ms.
		// If tokens were lost/discarded due to jitter, it would take much longer (e.g. > 500ms).
		Assert.That(stopwatch.Elapsed.TotalMilliseconds, Is.LessThanOrEqualTo(600));
	}

	[Test, Description("Cancelling while rate-limited releases the concurrency slot")]
	public async Task RateLimiter_CancelledRateWait_DoesNotLeakConcurrencySlot()
	{
		// 10 per second, so a refilled token arrives in about 100ms rather than the second a rate of
		// 1 made the final wait below take. The initial burst is the rate itself, so all of it is
		// drained first to reach the rate limited state this covers
		const int RequestsPerSecond = 10;
		using var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 1, maxRequestsPerSecond: RequestsPerSecond);

		// Consume the initial rate tokens, releasing only the concurrency slot each time. These
		// return immediately, the limiter isn't rate limited until they're gone
		for (int i = 0; i < RequestsPerSecond; i++)
		{
			using (await limiter.WaitAsync())
			{
			}
		}

		// Cancelled well inside the ~100ms refill, so the wait is rate limited when it's cancelled
		using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
		Assert.ThrowsAsync<OperationCanceledException>(async () =>
			await limiter.WaitAsync(cancellation.Token));

		// This needs the concurrency slot released by the cancelled wait. A rate token should
		// arrive within about 100ms; a leaked concurrency slot would block until this times out
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
		using IDisposable release = await limiter.WaitAsync(timeout.Token);
	}

	[Test, Description("Disposing a lease twice must not release two concurrency slots or throw")]
	public async Task RateLimiter_LeaseDispose_IsIdempotent()
	{
		using var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 1);
		IDisposable release = await limiter.WaitAsync();

		release.Dispose();
		Assert.DoesNotThrow(release.Dispose);

		using IDisposable nextRelease = await limiter.WaitAsync();
		using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
		Assert.ThrowsAsync<OperationCanceledException>(async () =>
			await limiter.WaitAsync(cancellation.Token));
	}

	[Test, Description("Disposing the limiter cancels pending waits and active leases remain disposable")]
	public async Task RateLimiter_Dispose_CancelsWaitersAndAllowsLeaseCleanup()
	{
		var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 1);
		IDisposable release = await limiter.WaitAsync();
		Task<IDisposable> waiting = limiter.WaitAsync();

		limiter.Dispose();

		Assert.ThrowsAsync<OperationCanceledException>(async () =>
			await waiting.WaitAsync(TimeSpan.FromSeconds(1)));
		Assert.DoesNotThrow(release.Dispose);
	}
}
