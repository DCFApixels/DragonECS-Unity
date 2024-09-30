#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System.Collections.Generic;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class ContainerMissingRefs
    {
        public readonly TypeData TypeData;
        public readonly string ReplacedLine;
        public readonly List<MissingTypeData> ManagedReferencesMissingTypeDatas = new List<MissingTypeData>(4);
        public readonly bool IsHasMetaIDRegistry;
        public ContainerMissingRefs(TypeData typeData)
        {
            TypeData = typeData;
            ReplacedLine = RepaireFileUtility.GenerateReplacedLine(typeData);
            IsHasMetaIDRegistry = MetaIDRegistry.instance.TryGetMetaID(TypeData, out _);
        }
    }
}
#endif