using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = NAME, menuName = EcsConsts.FRAMEWORK_NAME + "/Providers/" + NAME, order = 1)]
    public class EcsDefaultWorldSingletonProvider : EcsWorldProvider<EcsDefaultWorld>
    {
        private const string NAME = "SingletonDefaultWorld";
        private static EcsDefaultWorldSingletonProvider _instance;
        public static EcsDefaultWorldSingletonProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindOrCreateSingleton<EcsDefaultWorldSingletonProvider>(NAME);
                }
                return _instance;
            }
        }
        protected override EcsDefaultWorld BuildWorld(ConfigContainer configs) { return new EcsDefaultWorld(configs, WorldName, WorldID); }
    }
}
