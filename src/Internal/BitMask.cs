using System;
using System.Runtime.CompilerServices;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class BitMask
    {
        private const int OFFSET = 5;
        private const int MOD_MASK = 31;
        private const int DATA_BITS = 32;
        private int[] _data;

        private int _size;

        public BitMask(int size)
        {
            _data = Array.Empty<int>();
            Resize(size);
        }

        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (_data[index >> OFFSET] & (1 << (index & MOD_MASK))) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value)
                {
                    _data[index >> OFFSET] |= (1 << (index & MOD_MASK));
                }
                else
                {
                    _data[index >> OFFSET] &= ~(1 << (index & MOD_MASK));
                }
            }
        }

        public void Resize(int newSize)
        {
            if (newSize <= _size)
            {
                return;
            }

            _size = newSize / DATA_BITS + 1;
            Array.Resize(ref _data, _size);
        }
    }
}
