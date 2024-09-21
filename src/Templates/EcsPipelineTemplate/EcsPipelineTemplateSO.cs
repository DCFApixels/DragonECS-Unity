using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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





        [SerializeField]
        //[HideInInspector]
        private int _saveID;

        [Serializable]
        private struct SavedRecordList
        {
            public SavedRecord[] records;
            public SavedRecordList(SavedRecord[] records)
            {
                this.records = records;
            }
        }
        [Serializable]
        private struct SavedRecord
        {
            public int index;
            public string metaID;
            public string recordJson;
            public SavedRecord(int index, string metaID, string recordJson)
            {
                this.index = index;
                this.metaID = metaID;
                this.recordJson = recordJson;
            }
        }

        private void Awake()
        {
            Load();
        }

        private void Load()
        {
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
            if (_saveID != -1)
            {
                bool isChanged = false;
                var savedRecords = UnityEditorCache.instance.Get<SavedRecordList>(ref _saveID).records;
                if (savedRecords != null)
                {
                    for (int i = 0; i < savedRecords.Length; i++)
                    {
                        ref var savedRecord = ref savedRecords[i];
                        if (savedRecord.index < _records.Length)
                        {
                            ref var record = ref _records[savedRecord.index];
                            if (record.target == null && string.IsNullOrEmpty(savedRecord.metaID) == false)
                            {
                                record.target = RecoveryReferenceUtility.TryRecoverReference<object>(savedRecord.metaID);
                                if (record.target == null) { return; }
                                JsonUtility.FromJsonOverwrite(savedRecord.recordJson, record.target);
                                isChanged = true;
                            }
                        }
                    }
                }
                if (isChanged)
                {
                    EditorUtility.SetDirty(this);
                }
            }
#endif
        }
        private void Save()
        {
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
            if (_saveID == -1 || EditorApplication.isPlaying == false)
            {
                SavedRecord[] savedRecords = new SavedRecord[_records.Length];
                SavedRecordList list = new SavedRecordList(savedRecords);
                for (int i = 0; i < _records.Length; i++)
                {
                    ref var record = ref _records[i];
                    string metaid = record.target.GetMeta().MetaID;
                    if (string.IsNullOrEmpty(metaid) == false)
                    {
                        savedRecords[i] = new SavedRecord(i, metaid, JsonUtility.ToJson(record.target));
                    }
                    else
                    {
                        savedRecords[i] = default;
                    }
                }
                UnityEditorCache.instance.Set(list, ref _saveID);
                //RecoveryReferencesCache.instance.Save();
            }
#endif
        }

#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
        private void OnValidate()
        {
            Save();
        }
#endif
    }
}