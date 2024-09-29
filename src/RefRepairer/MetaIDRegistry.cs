#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [FilePath(EcsUnityConsts.LOCAL_CACHE_FOLDER + "/" + nameof(MetaIDRegistry) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    internal class MetaIDRegistry : ScriptableSingleton<MetaIDRegistry>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private TypeDataList[] _typeDataLists;
        [SerializeField]
        private int _typeDataListsCount = 0;
        [SerializeField]
        private TypeDataNode[] _typeDataNodes;
        [SerializeField]
        private int _typeDataNodesCount = 0;
        #region [SerializeField]
        private struct Pair
        {
            public string metaID;
            public int listIndex;
            public Pair(string metaID, int listIndex)
            {
                this.metaID = metaID;
                this.listIndex = listIndex;
            }
        }
        private Pair[] _metaIDListIndexPairsSerializable;
        #endregion
        private Dictionary<string, int> _metaIDListIndexPairs = new Dictionary<string, int>();



        #region Update
        private void Update()
        {
            var typeMetas = UnityEditorUtility._serializableTypeWithMetaIDMetas;
            bool isChanged = false;

            for (int i = 0; i < _typeDataListsCount; i++)
            {
                _typeDataLists[i].containsFlag = false;
            }

            foreach (var meta in typeMetas)
            {
                var type = meta.Type;

                var name = type.Name;
                var nameSpace = type.Namespace;
                var assembly = type.Assembly.FullName;

                if (_metaIDListIndexPairs.TryGetValue(meta.MetaID, out int listIndex) == false)
                {
                    if (_typeDataLists.Length <= _typeDataListsCount)
                    {
                        Array.Resize(ref _typeDataLists, _typeDataLists.Length << 1);
                    }
                    listIndex = _typeDataListsCount++;
                    _metaIDListIndexPairs.Add(meta.MetaID, listIndex);
                    isChanged = true;
                }

                ref var listRef = ref _typeDataLists[listIndex];
                listRef.containsFlag = true;
                if (listRef.count > 0 && _typeDataNodes[listRef.startNode].EqualsWith(name, nameSpace, assembly))
                {
                    continue;
                }

                if (_typeDataNodes.Length <= _typeDataNodesCount)
                {
                    Array.Resize(ref _typeDataNodes, _typeDataNodes.Length << 1);
                }
                int nodeIndex = _typeDataNodesCount++;
                ref var nodeRef = ref _typeDataNodes[nodeIndex];
                isChanged = true;

                nodeRef = new TypeDataNode(name, nameSpace, assembly);
                nodeRef.next = listRef.startNode;
                listRef.startNode = listIndex;
                listRef.count++;
            }

            for (int i = 0; i < _typeDataListsCount; i++)
            {
                ref var list = ref _typeDataLists[i];
                if (list.containsFlag == false)
                {
                    _metaIDListIndexPairs.Remove();
                }
            }

            if (isChanged)
            {
                EditorUtility.SetDirty(this);
            }
        }
        #endregion

        #region ISerializationCallbackReceiver
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _metaIDListIndexPairs.Clear();
            if (_typeDataNodes == null)
            {
                _typeDataLists = new TypeDataList[256];
                _typeDataListsCount = 0;
                _typeDataNodes = new TypeDataNode[256];
                _typeDataNodesCount = 0;
            }
            else
            {
                foreach (var pair in _metaIDListIndexPairsSerializable)
                {
                    _metaIDListIndexPairs[pair.metaID] = pair.listIndex;
                }
            }
            Update();
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            int i = 0;
            _metaIDListIndexPairsSerializable = new Pair[_metaIDListIndexPairs.Count];
            foreach (var pair in _metaIDListIndexPairs)
            {
                _metaIDListIndexPairsSerializable[i++] = new Pair(pair.Key, pair.Value);
            }
        }
        #endregion

        #region Utils
        [Serializable]
        public struct TypeDataList
        {
            public string metaID_key;
            public bool containsFlag;
            public int startNode;
            public int count;
        }
        [Serializable]
        public struct TypeDataNode : ILinkedNext
        {
            public readonly string Name;
            public readonly string Namespace;
            public readonly string Assembly;
            public int next;

            public bool EqualsWith(string name, string nameSpace, string assembly)
            {
                return name == Name && nameSpace == Namespace && assembly == Assembly;
            }
            public TypeDataNode(string name, string nameSpace, string assembly) : this()
            {
                Name = name;
                Namespace = nameSpace;
                Assembly = assembly;
            }
            int ILinkedNext.Next
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return next; }
            }
        }
        #endregion
    }
}
#endif