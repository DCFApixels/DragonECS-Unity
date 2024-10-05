using UnityEngine;

namespace DCFApixels.DragonECS
{
    [MetaName("ScriptableObjectSystem")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaDescription("Wrapper for ScriptableObject systems")]
    public class ScriptableObjectSystemWrapper : IEcsModule
    {
        public ScriptableObject system;
        public void Import(EcsPipeline.Builder b)
        {
            b.Add(system);
        }
    }
}
