using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    public abstract class MonoEntityTemplateBase : MonoBehaviour, ITemplate
    {
        [SerializeField]
        private int _saveID;
        public abstract void Apply(short worldID, int entityID);

        private static IComponentTemplate _fake = null;
        protected virtual IList<IComponentTemplate> GetToRecover() { return null; }
        protected virtual ref IComponentTemplate GetToRecoverSingle() { return ref _fake; }

        [Serializable]
        private struct SavedRecordList
        {
            public SavedRecord[] records;
            public SavedRecord singleRecord;
            public SavedRecordList(SavedRecord[] templates)
            {
                this.records = templates;
                singleRecord = default;
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
        protected void Save()
        {
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
            if (_saveID == -1 || EditorApplication.isPlaying == false)
            {
                var records = GetToRecover();
                var recordSingle = GetToRecoverSingle();

                SavedRecord[] savedRecords = new SavedRecord[records.Count];
                SavedRecordList list = new SavedRecordList(savedRecords);

                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    string metaid = record.GetMeta().MetaID;
                    if (string.IsNullOrEmpty(metaid) == false)
                    {
                        savedRecords[i] = new SavedRecord(i, metaid, JsonUtility.ToJson(record));
                    }
                    else
                    {
                        savedRecords[i] = default;
                    }
                }
                if(recordSingle != null)
                {
                    string metaid = recordSingle.GetMeta().MetaID;
                    list.singleRecord = new SavedRecord(-1, metaid, JsonUtility.ToJson(recordSingle));
                }
                UnityEditorCache.instance.Set(list, ref _saveID);
            }
#endif
        }
        protected void Load()
        {
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
            if (_saveID != -1)
            {
                bool isChanged = false;
                SavedRecordList list = UnityEditorCache.instance.Get<SavedRecordList>(ref _saveID);
                var savedRecords = list.records;
                if (savedRecords != null)
                {
                    var records = GetToRecover();
                    for (int i = 0; i < savedRecords.Length; i++)
                    {
                        ref var savedRecord = ref savedRecords[i];
                        if (savedRecord.index < records.Count)
                        {
                            var record = records[savedRecord.index];
                            if (record == null && string.IsNullOrEmpty(savedRecord.metaID) == false)
                            {
                                record = RecoveryReferenceUtility.TryRecoverReference<IComponentTemplate>(savedRecord.metaID);
                                records[savedRecord.index] = record;
                                if (record == null) { return; }
                                JsonUtility.FromJsonOverwrite(savedRecord.recordJson, record);
                                isChanged = true;
                            }
                        }
                    }
                }
                ref var recordSingle = ref GetToRecoverSingle();
                if (string.IsNullOrEmpty(list.singleRecord.metaID))
                {

                }
                if (isChanged)
                {
                    EditorUtility.SetDirty(this);
                }
            }
#endif
        }
    }

    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(MonoEntityTemplate), 30)]
    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, nameof(MonoBehaviour) + " implementation of an entity template. Templates are a set of components that are applied to entities.")]
    public class MonoEntityTemplate : MonoEntityTemplateBase, ITemplateInternal
    {
        [SerializeReference]
        private IComponentTemplate[] _components;

        #region Properties
        string ITemplateInternal.ComponentsPropertyName
        {
            get { return nameof(_components); }
        }
        #endregion

        #region Methods
        public override void Apply(short worldID, int entityID)
        {
            foreach (var item in _components)
            {
                item.Apply(worldID, entityID);
            }
        }
        public void Clear()
        {
            _components = Array.Empty<IComponentTemplate>();
        }
        #endregion

        #region UnityEvents
        private void OnValidate()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnValidate(gameObject);
            }
        }
        private void OnDrawGizmos()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Always);
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Selected);
            }
        }
        #endregion
    }
}
