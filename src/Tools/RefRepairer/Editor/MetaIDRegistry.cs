#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Editors;
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    [InitializeOnLoad]
    [FilePath(EcsUnityConsts.LOCAL_CACHE_FOLDER + "/" + nameof(MetaIDRegistry) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    internal class MetaIDRegistry : ScriptableSingleton<MetaIDRegistry>, ISerializationCallbackReceiver
    {
        #region [SerializeField]
        [Serializable]
        private struct Pair
        {
            public TypeDataSerializable key;
            public string value;
            public Pair(TypeDataSerializable key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }
        [SerializeField]
        private Pair[] _typeKeyMetaIDPairsSerializable;
        #endregion
        private Dictionary<TypeData, string> _typeKeyMetaIDPairs = new Dictionary<TypeData, string>();
        internal IReadOnlyDictionary<TypeData, string> TypeKeyMetaIDPairs => _typeKeyMetaIDPairs;
        private bool _isChanged = false;

        public bool TryGetMetaID(TypeData key, out string metaID)
        {
            bool result = _typeKeyMetaIDPairs.TryGetValue(key, out metaID);
            if (result && string.IsNullOrEmpty(metaID))
            {
                result = false;
                _typeKeyMetaIDPairs.Remove(key);
            }
            return result;
        }


        static MetaIDRegistry()
        {
            EditorApplication.update += BeforeCompilation;
        }
        private static void BeforeCompilation()
        {
            EditorApplication.update -= BeforeCompilation;
            instance.TryGetMetaID(TypeData.Empty, out _);
            instance.Update();
        }

        #region Update
        public void Reinit()
        {
            _typeKeyMetaIDPairs.Clear();
            Update();
        }
        private void Update()
        {
            if (UnityEditorUtility.IsHasAnyMetaIDCollision) { return; }
            var typeMetas = UnityEditorUtility._typeWithMetaIDMetas;
            foreach (var meta in typeMetas)
            {
                var typeKey = new TypeData(meta.Type);
                var metaID = meta.MetaID;

                //Debug.LogWarning(type + " " + metaID);

                if (_typeKeyMetaIDPairs.TryGetValue(typeKey, out string storedMetaID))
                {
                    if (storedMetaID != metaID)
                    {
                        _typeKeyMetaIDPairs[typeKey] = string.Empty; //Таким образом помечаются моменты когда не однозначно какой идентификатор принадлежит этому имени
                        _isChanged = true;
                    }
                }
                else
                {
                    _typeKeyMetaIDPairs[typeKey] = metaID;
                    _isChanged = true;
                }
            }
            if (_isChanged)
            {
                _isChanged = false;
                EditorUtility.SetDirty(this);
                Save(true);
            }
        }
        #endregion

        #region ISerializationCallbackReceiver
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _typeKeyMetaIDPairs.Clear();
            foreach (var pair in _typeKeyMetaIDPairsSerializable)
            {
                if (string.IsNullOrEmpty(pair.value) || pair.key.IsEmpty) { continue; }

                _typeKeyMetaIDPairs[pair.key] = pair.value;
            }
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            int i = 0;
            _typeKeyMetaIDPairsSerializable = new Pair[_typeKeyMetaIDPairs.Count];
            foreach (var pair in _typeKeyMetaIDPairs)
            {
                _typeKeyMetaIDPairsSerializable[i++] = new Pair(pair.Key, pair.Value);
            }
        }
        #endregion
    }
}
#endif