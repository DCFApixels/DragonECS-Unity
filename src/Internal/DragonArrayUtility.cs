using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DCFApixels.DragonECS.Unity.Internal
{
    public readonly struct ArrayBuffer<T>
    {
        public readonly T[] Array;
        public readonly int Length;
        public ArrayBuffer(T[] array, int length)
        {
            Array = array;
            Length = length;
        }
    }
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

    internal static class DragonArrayUtility
    {
        public static int NextPow2(int v)
        {
            unchecked
            {
                v--;
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                return ++v;
            }
        }
        public static int NextPow2_ClampOverflow(int v)
        {
            unchecked
            {
                const int NO_SIGN_HIBIT = 0x40000000;
                if ((v & NO_SIGN_HIBIT) != 0)
                {
                    return int.MaxValue;
                }
                return NextPow2(v);
            }
        }
        public static void Fill<T>(T[] array, T value, int startIndex = 0, int length = -1)
        {
            if (length < 0)
            {
                length = array.Length;
            }
            else
            {
                length = startIndex + length;
            }
            for (int i = startIndex; i < length; i++)
            {
                array[i] = value;
            }
        }
    }
}