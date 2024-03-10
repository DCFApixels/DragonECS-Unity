using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsDefaultWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/WorldProviders/" + nameof(EcsDefaultWorldProvider), order = 1)]
    public class EcsDefaultWorldProvider : EcsWorldProvider<EcsDefaultWorld> { }
}
