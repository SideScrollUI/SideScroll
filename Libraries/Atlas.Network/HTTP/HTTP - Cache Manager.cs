using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Network
{
    public class HttpCacheManager
    {
        private Dictionary<string, HttpCache> httpCaches = new Dictionary<string, HttpCache>();

        // should we keep the imports open all the time?
        // should we be returning disposable references?
        public HttpCache OpenCache(string path)
        {
            HttpCache httpCache = null;
            if (httpCaches.TryGetValue(path, out httpCache))
                return httpCache;
            httpCache = new HttpCache(path, true);
            httpCaches[path] = httpCache;
            return httpCache;
        }

        public void DeleteHttpCache(string path)
        {
            HttpCache httpCache = null;
            if (httpCaches.TryGetValue(path, out httpCache))
            {
                httpCache.Dispose();
                httpCaches.Remove(path);
            }

            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
    }
}
