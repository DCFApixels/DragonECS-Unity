using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Linq;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static PipelineTemplateUtility;

    public abstract class ScriptablePipelineTemplateBase : ScriptableObject, IEcsModule
    {
        public abstract void Import(EcsPipeline.Builder b);
    }

    [CreateAssetMenu(fileName = nameof(ScriptablePipelineTemplate), menuName = EcsConsts.FRAMEWORK_NAME + "/" + nameof(ScriptablePipelineTemplate), order = 1)]
    public sealed class ScriptablePipelineTemplate : ScriptablePipelineTemplateBase, IPipelineTemplate
    {
        [SerializeField]
        [ArrayElement]
        private string[] _layers = DefaultLayers.ToArray();

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

        public sealed override void Import(EcsPipeline.Builder b)
        {
            b.Layers.MergeWith(_layers);
            foreach (var s in _records)
            {
                if (s.target == null) { continue; }
                b.Add(s.target, s.parameters);
            }
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
            bool resutl = ValidateLayers(ref _layers);
            resutl |= ValidateRecords(ref _records);
            return resutl;
        }
    }
}