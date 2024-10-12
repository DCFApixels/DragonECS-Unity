﻿using DCFApixels.DragonECS.Unity;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [MetaName("ScriptableObjectSystem")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaDescription("Wrapper for ScriptableObject systems")]
    [MetaID("F5A94C5F92015B4D9286E76809311AF4")]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.OTHER_GROUP)]
    public class ScriptableObjectSystemWrapper : IEcsModule
    {
        public ScriptableObject system;
        public void Import(EcsPipeline.Builder b)
        {
            b.Add(system);
        }
    }
}
