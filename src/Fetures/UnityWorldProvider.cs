using DCFApixels.DragonECS;
using System;

namespace DCFApixels.Assets.DragonECS_Unity.src.Fetures
{
    public static class UnityWorldProvider<TWorld>
        where TWorld : EcsWorld<TWorld>
    {
        private static TWorld _world;

        public static TWorld Get(Func<TWorld> builder)
        {
            if (builder == null)
                throw new ArgumentNullException();

            if (_world == null)
                _world = builder();

            return _world;
        }
    }
}
