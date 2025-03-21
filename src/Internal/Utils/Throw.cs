using System;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class Throw
    {
        internal static void ArgumentOutOfRange()
        {
            throw new ArgumentOutOfRangeException();
        }
        internal static void Argument(string message)
        {
            throw new ArgumentException(message);
        }
        internal static void Exception()
        {
            throw new Exception();
        }
    }
}
