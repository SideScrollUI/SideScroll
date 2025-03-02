using System.Collections.Concurrent;
using System.Diagnostics;

namespace SideScroll.Utilities;

public class RateLimiter1 : IDisposable
{
	public static int DefaultMaxConcurrentRequests { get; set; } = 10;

	public int MaxConcurrentRequests { get; init; }
	public int? MaxRequestsPerSecond { get; init; }

	private readonly SemaphoreSlim _concurrencySemaphore;
	private readonly SemaphoreSlim? _rateSemaphore;

	private readonly ConcurrentQueue<Stopwatch> _requestTimestamps = new();
	private readonly Timer? _timer;

	public RateLimiter1(int? maxConcurrentRequests = null, int? maxRequestsPerSecond = null)
	{
		MaxConcurrentRequests = maxConcurrentRequests ?? DefaultMaxConcurrentRequests;
		MaxRequestsPerSecond = maxRequestsPerSecond;

		_concurrencySemaphore = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);
		if (MaxRequestsPerSecond is int rps && rps > 0)
		{
			_rateSemaphore = new SemaphoreSlim(rps, rps);

			// Timer to release tokens periodically
			_timer = new Timer(ReleaseRateTokens, null, 0, Math.Max(10, 1000 / rps));
		}
	}

	public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
	{
		if (_rateSemaphore != null)
		{
			await _rateSemaphore.WaitAsync(cancellationToken);
			await _concurrencySemaphore.WaitAsync(cancellationToken);

			lock (_requestTimestamps)
			{
				var timestamp = Stopwatch.StartNew();
				_requestTimestamps.Enqueue(timestamp);
			}
		}
		else
		{

			await _concurrencySemaphore.WaitAsync(cancellationToken);
		}

		return new ConcurrencyRelease(_concurrencySemaphore);
	}

	private void ReleaseRateTokens(object? state)
	{
		lock (_requestTimestamps)
		{
			while (_requestTimestamps.TryPeek(out var timestamp))
			{
				if (timestamp.ElapsedMilliseconds < 1000) // Older than 1 second
				{
					break;
				}
				
				_requestTimestamps.TryDequeue(out _);
				_rateSemaphore?.Release();
			}
		}
	}

	public void Dispose()
	{
		_timer?.Dispose();
		_rateSemaphore?.Dispose();
		_concurrencySemaphore.Dispose();
	}

	private class ConcurrencyRelease(SemaphoreSlim semaphore) : IDisposable
	{
		private readonly SemaphoreSlim _semaphore = semaphore;

		public void Dispose() => _semaphore.Release();
	}
}
