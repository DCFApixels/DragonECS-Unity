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

    internal static class ArrayUtility
    {
        private static int GetHighBitNumber(uint bits)
        {
            if (bits == 0)
            {
                return -1;
            }
            int bit = 0;
            if ((bits & 0xFFFF0000) != 0)
            {
                bits >>= 16;
                bit |= 16;
            }
            if ((bits & 0xFF00) != 0)
            {
                bits >>= 8;
                bit |= 8;
            }
            if ((bits & 0xF0) != 0)
            {
                bits >>= 4;
                bit |= 4;
            }
            if ((bits & 0xC) != 0)
            {
                bits >>= 2;
                bit |= 2;
            }
            if ((bits & 0x2) != 0)
            {
                bit |= 1;
            }
            return bit;
        }
        public static int NormalizeSizeToPowerOfTwo(int minSize)
        {
            unchecked
            {
                return 1 << (GetHighBitNumber((uint)minSize - 1u) + 1);
            }
        }
        public static int NormalizeSizeToPowerOfTwo_ClampOverflow(int minSize)
        {
            unchecked
            {
                int hibit = (GetHighBitNumber((uint)minSize - 1u) + 1);
                if (hibit >= 32)
                {
                    return int.MaxValue;
                }
                return 1 << hibit;
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