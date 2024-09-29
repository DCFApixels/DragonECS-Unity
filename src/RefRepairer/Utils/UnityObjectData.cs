#if UNITY_EDITOR
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class UnityObjectData : UnityObjectDataBase
    {
        private readonly GUID _assetGUID;
        public readonly UnityObject UnityObject;
        public sealed override GUID AssetGuid { get { return _assetGUID; } }
        public UnityObjectData(UnityObject unityObject, string pathToPrefab)
        {
            _assetGUID = AssetDatabase.GUIDFromAssetPath(pathToPrefab);
            UnityObject = unityObject;
        }
    }
}
#endif