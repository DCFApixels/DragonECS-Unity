using DCFApixels.DragonECS.RunnersCore;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DCFApixels.DragonECS.Unity.PipelineTemplateUtility;

namespace DCFApixels.DragonECS.Unity
{
    public interface IPipelineTemplate
    {
        ReadOnlySpan<string> Layers { get; }
        ReadOnlySpan<Record> Records { get; }
        bool Validate();
    }
    public static class PipelineTemplateUtility
    {
        internal static readonly string[] DefaultLayers = new string[]
        {
            EcsConsts.PRE_BEGIN_LAYER,
            EcsConsts.BEGIN_LAYER,
            EcsConsts.BASIC_LAYER,
            EcsConsts.END_LAYER,
            EcsConsts.POST_END_LAYER,
        };

        internal static bool ValidateLayers(ref string[] layers)
        {
            bool result = false;
            Dictionary<string, int> builtinLayerIndexes = new Dictionary<string, int>();
            foreach (var item in DefaultLayers)
            {
                builtinLayerIndexes.Add(item, -1);
            }

            List<string> newLayers = layers.Distinct().ToList();

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
                    if (newLayers[pair.Value] != DefaultLayers[i])
                    {
                        newLayers[pair.Value] = DefaultLayers[i];
                        result = true;
                    }
                    i++;
                }
            }

            layers = newLayers.ToArray();
            return result;
        }
        internal static bool ValidateRecords(ref Record[] records)
        {
            return false;
        }

        public static EcsPipelineTemplate GenerateSerializableTemplate(this IPipelineTemplate self)
        {
            EcsPipelineTemplate result = new EcsPipelineTemplate();
            result.layers = new string[self.Layers.Length];
            for (int i = 0; i < self.Layers.Length; i++)
            {
                result.layers[i] = self.Layers[i];
            }
            result.records = new EcsPipelineTemplate.Record[self.Records.Length];
            for (int i = 0; i < result.records.Length; i++)
            {
                var s = self.Records[i];
                result.records[i] = new EcsPipelineTemplate.Record(s.target, s.parameters);
            }
            return result;
        }

        [Serializable]
        public struct Record
        {
            [SerializeReference]
            [ReferenceButton(true, typeof(IEcsModule), typeof(IEcsProcess))]
            [ReferenceButtonWithOut(typeof(IEcsRunner))]
            [ArrayElement]
            public object target;// нельзя менять поярдок полей, иначе это поломает отрисовку в инспекторе изза применения property.Next(bool);
            public AddParams parameters;
            [CustomToggle(Name = "Enable", IsLeft = true, IsInverted = true)]
            public bool disabled;
            public Record(object target, AddParams parameters, bool disabled)
            {
                this.target = target;
                this.parameters = parameters;
                this.disabled = disabled;
            }
        }
    }
}
