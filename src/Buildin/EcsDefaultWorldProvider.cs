using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsDefaultWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/WorldProviders/" + nameof(EcsDefaultWorldProvider), order = 1)]
    public class EcsDefaultWorldProvider : EcsWorldProvider<EcsDefaultWorld>
    {
        //private static EcsDefaultWorldProvider _singleInstance;
        //public static EcsDefaultWorldProvider SingletonInstance
        //{
        //    get
        //    {
        //        if (_singleInstance == null)
        //        {
        //            _singleInstance = FindOrCreateSingleton<EcsDefaultWorldProvider>();
        //        }
        //        return _singleInstance;
        //    }
        //}
    }
}
