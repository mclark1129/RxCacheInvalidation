using System;
using System.Reactive;

namespace RxCacheInvalidation
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cache = new Cache())
            {
                cache.InvalidateKey("test", "123");
                cache.InvalidateKey("test", "123");
                cache.InvalidateKey("test2", "123");
                cache.InvalidateKeys("test", new[] { "4567", "789" });
                cache.InvalidateKeys("test", new[] { "456", "789" });

                Console.ReadLine();
            }
        }
    }
}
