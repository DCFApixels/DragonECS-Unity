namespace DCFApixels.DragonECS
{
    public class EcsDefaultWorldSingletonProvider : EcsWorldProvider<EcsDefaultWorld>
    {
        private static EcsDefaultWorldSingletonProvider _instance;
        public static EcsDefaultWorldSingletonProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindOrCreateSingleton<EcsDefaultWorldSingletonProvider>("DefaultSingletonProvider");
                }
                return _instance;
            }
        }

        protected override EcsDefaultWorld BuildWorld()
        {
            return new EcsDefaultWorld();
        }
    }
}
