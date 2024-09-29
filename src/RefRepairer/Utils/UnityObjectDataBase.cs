#if UNITY_EDITOR
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal abstract class UnityObjectDataBase
    {
        //public string LocalAssetPath => AssetDatabase.GUIDToAssetPath(AssetGuid);
        public abstract GUID AssetGuid { get; }
        public string GetLocalAssetPath()
        {
            return AssetDatabase.GUIDToAssetPath(AssetGuid);
        }
    }
}
#endif