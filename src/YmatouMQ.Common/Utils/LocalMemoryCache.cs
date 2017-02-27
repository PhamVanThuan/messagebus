using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQNet4.Utils;
using YmatouMQ.Common.Extensions;

namespace YmatouMQ.Common.Utils
{
    public class LocalMemoryCache<K, V>
    {
        private readonly ConcurrentDictionary<K, CacheItem> cache = new ConcurrentDictionary<K, CacheItem>();
        private readonly int maxItem;
        public LocalMemoryCache(int max=1000)
        {
            this.maxItem = max;
        }

        public LocalMemoryCache<K, V> AddItem(K k, V v, TimeSpan timeOut)
        {
            YmtSystemAssert.AssertArgumentNotNull(k, "cache key can't null");
            if (cache.ContainsKey(k)) return this;
            cache.TryAdd(k, new CacheItem { timeOut = DateTime.Now.Add(timeOut), v = v });
            LazyRemoveExpiredItem();
            return this;
        }
        public V GetCacheItem(K k, V defVal = default(V))
        {
            YmtSystemAssert.AssertArgumentNotNull(k, "cache key can't null");
            if (!cache.ContainsKey(k))
            {
                return defVal;
            }
            CacheItem item = null;
            if (!cache.TryGetValue(k, out item))
            {
                return defVal;
            }
            if (item == null) return defVal;
            if (item.timeOut <= DateTime.Now)
            {
                cache.TryRemove(k, out item);
                return defVal;
            }
            return item.v;
        }
        private void LazyRemoveExpiredItem()
        {
            if (cache.Count >= maxItem)
            {
                cache.Where(_v => _v.Value.timeOut <= DateTime.Now)
                    .Select(_v => _v.Key)
                    .EachAction(_k =>
                    {
                        CacheItem cacheItem;
                        cache.TryRemove(_k, out cacheItem);
                    });
            }
        }
        private class CacheItem
        {
            public V v { get; set; }
            public DateTime timeOut { get; set; }
        }
    }
}
