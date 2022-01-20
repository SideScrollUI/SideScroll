using Atlas.Core;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Atlas.Network;

[Unserialized]
public class MemoryTypeCache<T1>
{
	public TimeSpan CacheDuration;

	public MemoryCache MemoryCache;

	public int ExpirationSeconds { get; set; } = 60;
	public int MaxItems { get; set; } = 100;

	public MemoryTypeCache(TimeSpan cacheDuration)
	{
		CacheDuration = cacheDuration;

		var options = new MemoryCacheOptions()
		{
			SizeLimit = MaxItems,
			ExpirationScanFrequency = TimeSpan.FromSeconds(60),
		};
		MemoryCache = new MemoryCache(options);
	}

	public void Add(Call call, string key, T1 response)
	{
		if (response == null)
			return;

		var options = new MemoryCacheEntryOptions()
		{
			Size = 1, // Assume all items are the same size for now
		};

		MemoryCache.Set(key, response, options);
	}

	public T1 Get(Call call, string key)
	{
		if (MemoryCache.TryGetValue(key, out object result))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			return (T1)result;
		}

		return default;
	}

	public bool TryGetValue(Call call, string key, out T1 t1)
	{
		if (MemoryCache.TryGetValue(key, out object result))
		{
			call.Log.Add("Found cached copy", new Tag("Uri", key));
			t1 = (T1)result;
			return true;
		}
		t1 = default;
		return false;
	}
}
