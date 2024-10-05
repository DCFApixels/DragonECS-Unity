#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal abstract class UnityObjectDataBase
    {
        public abstract GUID AssetGuid { get; }
        public string GetLocalAssetPath()
        {
            return AssetDatabase.GUIDToAssetPath(AssetGuid);
        }
    }

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

    internal class SceneObjectData : UnityObjectDataBase
    {
        private readonly GUID _assetGUID;
        public readonly string SceneName;
        public sealed override GUID AssetGuid { get { return _assetGUID; } }
        public SceneObjectData(Scene scene)
        {
            _assetGUID = AssetDatabase.GUIDFromAssetPath(scene.path);
            SceneName = scene.name;
        }
    }
}
#endif