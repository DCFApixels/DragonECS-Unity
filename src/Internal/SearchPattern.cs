#if UNITY_EDITOR
using System;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal readonly struct SearchPattern
    {
        public const char DefaultSeparator = '/';
		private readonly string _pattern;
        private readonly char _separator;
        public SearchPattern(string pattern, char separator)
        {
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            _separator = separator;
        }
        public Enumerator GetEnumerator() { return new Enumerator(_pattern, _separator); }
        public ref struct Enumerator
        {
            private readonly string _pattern;
            private readonly char _separator;
            private int _start;
            private int _currentStart;
            private int _currentLength;

            public Enumerator(string pattern, char separator)
            {
                _pattern = pattern;
                _separator = separator;
                _start = 0;
                _currentStart = -1;
                _currentLength = 0;
            }

            public ReadOnlySpan<char> Current
            {
                get { return _pattern.AsSpan(_currentStart, _currentLength); }
            }

            public bool MoveNext()
            {
                if (_pattern == null || _start > _pattern.Length)
                {
                    return false;
                }

                int len = _pattern.Length;
                while (_start <= len)
                {
                    int i = _start;
                    while (i < len && _pattern[i] != _separator)
                    {
                        i++;
                    }

                    int subLen = i - _start;
                    if (subLen > 0) // возвращаем только непустые подстроки
                    {
                        _currentStart = _start;
                        _currentLength = subLen;
                        _start = i + 1;
                        return true;
                    }

                    // пустая подстрока — пропускаем разделитель и продолжаем
                    _start = i + 1;
                }

                return false;
            }
        }
    }
}
#endif