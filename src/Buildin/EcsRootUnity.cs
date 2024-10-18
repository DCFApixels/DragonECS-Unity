using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsRootUnity), 30)]
    public class EcsRootUnity : MonoBehaviour
    {
        [SerializeField]
        private ScriptablePipelineTemplateBase[] _scriptableTemplates;
        [SerializeField]
        private MonoPipelineTemplateBase[] _monoTemplates;

        private EcsPipeline _pipeline;

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
            get { return _pipeline != null && _pipeline.IsInit; }
        }

        private void Start()
        {
            var pipelineBuilder = EcsPipeline.New();
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
            _pipeline = pipelineBuilder.BuildAndInit();
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
}
