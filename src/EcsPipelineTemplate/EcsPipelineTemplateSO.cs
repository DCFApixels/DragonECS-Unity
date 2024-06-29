using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsPipelineTemplateSO), menuName = EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsPipelineTemplateSO), order = 1)]
    public class EcsPipelineTemplateSO : ScriptableObject, IEcsModule
    {
        private static string[] _defaultLayers = new string[]
        {
            EcsConsts.PRE_BEGIN_LAYER,
            EcsConsts.BEGIN_LAYER,
            EcsConsts.BASIC_LAYER,
            EcsConsts.END_LAYER,
            EcsConsts.POST_END_LAYER,
        };

        [SerializeField]
        private string[] _layers = _defaultLayers.ToArray();

        [SerializeField]
        private SystemRecord[] _systems;

        [SerializeField]
        [SerializeReference]
        [ReferenceButton]
        private IEcsModule[] _modules;

        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            b.Layers.MergeWith(_layers);
            foreach (var s in _systems)
            {
                if (s.system == null) { continue; }

                int? sortOrder = s.isCustomSortOrder ? s.sortOrder : default(int?);
                if (s.isUnique)
                {
                    b.AddUnique(s.system, s.layer, sortOrder);
                }
                else
                {
                    b.Add(s.system, s.layer, sortOrder);
                }
            }
        }

        public EcsPipelineTemplate GenerateSerializableTemplate()
        {
            EcsPipelineTemplate result = new EcsPipelineTemplate();
            result.layers = new string[_layers.Length];
            Array.Copy(_layers, result.layers, _layers.Length);
            result.systems = new EcsPipelineTemplate.SystemRecord[_systems.Length];
            for (int i = 0; i < result.systems.Length; i++)
            {
                ref var s = ref _systems[i];
                result.systems[i] = new EcsPipelineTemplate.SystemRecord(s.system, s.layer, s.NullableSortOrder, s.isUnique);
            }
            return result;
        }

        public void SetFromSerializableTemplate(EcsPipelineTemplate template)
        {
            _layers = new string[template.layers.Length];
            Array.Copy(template.layers, _layers, template.layers.Length);
            _systems = new SystemRecord[template.systems.Length];
            for (int i = 0; i < _systems.Length; i++)
            {
                ref var s = ref template.systems[i];
                _systems[i] = new SystemRecord(s.system, s.layer, s.NullableSortOrder, s.isUnique);
            }
        }

        public void Validate()
        {
            ValidateLayers();
            ValidateSystems();
        }
        private void ValidateLayers()
        {
            Dictionary<string, int> builtinLayerIndexes = new Dictionary<string, int>();
            foreach (var item in _defaultLayers)
            {
                builtinLayerIndexes.Add(item, -1);
            }

            List<string> newLayers = _layers.Distinct().ToList();

            for (int i = 0; i < newLayers.Count; i++)
            {
                var layer = newLayers[i];
                if (builtinLayerIndexes.ContainsKey(layer))
                {
                    builtinLayerIndexes[layer] = i;
                }
            }

            int lastNewLayersCount = newLayers.Count;
            foreach (var pair in builtinLayerIndexes)
            {
                if(pair.Value < 0)
                {
                    newLayers.Add(pair.Key);
                }
            }
            for (int i = lastNewLayersCount; i < newLayers.Count; i++)
            {
                builtinLayerIndexes[newLayers[i]] = i;
            }

            {
                int i = 0;
                foreach (var pair in builtinLayerIndexes.OrderBy(o => o.Value))
                {
                    newLayers[pair.Value] = _defaultLayers[i];
                    i++;
                }
            }

            _layers = newLayers.ToArray();
        }
        private void ValidateSystems()
        {

        }

        [Serializable]
        public struct SystemRecord
        {
            [SerializeReference]
            [ReferenceButton]
            public IEcsProcess system;
            public string layer;
            public int sortOrder;
            public bool isCustomSortOrder;
            public bool isUnique;
            public int? NullableSortOrder { get { return isCustomSortOrder ? sortOrder : default(int?); } }
            public SystemRecord(IEcsProcess system, string layer, int? sortOrder, bool isUnique)
            {
                this.system = system;
                this.layer = layer;
                this.sortOrder = sortOrder.HasValue ? sortOrder.Value : 0;
                isCustomSortOrder = sortOrder.HasValue;
                this.isUnique = isUnique;
            }
        }
    }
}
