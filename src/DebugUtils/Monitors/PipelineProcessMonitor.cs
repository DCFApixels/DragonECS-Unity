using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    internal class PipelineProcessMonitor : MonoBehaviour
    {
        private EcsPipeline _pipeline;
        public EcsPipeline Pipeline
        {
            get { return _pipeline; }
        }
        public void Set(EcsPipeline pipeline)
        {
            _pipeline = pipeline;
        }
    }
}
