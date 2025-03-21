using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [Serializable]
    [DebuggerDisplay("Count: {Count}")]
    internal struct StructList<T>
    {
        [SerializeField]
        internal T[] _items;
        [SerializeField]
        internal int _count;

        public IEnumerable<T> Enumerable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _items.Take(_count); }
        }
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _items == null; }
        }
        public bool IsNullOrEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _items == null || _items.Length <= 0; }
        }
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _count; }
        }
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _items.Length; }
            set
            {
                if (value <= _items.Length) { return; }
                value = DragonArrayUtility.NextPow2(value);
                Array.Resize(ref _items, value);
            }
        }
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if (index < 0 || index >= _count) { Throw.ArgumentOutOfRange(); }
#endif
                return _items[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG
                if (index < 0 || index >= _count) { Throw.ArgumentOutOfRange(); }
#endif
                _items[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StructList(int capacity)
        {
            _items = new T[DragonArrayUtility.NextPow2(capacity)];
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (_count >= _items.Length)
            {
                Array.Resize(ref _items, _items.Length << 1);
            }
            _items[_count++] = item;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, _count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapAt(int idnex1, int idnex2)
        {
            T tmp = _items[idnex1];
            _items[idnex1] = _items[idnex2];
            _items[idnex2] = tmp;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T FastDequeue()
        {
#if DEBUG
            if (_count <= 0) { Throw.ArgumentOutOfRange(); }
#endif
            return ref _items[--_count];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
#if DEBUG
            if (_count <= 0) { Throw.ArgumentOutOfRange(); }
#endif
            T result = _items[--_count];
            _items[_count] = default;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastRemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= _count) { Throw.ArgumentOutOfRange(); }
#endif
            _items[index] = _items[--_count];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= _count) { Throw.ArgumentOutOfRange(); }
#endif
            _items[index] = _items[--_count];
            _items[_count] = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAtWithOrder(int index)
        {
#if DEBUG
            if (index < 0 || index >= _count) { Throw.ArgumentOutOfRange(); }
#endif
            for (int i = index; i < _count;)
            {
                _items[i++] = _items[i];
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveWithOrder(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAtWithOrder(index);
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            _count = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (int i = 0; i < _count; i++)
            {
                _items[i] = default;
            }
            _count = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T>.Enumerator GetEnumerator()
        {
            return new ReadOnlySpan<T>(_items, 0, _count).GetEnumerator();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> ToReadOnlySpan()
        {
            return new ReadOnlySpan<T>(_items, 0, _count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> ToEnumerable()
        {
            return _items.Take(_count);
        }

        public T[] ToArray()
        {
            T[] result = new T[_count];
            Array.Copy(_items, result, _count);
            return _items;
        }
    }

    internal static class StructListExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add_MultiAccess<T>(this ref StructList<T> self, T item)
        {
            var index = Interlocked.Increment(ref self._count);
            index -= 1;
            if (index >= self._items.Length)
            {
                lock (self._items)
                {
                    if (index >= self._items.Length)
                    {
                        Array.Resize(ref self._items, self._items.Length << 1);
                    }
                }
            }
            self._items[index] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDequeue_MultiAccess<T>(this ref StructList<T> self, T item)
        {
            var index = Interlocked.Increment(ref self._count);

#if DEBUG
            if (_count <= 0) { Throw.ArgumentOutOfRange(); }
#endif
            T result = _items[--_count];
            _items[_count] = default;
            return result;
        }
    }
}