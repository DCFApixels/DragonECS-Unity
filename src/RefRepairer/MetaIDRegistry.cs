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
        private bool _isChanged = false;

        public bool TryGetMetaID(TypeData key, out string metaID)
        {
            return _typeKeyMetaIDPairs.TryGetValue(key, out metaID);
        }


        static MetaIDRegistry()
        {
            EditorApplication.update += BeforeCompilation;
        }
        private static void BeforeCompilation()
        {
            EditorApplication.update -= BeforeCompilation;
            instance.TryGetMetaID(default, out _);
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
            var typeMetas = UnityEditorUtility._serializableTypeWithMetaIDMetas;

            foreach (var meta in typeMetas)
            {
                var type = meta.Type;
                var key = new TypeData(type.Name, type.Namespace, type.Assembly.FullName);
                var metaID = meta.MetaID;

                if (_typeKeyMetaIDPairs.TryGetValue(key, out string keyMetaID) == false)
                {
                    if (keyMetaID != metaID)
                    {
                        _typeKeyMetaIDPairs[key] = null; //Таким образом помечаются моменты когда не однозначно какой идентификатор принадлежит этому имени
                        _isChanged = true;
                    }
                }
                else
                {
                    _typeKeyMetaIDPairs[key] = metaID;
                    _isChanged = true;
                }
            }

            if (_isChanged)
            {
                EditorUtility.SetDirty(this);
                _isChanged = false;
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
                if (string.IsNullOrEmpty(pair.value) == false)
                {
                    _typeKeyMetaIDPairs[pair.key] = pair.value;
                }
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