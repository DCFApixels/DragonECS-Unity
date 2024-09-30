#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Editors;
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;
using System.Collections.Generic;
using System.Text;
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
            StringBuilder sb = null;
            foreach (var meta in typeMetas)
            {
                var type = meta.Type;
                string name = null;
                if (type.DeclaringType == null)
                {
                    name = type.Name;
                }
                else
                {
                    Type iteratorType = type;
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    sb.Clear();
                    sb.Append(iteratorType.Name);
                    while ((iteratorType = iteratorType.DeclaringType) != null)
                    {
                        sb.Insert(0, '/');
                        sb.Insert(0, iteratorType.Name);
                    }
                    name = sb.ToString();
                }

                var key = new TypeData(name, type.Namespace, type.Assembly.GetName().Name);
                var metaID = meta.MetaID;

                //Debug.LogWarning(type + " " + metaID);

                if (_typeKeyMetaIDPairs.TryGetValue(key, out string keyMetaID))
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