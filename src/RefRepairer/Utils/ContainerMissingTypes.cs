#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System.Collections.Generic;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class ContainerMissingTypes
    {
        public readonly TypeData TypeData;
        public readonly string ReplacedLine;
        public readonly List<MissingTypeData> ManagedReferencesMissingTypeDatas = new List<MissingTypeData>();
        public ContainerMissingTypes(TypeData typeData)
        {
            TypeData = typeData;
            ReplacedLine = FileRepaireUtility.GenerateReplacedLine(typeData);
        }
    }
}
#endif