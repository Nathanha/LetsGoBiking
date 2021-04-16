using System;
using System.Runtime.Caching;

namespace WebProxyService
{
    /* Generic class to cache objects */
    class Cache<T>
    {
        private ObjectCache cache = MemoryCache.Default;
        private CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5),
        };
        double dt_seconds = 0;

        public Cache()
        {

        }

        public Cache(double dt_seconds)
        {
            this.dt_seconds = dt_seconds;
        }

        public T Get(string key)
        {
            T value = (T)cache.Get(key);
            return value;
        }

        public void Set(string key, T obj)
        {
            if (this.dt_seconds > 0) cache.Add(key, obj, DateTimeOffset.Now.AddSeconds(dt_seconds));
            else cache.Add(key, obj, cacheItemPolicy);
        }

    }
}
