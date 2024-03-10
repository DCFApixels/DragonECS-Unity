using DCFApixels.DragonECS.Unity.Editors;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.Gray)]
    internal class PipelineMonitor : MonoBehaviour
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

    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.Gray)]
    internal class PipelineMonitorSystem : IEcsInit, IEcsPipelineMember, IEcsDestroy
    {
        private PipelineMonitor _monitor;
        private PipelineProcessMonitor _processesMonitor;
        public EcsPipeline Pipeline { get; set; }

        public void Init()
        {
            TypeMeta meta = typeof(EcsPipeline).ToMeta();
            _monitor = new GameObject($"{UnityEditorUtility.TransformToUpperName(meta.Name)}").AddComponent<PipelineMonitor>();
            UnityEngine.Object.DontDestroyOnLoad(_monitor);
            _monitor.Set(Pipeline);
            _monitor.gameObject.SetActive(false);

            _processesMonitor = new GameObject($"PROCESS_MATRIX").AddComponent<PipelineProcessMonitor>();
            _processesMonitor.transform.SetParent(_monitor.transform);
            _processesMonitor.Set(Pipeline);
            _processesMonitor.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(_monitor);
        }
    }
}
