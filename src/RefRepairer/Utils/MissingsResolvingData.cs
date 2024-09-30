#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class MissingsResolvingData
    {
        public readonly TypeData OldTypeData;
        public readonly string OldSerializedInfoLine;
        private TypeData _newTypeData;
        private string _newSerializedInfoLine;
        public MissingsResolvingData(TypeData oldTypeData)
        {
            OldTypeData = oldTypeData;
            OldSerializedInfoLine = RepaireFileUtility.GenerateReplacedLine(oldTypeData);
        }
        public bool IsResolved
        {
            get { return string.IsNullOrEmpty(_newTypeData.ClassName) == false; }
        }
        public TypeData NewTypeData
        {
            get { return _newTypeData; }
            set
            {
                _newTypeData = value;
                _newSerializedInfoLine = null;
            }
        }
        public string NewSerializedInfoLine
        {
            get
            {
                if (_newSerializedInfoLine == null)
                {
                    _newSerializedInfoLine = RepaireFileUtility.GenerateReplacedLine(_newTypeData);
                }
                return _newSerializedInfoLine;
            }
        }
    }
}
#endif