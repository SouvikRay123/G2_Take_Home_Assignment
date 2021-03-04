using System.Collections.Concurrent;

namespace Helper
{
    public static class ApplicationCache
    {
        private static ConcurrentDictionary<string, object> _Collection = new ConcurrentDictionary<string, object> { };

        public static void AddToCache<T>(string key, T value)
        {
            _Collection.TryAdd(key, value);
        }

        public static T RetrieveFromCache<T>(string key)
        {
            object value = default(T);

            _Collection.TryGetValue(key, out value);

            return (T)value;
        }
    }
}
