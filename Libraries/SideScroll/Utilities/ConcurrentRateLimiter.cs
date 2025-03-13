using System.Collections.Concurrent;
using System.Diagnostics;

namespace SideScroll.Utilities;

public class ConcurrentRateLimiter : IDisposable
{
	public static int DefaultMaxConcurrentRequests { get; set; } = 10;

	public int MaxConcurrentRequests { get; }
	public int? MaxRequestsPerSecond { get; }

	private readonly SemaphoreSlim _concurrencySemaphore;
	private readonly SemaphoreSlim? _rateSemaphore;
	private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
	private readonly CancellationTokenSource _cts = new();
	private readonly Task? _tokenRefillTask;

	public ConcurrentRateLimiter(int? maxConcurrentRequests = null, int? maxRequestsPerSecond = null)
	{
		MaxConcurrentRequests = maxConcurrentRequests ?? DefaultMaxConcurrentRequests;
		MaxRequestsPerSecond = maxRequestsPerSecond;

		_concurrencySemaphore = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);

		if (MaxRequestsPerSecond is int rps && rps > 0)
		{
			_rateSemaphore = new SemaphoreSlim(rps, rps);
			_tokenRefillTask = Task.Run(() => RefillTokensAsync(_cts.Token));
		}
	}

	public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
	{
		if (_rateSemaphore != null)
		{
			await _rateSemaphore.WaitAsync(cancellationToken);
			_requestTimestamps.Enqueue(DateTime.UtcNow);
		}

		await _concurrencySemaphore.WaitAsync(cancellationToken);

		return new ConcurrencyRelease(_concurrencySemaphore);
	}
	private async Task RefillTokensAsync(CancellationToken cancellationToken)
	{
		if (MaxRequestsPerSecond is not int rps || rps <= 0)
			return;

		var stopwatch = Stopwatch.StartNew();

		while (!cancellationToken.IsCancellationRequested)
		{
			// Dynamically adjust the delay: 
			// - High RPS = shorter delay
			// - Low RPS = longer delay (up to 100ms)
			int delayMs = Math.Max(1, 1000 / rps); // Minimum 1ms delay for high RPS
			delayMs = Math.Min(delayMs, 100); // Cap at 100ms for low RPS to avoid sluggishness

			await Task.Delay(delayMs, cancellationToken);

			double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
			int tokensToRelease = (int)(elapsedSeconds * rps); // Calculate tokens based on elapsed time

			if (tokensToRelease > 0)
			{
				stopwatch.Restart(); // Reset the stopwatch after releasing tokens

				for (int i = 0; i < tokensToRelease; i++)
				{
					if (!_requestTimestamps.TryDequeue(out _))
						break;

					_rateSemaphore?.Release();
				}
			}
		}
	}

	public void Dispose()
	{
		_cts.Cancel();
		try
		{
			_tokenRefillTask?.Wait(); // Ensure cleanup of background task
		}
		catch (AggregateException)
		{
		}
		_cts.Dispose();
		_rateSemaphore?.Dispose();
		_concurrencySemaphore.Dispose();
	}

	private class ConcurrencyRelease(SemaphoreSlim semaphore) : IDisposable
	{
		public void Dispose() => semaphore.Release();
	}
}
