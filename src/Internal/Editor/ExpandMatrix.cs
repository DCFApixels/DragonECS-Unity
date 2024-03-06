#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class ExpandMatrix
    {
        private const bool TOP_DEFAULT = true;
        private const bool DEFAULT = false;
        private static Dictionary<Type, ExpandMatrix> _instances = new Dictionary<Type, ExpandMatrix>();
        public static ExpandMatrix Take(Type type)
        {
            if (_instances.TryGetValue(type, out ExpandMatrix result) == false)
            {
                result = new ExpandMatrix();
                _instances.Add(type, result);
            }
            return result;
        }
        private bool[] _flags = new bool[8];
        private int _count = 0;
        private int _ptr = 0;

        public int Count
        {
            get { return _count; }
        }
        public ref bool CurrentIsExpanded
        {
            get { return ref _flags[_ptr]; }
        }
        public void Up()
        {
            if (_ptr < 0)
            {
                throw new Exception("нарушение баланса инкремент/декремент");
            }
            _ptr--;
        }

        public ref bool Down()
        {
            _ptr++;
            if (_ptr >= _count)
            {
                if (_count >= _flags.Length)
                {
                    Array.Resize(ref _flags, _flags.Length << 1);
                }
                _flags[_count++] = _ptr <= 1 ? TOP_DEFAULT : DEFAULT;
            }
            return ref _flags[_ptr];
        }
    }
}
#endif