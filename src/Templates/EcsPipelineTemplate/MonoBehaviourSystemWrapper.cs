using DCFApixels.DragonECS.Unity;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static EcsConsts;

    [MetaName("MonoBehaviourSystem")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaDescription(AUTHOR, "Wrapper for MonoBehaviour systems")]
    [MetaID("2877029E9201347B4F58E1EC0A4BCD1B")]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, OTHER_GROUP)]
    public class MonoBehaviourSystemWrapper : IEcsModule
    {
        public MonoBehaviour system;
        public void Import(EcsPipeline.Builder b)
        {
            b.Add(system);
        }
    }
}
