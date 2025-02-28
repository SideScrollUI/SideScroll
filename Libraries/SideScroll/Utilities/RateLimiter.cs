using System.Collections.Concurrent;
using System.Diagnostics;

namespace SideScroll.Utilities;

public class RateLimiter : IDisposable
{
	public static int MaxRequestsPerSecond { get; set; } = 10;
	public static int MaxConcurrentRequests { get; set; } = 10;

	private readonly int _maxRequestsPerSecond;
	private readonly int _maxConcurrentRequests;

	private readonly SemaphoreSlim _rateSemaphore;
	private readonly SemaphoreSlim _concurrencySemaphore;

	private readonly ConcurrentQueue<Stopwatch> _requestTimestamps = new();
	private readonly Timer _timer;

	public RateLimiter(int? maxRequestsPerSecond = null, int? maxConcurrentRequests = null)
	{
		_maxRequestsPerSecond = maxRequestsPerSecond ?? MaxRequestsPerSecond;
		_maxConcurrentRequests = maxConcurrentRequests ?? maxRequestsPerSecond ?? MaxConcurrentRequests;

		_rateSemaphore = new SemaphoreSlim(_maxRequestsPerSecond, _maxConcurrentRequests);
		_concurrencySemaphore = new SemaphoreSlim(_maxConcurrentRequests, _maxConcurrentRequests);

		// Timer to release tokens periodically
		_timer = new Timer(ReleaseRateTokens, null, 0, 1000 / _maxRequestsPerSecond);
	}

	public async Task<IDisposable> WaitAsync(CancellationToken cancellationToken = default)
	{
		await _rateSemaphore.WaitAsync(cancellationToken);
		await _concurrencySemaphore.WaitAsync(cancellationToken);

		lock (_requestTimestamps)
		{
			var timestamp = Stopwatch.StartNew();
			_requestTimestamps.Enqueue(timestamp);
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
				_rateSemaphore.Release();
			}
		}
	}

	public void Dispose()
	{
		_timer.Dispose();
		_rateSemaphore.Dispose();
		_concurrencySemaphore.Dispose();
	}

	private class ConcurrencyRelease : IDisposable
	{
		private readonly SemaphoreSlim _semaphore;

		public ConcurrencyRelease(SemaphoreSlim semaphore) => _semaphore = semaphore;

		public void Dispose() => _semaphore.Release();
	}
}
