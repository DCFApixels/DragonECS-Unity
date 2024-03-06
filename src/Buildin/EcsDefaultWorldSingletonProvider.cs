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
                    _instance = FindOrCreateSingleton<EcsDefaultWorldSingletonProvider>("SingletonDefaultWorld");
                }
                return _instance;
            }
        }
    }
}
