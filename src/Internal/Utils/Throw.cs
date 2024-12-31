using System;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class Throw
    {
        internal static void ArgumentOutOfRange()
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}
