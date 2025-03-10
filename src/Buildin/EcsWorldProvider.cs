using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/Providers/" + nameof(EcsWorldProvider), order = 1)]
    public class EcsWorldProvider : EcsWorldProvider<EcsWorld>
    {
        protected override EcsWorld BuildWorld(ConfigContainer configs) { return new EcsWorld(configs, WorldName, WorldID); }
    }
}
