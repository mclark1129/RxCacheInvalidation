using System;
using System.Collections.Generic;
using System.Linq;

using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace RxCacheInvalidation
{
    public interface IActiveCacheBus
    {
        void InvalidateKey(string entityName, string key);
        void InvalidateKeys(string entityName, IEnumerable<string> keys);
    }

    class CacheInvalidation
    {
        public CacheInvalidation(string entityName, IEnumerable<string> keys)
        {
            EntityName = entityName;
            Keys = keys;
        }

        public string EntityName { get; }
        public IEnumerable<string> Keys { get; }

        public override string ToString()
        {
            if ((EntityName == null) || (Keys == null))
                return string.Empty;

            return $"Cache Invalidation: Entity = {EntityName}, Keys = {string.Join(',', Keys)}";
        }
    }

    public class Cache : IActiveCacheBus, IDisposable
    {
        private Subject<CacheInvalidation> _cacheInvalidations = new Subject<CacheInvalidation>();

        public Cache()
        {
            // Subscribe to a stream of all invalidation requests to this 
            _cacheInvalidations
                // Split the invalidations into multiple streams
                // based on entity-type
                .GroupBy(x => x.EntityName)
                .Subscribe(grp =>
                {
                    // Batch all invalidations for 10 seconds, up to 100 invalidations at a time
                    grp.Buffer(TimeSpan.FromSeconds(10), 100)
                        .Where(x => x.Any()) // Ignore empty batches
                        .Select(BatchInvalidations) // Consolidate batches into a single invalidation
                        .Subscribe(Console.WriteLine);
                });
        }

        public void Dispose()
        {
            _cacheInvalidations.Dispose();
        }

        public void InvalidateKey(string entityName, string key)
        {
            _cacheInvalidations.OnNext(new CacheInvalidation(entityName, new[] { key }));
        }

        public void InvalidateKeys(string entityName, IEnumerable<string> keys)
        {
            _cacheInvalidations.OnNext(new CacheInvalidation(entityName, keys));
        }

        private CacheInvalidation BatchInvalidations(IEnumerable<CacheInvalidation> cacheInvalidations)
        {
            // Combine all the keys in a single batch for
            var allKeys = cacheInvalidations.SelectMany(x => x.Keys).Distinct();
            return new CacheInvalidation(cacheInvalidations.First().EntityName, allKeys);
        }
    }
}
