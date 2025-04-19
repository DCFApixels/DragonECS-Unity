using System.Collections;
using System.Collections.Generic;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal readonly struct ReadOnlyList<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        private readonly List<T> _list;
        public ReadOnlyList(List<T> list)
        {
            _list = list;
        }
        public int Count
        {
            get { return _list.Count; }
        }
        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public List<T>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
        public static implicit operator ReadOnlyList<T>(List<T> a) { return new ReadOnlyList<T>(a); }
    }
}
