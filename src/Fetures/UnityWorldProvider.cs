using System;

namespace DCFApixels.DragonECS
{
    public static class UnityWorldProvider<TWorld>
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

        public static TWorld Get()
        {
            if (_world == null)
                _world = (TWorld)Activator.CreateInstance(typeof(TWorld));
            return _world;
        }
    }

}
