#if UNITY_EDITOR
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal readonly struct CollectedAssetMissingRecord
    {
        public readonly UnityObjectDataBase UnityObject;
        public readonly ManagedReferenceMissingType Missing;
        public readonly MissingsResolvingData ResolvingData;
        public bool IsResolvedOrNull
        {
            get { return UnityObject == null || ResolvingData.IsResolved; }
        }
        public bool IsNull
        {
            get { return UnityObject == null; }
        }
        public CollectedAssetMissingRecord(UnityObjectDataBase unityObject, ManagedReferenceMissingType missing, MissingsResolvingData resolvingData)
        {
            UnityObject = unityObject;
            Missing = missing;
            ResolvingData = resolvingData;
        }
    }
}
#endif