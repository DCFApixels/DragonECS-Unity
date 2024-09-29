#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
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