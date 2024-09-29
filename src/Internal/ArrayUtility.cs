﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal interface ILinkedNext
    {
        int Next { get; }
    }
    internal readonly struct LinkedListIterator<T> : IEnumerable<T>
        where T : ILinkedNext
    {
        public readonly T[] Array;
        public readonly int Count;
        public readonly int StartIndex;
        public LinkedListIterator(T[] array, int count, int startIndex)
        {
            Array = array;
            Count = count;
            StartIndex = startIndex;
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(Array, Count, StartIndex);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _array;
            private readonly int _count;
            private int _index;
            private int _counter;
            public Enumerator(T[] array, int count, int index)
            {
                _array = array;
                _count = count;
                _index = index;
                _counter = 0;
            }
            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return ref _array[_index]; }
            }
            T IEnumerator<T>.Current { get { return _array[_index]; } }
            object IEnumerator.Current { get { return Current; } }
            public bool MoveNext()
            {
                if (++_counter > _count) { return false; }
                if (_counter > 1)
                {
                    _index = _array[_index].Next;
                }
                return true;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDisposable.Dispose() { }
            void IEnumerator.Reset() { throw new NotSupportedException(); }
        }
    }
}