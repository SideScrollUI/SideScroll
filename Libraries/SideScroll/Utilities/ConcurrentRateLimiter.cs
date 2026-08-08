using System.Collections.Concurrent;
using System.Diagnostics;

namespace SideScroll.Utilities;

/// <summary>
/// Provides concurrent rate limiting functionality with configurable concurrency and request rate limits
/// </summary>
public class ConcurrentRateLimiter : IDisposable
{
	/// <summary>
	/// Gets or sets the default maximum number of concurrent requests
	/// </summary>
	public static int DefaultMaxConcurrentRequests { get; set; } = 10;

	/// <summary>
	/// Gets the maximum number of concurrent requests allowed
	/// </summary>
	public int MaxConcurrentRequests { get; }

	/// <summary>
	/// Gets the maximum number of requests per second, or null if no rate limit is set
	/// </summary>
	public int? MaxRequestsPerSecond { get; }

	private readonly SemaphoreSlim _concurrencySemaphore;
	private readonly SemaphoreSlim? _rateSemaphore;
	private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
	private readonly CancellationTokenSource _cts = new();
	private readonly Task? _tokenRefillTask;
	private int _disposeRequested;
	private int _activeUsers;
	private int _refillStopped;
	private int _resourcesDisposed;

	/// <summary>
	/// Initializes a new instance of the ConcurrentRateLimiter class
	/// </summary>
	public ConcurrentRateLimiter(int? maxConcurrentRequests = null, int? maxRequestsPerSecond = null)
	{
		MaxConcurrentRequests = maxConcurrentRequests ?? DefaultMaxConcurrentRequests;
		MaxRequestsPerSecond = maxRequestsPerSecond;

		_concurrencySemaphore = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);

		if (MaxRequestsPerSecond is { } rps and > 0)
		{
			_rateSemaphore = new SemaphoreSlim(rps, rps);
			_tokenRefillTask = Task.Run(() => RefillTokensAsync(_cts.Token));
		}
	}

	/// <summary>
	/// Waits asynchronously for permission to proceed based on concurrency and rate limits
	/// </summary>
	public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeRequested) != 0, this);
		Interlocked.Increment(ref _activeUsers);
		if (Volatile.Read(ref _disposeRequested) != 0)
		{
			ReleaseUser();
			throw new ObjectDisposedException(GetType().FullName);
		}

		using CancellationTokenSource linkedCancellation =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
		CancellationToken waitToken = linkedCancellation.Token;

		bool concurrencyAcquired = false;
		try
		{
			await _concurrencySemaphore.WaitAsync(waitToken);
			concurrencyAcquired = true;

			if (_rateSemaphore != null)
			{
				await _rateSemaphore.WaitAsync(waitToken);
				_requestTimestamps.Enqueue(DateTime.UtcNow);
			}

			return new ConcurrencyRelease(this);
		}
		catch
		{
			// Cancellation or disposal while waiting for a rate token must not strand
			// the concurrency slot acquired above
			if (concurrencyAcquired)
				_concurrencySemaphore.Release();
			ReleaseUser();
			throw;
		}
	}

	private async Task RefillTokensAsync(CancellationToken cancellationToken)
	{
		if (MaxRequestsPerSecond is not ({ } rps and > 0))
			return;

		var stopwatch = Stopwatch.StartNew();
		double earnedTokens = 0; // Carries the fractional remainder between cycles so the rate doesn't drift low

		while (!cancellationToken.IsCancellationRequested)
		{
			// Dynamically adjust the delay:
			// - High RPS = shorter delay
			// - Low RPS = longer delay (up to 100ms)
			int delayMs = Math.Max(1, 1000 / rps); // Minimum 1ms delay for high RPS
			delayMs = Math.Min(delayMs, 100); // Cap at 100ms for low RPS to avoid sluggishness

			await Task.Delay(delayMs, cancellationToken);

			earnedTokens += stopwatch.Elapsed.TotalSeconds * rps; // Calculate tokens based on elapsed time
			stopwatch.Restart();

			// Cap earnedTokens at the number of outstanding requests to prevent accumulating excess tokens when idle
			int queueCount = _requestTimestamps.Count;
			if (earnedTokens > queueCount)
			{
				earnedTokens = queueCount;
			}

			int tokensToRelease = (int)earnedTokens;
			if (tokensToRelease <= 0) continue;

			int releasedCount = 0;
			for (int i = 0; i < tokensToRelease; i++)
			{
				if (!_requestTimestamps.TryDequeue(out _))
					break;

				_rateSemaphore?.Release();
				releasedCount++;
			}

			earnedTokens -= releasedCount;
		}
	}

	/// <summary>
	/// Disposes the rate limiter and releases all resources
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref _disposeRequested, 1) != 0)
			return;

		if (disposing)
		{
			// Dispose managed resources
			_cts.Cancel();
			try
			{
				_tokenRefillTask?.Wait(); // Ensure cleanup of background task
			}
			catch (AggregateException)
			{
				// Expected when cancellation occurs
			}
			// Clear queue
			_requestTimestamps.Clear();
			Volatile.Write(ref _refillStopped, 1);
			TryDisposeResources();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void ReleaseLease()
	{
		_concurrencySemaphore.Release();
		ReleaseUser();
	}

	private void ReleaseUser()
	{
		if (Interlocked.Decrement(ref _activeUsers) == 0)
			TryDisposeResources();
	}

	private void TryDisposeResources()
	{
		if (Volatile.Read(ref _disposeRequested) == 0 ||
			Volatile.Read(ref _refillStopped) == 0 ||
			Volatile.Read(ref _activeUsers) != 0 ||
			Interlocked.Exchange(ref _resourcesDisposed, 1) != 0)
			return;

		_rateSemaphore?.Dispose();
		_concurrencySemaphore.Dispose();
		_cts.Dispose();
	}

	private sealed class ConcurrencyRelease(ConcurrentRateLimiter limiter) : IDisposable
	{
		private ConcurrentRateLimiter? _limiter = limiter;

		public void Dispose()
		{
			Interlocked.Exchange(ref _limiter, null)?.ReleaseLease();
		}
	}
}
