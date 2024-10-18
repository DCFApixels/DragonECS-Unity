using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class DragonEcsRootUnity : MonoBehaviour
    {
        [SerializeReference]
        [ReferenceButton(false, typeof(IEcsModule))]
        //[ArrayElement]
        private IEcsModule _module = new MonoBehaviourSystemWrapper();
        [SerializeField]
        private AddParams _parameters;

        private EcsPipeline _pipeline;

        public IEcsModule Module
        {
            get { return _module; }
        }
        public AddParams AddParams
        {
            get { return _parameters; }
        }
        public EcsPipeline Pipeline
        {
            get { return _pipeline; }
        }
        public bool IsInit
        {
            get { return _pipeline != null && _pipeline.IsInit; }
        }

        private void Start()
        {
            _pipeline = EcsPipeline.New().AddModule(_module, _parameters).BuildAndInit();
        }

        private void Update()
        {
            _pipeline.Run();
        }

        private void LateUpdate()
        {
            _pipeline.LateRun();
        }

        private void FixedUpdate()
        {
            _pipeline.FixedRun();
        }

        private void OnDrawGizmos()
        {
            _pipeline?.DrawGizmos();
        }

        private void OnDestroy()
        {
            _pipeline.Destroy();
        }
    }
}
