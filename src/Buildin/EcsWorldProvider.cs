using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/WorldProviders/" + nameof(EcsWorldProvider), order = 1)]
    public class EcsWorldProvider : EcsWorldProvider<EcsWorld>
    {
        protected override EcsWorld BuildWorld(ConfigContainer configs) { return new EcsWorld(configs, null, WorldID); }
    }
}
