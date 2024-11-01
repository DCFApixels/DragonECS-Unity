using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Linq;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static PipelineTemplateUtility;

    public abstract class MonoPipelineTemplateBase : MonoBehaviour, IEcsModule
    {
        public abstract void Import(EcsPipeline.Builder b);
    }

    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(MonoPipelineTemplate), 30)]
    public class MonoPipelineTemplate : MonoPipelineTemplateBase, IPipelineTemplate, IEcsDefaultAddParams
    {
        [SerializeField]
        [ArrayElement]
        private string[] _layers = DefaultLayers.ToArray();

        [SerializeField]
        private AddParams _parameters;

        [SerializeField]
        [ArrayElement]
        private Record[] _records;

        public ReadOnlySpan<string> Layers
        {
            get { return _layers; }
        }
        public AddParams AddParams
        {
            get { return _parameters; }
        }
        public ReadOnlySpan<Record> Records
        {
            get { return _records; }
        }

        public sealed override void Import(EcsPipeline.Builder b)
        {
            b.Layers.MergeWith(_layers);
            foreach (var r in _records)
            {
                if (r.target == null || r.disabled) { continue; }
                b.Add(r.target, r.parameters);
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
                _records[i] = new Record(s.target, s.parameters, true);
            }
        }

        [ContextMenu("Validate")]
        public bool Validate()
        {
            bool resutl = ValidateLayers(ref _layers);
            resutl |= ValidateRecords(ref _records);
            return resutl;
        }
    }
}
