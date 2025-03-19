#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static EcsConsts;

    [MetaName("ScriptableObjectSystem")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaDescription(AUTHOR, "Wrapper for ScriptableObject systems")]
    [MetaID("DragonECS_F5A94C5F92015B4D9286E76809311AF4")]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, OTHER_GROUP)]
    public class ScriptableObjectSystemWrapper : IEcsModule
    {
        public ScriptableObject system;
        public void Import(EcsPipeline.Builder b)
        {
            b.Add(system);
        }
    }
}
