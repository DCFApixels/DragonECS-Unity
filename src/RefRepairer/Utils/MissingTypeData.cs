#if UNITY_EDITOR
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal readonly struct MissingTypeData
    {
        public readonly ManagedReferenceMissingType Data;
        public readonly UnityObjectDataBase UnityObject;

        public MissingTypeData(ManagedReferenceMissingType missingType, UnityObjectDataBase unityObject)
        {
            Data = missingType;
            UnityObject = unityObject;
        }
    }
}
#endif