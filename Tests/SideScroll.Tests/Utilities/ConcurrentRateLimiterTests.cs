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
		int rps = 5;
		using var limiter = new ConcurrentRateLimiter(maxConcurrentRequests: 10, maxRequestsPerSecond: rps);

		// Wait to allow any potential initial/idle tokens to accumulate
		await Task.Delay(1000);

		var stopwatch = Stopwatch.StartNew();
		int completedRequests = 0;

		// Try to make 10 requests in parallel
		var tasks = new List<Task>();
		for (int i = 0; i < 10; i++)
		{
			tasks.Add(Task.Run(async () =>
			{
				using var release = await limiter.WaitAsync();
				Interlocked.Increment(ref completedRequests);
			}));
		}

		// Wait a short time: less than 1/rps (200ms).
		// Since RPS is 5, at most 5 requests should be allowed immediately (burst of max capacity),
		// and any further requests must wait.
		// Within 100ms, only the first batch of 5 should have proceeded.
		await Task.Delay(100);

		int currentCompleted = Volatile.Read(ref completedRequests);
		Assert.That(currentCompleted, Is.LessThanOrEqualTo(rps), $"Should not allow more than {rps} requests immediately");

		// Wait for all to complete
		await Task.WhenAll(tasks);
		stopwatch.Stop();

		// To complete 10 requests at 5 RPS, it must take at least ~1 second
		Assert.That(stopwatch.Elapsed.TotalSeconds, Is.GreaterThanOrEqualTo(0.8));
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
}
