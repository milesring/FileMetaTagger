using System.Collections.Concurrent;

namespace FileMetaTagger
{
    public class ConcurrentHashSet<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary;
        public ConcurrentHashSet()
        {
            _dictionary = new ConcurrentDictionary<T, byte>();
        }

        public ConcurrentHashSet(IEnumerable<T> values) : this()
        {
            foreach (var item in values)
            {
                _dictionary.TryAdd(item, 0);
            }
        }

        public bool Add(T item)
        {
            return _dictionary.TryAdd(item, 0);
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public bool Remove(T item)
        {
            return _dictionary.TryRemove(item, out _);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public int Count => _dictionary.Count;
    }
}
