using Atlas.Core;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;

namespace Atlas.Network
{
	public class HttpMemoryCache
	{
		public static int MaxAttempts { get; set; } = 5;
		public int MaxItems { get; set; } = 100;
		public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);

		public MemoryCache MemoryCache;

		public HttpMemoryCache()
		{
			var options = new MemoryCacheOptions()
			{
				SizeLimit = MaxItems,
				ExpirationScanFrequency = TimeSpan.FromSeconds(60),
			};
			MemoryCache = new MemoryCache(options);
		}

		public void Add(string key, object obj)
		{
			if (obj == null)
				return;

			var options = new MemoryCacheEntryOptions()
			{
				Size = 1, // Assume all items are the same size for now
			};

			MemoryCache.Set(key, obj, options);
		}

		public T Get<T>(Call call, string uri)
		{
			if (TryGetValue<T>(call, uri, out T t))
				return t;

			return default;
		}

		public bool TryGetValue<T>(Call call, string uri, out T t)
		{
			if (uri == null)
			{
				t = default;
				return false;
			}

			if (MemoryCache.TryGetValue(uri, out object result))
			{
				call.Log.Add("Found cached copy", new Tag("Uri", uri));
				t = (T)result;
				return true;
			}

			string text = HttpClient.GetString(call, uri);
			if (text != null)
			{
				try
				{
					t = JsonConvert.DeserializeObject<T>(text);
					Add(uri, t);
					return true;
				}
				catch (Exception e)
				{
					call.Log.Add(e);
				}
			}
			t = default;
			return false;
		}
	}
}
