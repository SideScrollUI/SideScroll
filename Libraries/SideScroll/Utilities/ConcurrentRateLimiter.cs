using System.Threading.RateLimiting;

namespace SideScroll.Utilities;

public class ConcurrentRateLimiter : IDisposable
{
	public static int DefaultMaxConcurrentRequests { get; set; } = 10;

	public int MaxConcurrentRequests { get; init; }
	public int? MaxRequestsPerSecond { get; init; }

	private readonly SemaphoreSlim _concurrencyLimiter;
	private readonly RateLimiter? _rateLimiter;

	public ConcurrentRateLimiter(int? maxConcurrentRequests = null, int? maxRequestsPerSecond = null)
	{
		MaxConcurrentRequests = maxConcurrentRequests ?? DefaultMaxConcurrentRequests;
		MaxRequestsPerSecond = maxRequestsPerSecond;

		_concurrencyLimiter = new SemaphoreSlim(MaxConcurrentRequests);

		if (maxRequestsPerSecond is int rps && rps > 0)
		{
			_rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
			{
				TokenLimit = rps,
				TokensPerPeriod = rps,
				ReplenishmentPeriod = TimeSpan.FromMilliseconds(10),
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
				QueueLimit = int.MaxValue
			});
		}
	}

	public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
	{
		RateLimitLease? lease = null;
		if (_rateLimiter != null)
		{
			lease = await _rateLimiter.AcquireAsync(1, cancellationToken);
			if (!lease.IsAcquired) throw new Exception("Rate limit exceeded");
		}

		await _concurrencyLimiter.WaitAsync(cancellationToken);

		return new ConcurrencyRelease(_concurrencyLimiter, lease);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_rateLimiter?.Dispose();
			_concurrencyLimiter.Dispose();
		}
	}

	private class ConcurrencyRelease(SemaphoreSlim semaphore, RateLimitLease? lease) : IDisposable
	{
		public void Dispose()
		{
			semaphore.Release();
			lease?.Dispose();
		}
	}
}
