using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.Gray)]
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
