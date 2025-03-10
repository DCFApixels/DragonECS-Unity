using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsRootUnity), 30)]
    public class EcsRootUnity : MonoBehaviour
    {
        [SerializeField]
        private EcsPipelineProvider _pipelineProvider;
        [SerializeField]
        private bool _enablePipelineDebug = true;
        [SerializeField]
        private bool _enableWorldDebug = true;
        [SerializeField]
        private ScriptablePipelineTemplateBase[] _scriptableTemplates;
        [SerializeField]
        private MonoPipelineTemplateBase[] _monoTemplates;

        private EcsPipeline _pipeline;
        private bool _isInit = false;

        public IEnumerable<ScriptablePipelineTemplateBase> ScriptableTemplates
        {
            get { return _scriptableTemplates; }
        }
        public IEnumerable<MonoPipelineTemplateBase> MonoTemplates
        {
            get { return _monoTemplates; }
        }
        public EcsPipeline Pipeline
        {
            get { return _pipeline; }
        }
        public bool IsInit
        {
            get { return _isInit; }
        }
        public bool EnablePipelineDebug
        {
            get { return _enablePipelineDebug; }
        }
        public bool EnableWorldDebug
        {
            get { return _enableWorldDebug; }
        }

        public void ManualStart()
        {
            if (_isInit) { return; }

            var pipelineBuilder = EcsPipeline.New(new ConfigContainer(this));

            foreach (var template in _scriptableTemplates)
            {
                if (template == null) { continue; }
                pipelineBuilder.Add(template);
            }
            foreach (var template in _monoTemplates)
            {
                if (template == null) { continue; }
                pipelineBuilder.Add(template);
            }

#if UNITY_EDITOR
            if (_enablePipelineDebug)
            {
                pipelineBuilder.Layers.Insert(EcsConsts.POST_END_LAYER, EcsUnityConsts.DEBUG_LAYER);
                pipelineBuilder.AddUnique(new PipelineMonitorSystem(), EcsUnityConsts.DEBUG_LAYER);
            }
#endif
            _pipeline = pipelineBuilder.Build();
            if (_pipelineProvider != null)
            {
                _pipelineProvider.Set(_pipeline);
            }
            _pipeline.Init();

            _isInit = true;
        }
        private void Start()
        {
            ManualStart();
        }

        private void OnValidate()
        {
            if (_pipelineProvider == null)
            {
                _pipelineProvider = EcsPipelineProvider.SingletonInstance;
            }
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

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("Autoset")]
        internal void Autoset_Editor()
        {
            Autoset(this);
        }
        [ContextMenu("Validate")]
        internal void Validate_Editor()
        {
            _scriptableTemplates = _scriptableTemplates.Where(o => o != null).ToArray();
            _monoTemplates = _monoTemplates.Where(o => o != null).ToArray();
            EditorUtility.SetDirty(this);
        }

        private static void Autoset(EcsRootUnity target)
        {
            IEnumerable<MonoPipelineTemplateBase> result;
            if (target.MonoTemplates != null && target.MonoTemplates.Count() > 0)
            {
                result = target.MonoTemplates.Where(o => o != null).Union(GetTemplatesFor(target.transform));
            }
            else
            {
                result = GetTemplatesFor(target.transform);
            }

            target._monoTemplates = result.ToArray();
            EditorUtility.SetDirty(target);
        }
        private static IEnumerable<MonoPipelineTemplateBase> GetTemplatesFor(Transform parent)
        {
            IEnumerable<MonoPipelineTemplateBase> result = parent.GetComponents<MonoPipelineTemplateBase>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.TryGetComponent<EcsRootUnity>(out _))
                {
                    return Enumerable.Empty<MonoPipelineTemplateBase>();
                }
                result = result.Concat(GetTemplatesFor(child));
            }
            return result;
        }
#endif
        #endregion
    }
    public static class EcsRootUnityExt
    {
        public static bool IsEnableWorldDebug(this EcsPipeline.Builder self)
        {
            return self.Configs.Instance.Get<EcsRootUnity>().EnableWorldDebug;
        }
    }
}
