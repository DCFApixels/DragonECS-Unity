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
        private Record[] _systems;

        [SerializeField]
        [SerializeReference]
        [ReferenceButton]
        private IEcsModule[] _modules;

        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            b.Layers.MergeWith(_layers);
            foreach (var s in _systems)
            {
                if (s.target == null) { continue; }

                b.Add(s.target, s.parameters);
            }
        }

        public EcsPipelineTemplate GenerateSerializableTemplate()
        {
            EcsPipelineTemplate result = new EcsPipelineTemplate();
            result.layers = new string[_layers.Length];
            Array.Copy(_layers, result.layers, _layers.Length);
            result.systems = new EcsPipelineTemplate.AddCommand[_systems.Length];
            for (int i = 0; i < result.systems.Length; i++)
            {
                ref var s = ref _systems[i];
                result.systems[i] = new EcsPipelineTemplate.AddCommand(s.target, s.parameters);
            }
            return result;
        }

        public void SetFromSerializableTemplate(EcsPipelineTemplate template)
        {
            _layers = new string[template.layers.Length];
            Array.Copy(template.layers, _layers, template.layers.Length);
            _systems = new Record[template.systems.Length];
            for (int i = 0; i < _systems.Length; i++)
            {
                ref var s = ref template.systems[i];
                _systems[i] = new Record(s.target, s.parameters);
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
                if (pair.Value < 0)
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
        public struct Record
        {
            [SerializeReference]
            [ReferenceButton]
            public object target;
            public AddParams parameters;
            public Record(object target, AddParams parameters)
            {
                this.target = target;
                this.parameters = parameters;
            }
        }
    }
}
