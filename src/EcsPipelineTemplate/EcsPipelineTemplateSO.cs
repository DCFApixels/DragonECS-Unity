﻿using DCFApixels.DragonECS.Unity.Internal;
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
        [ArrayElement]
        private string[] _layers = _defaultLayers.ToArray();

        [SerializeField]
        [ArrayElement]
        private Record[] _records;

        public ReadOnlySpan<string> Layers
        {
            get { return _layers; }
        }
        public ReadOnlySpan<Record> Records
        {
            get { return _records; }
        }

        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            b.Layers.MergeWith(_layers);
            foreach (var s in _records)
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
            result.records = new EcsPipelineTemplate.Record[_records.Length];
            for (int i = 0; i < result.records.Length; i++)
            {
                ref var s = ref _records[i];
                result.records[i] = new EcsPipelineTemplate.Record(s.target, s.parameters);
            }
            return result;
        }

        public void SetFromSerializableTemplate(EcsPipelineTemplate template)
        {
            _layers = new string[template.layers.Length];
            Array.Copy(template.layers, _layers, template.layers.Length);
            _records = new Record[template.records.Length];
            for (int i = 0; i < _records.Length; i++)
            {
                ref var s = ref template.records[i];
                _records[i] = new Record(s.target, s.parameters);
            }
        }

        public bool Validate()
        {
            bool resutl = ValidateLayers();
            resutl |= ValidateSystems();
            return resutl;
        }
        private bool ValidateLayers()
        {
            bool result = false;
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
                    if (newLayers[pair.Value] != _defaultLayers[i])
                    {
                        newLayers[pair.Value] = _defaultLayers[i];
                        result = true;
                    }
                    i++;
                }
            }

            _layers = newLayers.ToArray();
            return result;
        }
        private bool ValidateSystems()
        {
            return false;
        }

        [Serializable]
        public struct Record
        {
            [SerializeReference]
            [ReferenceButton(true, typeof(IEcsModule), typeof(IEcsProcess))]
            [ArrayElement]
            public object target;// нельзя менять поярдок полей, иначе это поломает отрисовку в инспекторе изза применения property.Next(bool);
            public AddParams parameters;
            public Record(object target, AddParams parameters)
            {
                this.target = target;
                this.parameters = parameters;
            }
        }
    }
}