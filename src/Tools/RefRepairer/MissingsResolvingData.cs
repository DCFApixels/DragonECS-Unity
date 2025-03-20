#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class MissingsResolvingData
    {
        public readonly TypeData OldTypeData;
        public readonly string OldSerializedInfoLine;

        private TypeData _newTypeData;
        private string _newSerializedInfoLine;

        private Type _chachedNewType = null;
        private bool _chachedNewTypeInited = false;

        public MissingsResolvingData(TypeData oldTypeData)
        {
            OldTypeData = oldTypeData;
            OldSerializedInfoLine = RepaireFileUtility.GenerateReplacedLine(oldTypeData);
        }
        public bool IsResolved
        {
            get { return FindNewType() != null; }
        }
        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrEmpty(_newTypeData.ClassName) ||
                    string.IsNullOrEmpty(_newTypeData.AssemblyName);
            }
        }
        public TypeData NewTypeData
        {
            get { return _newTypeData; }
            set
            {
                _newTypeData = value;
                _newSerializedInfoLine = null;
                _chachedNewType = null;
                _chachedNewTypeInited = false;
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

        public Type FindNewType()
        {
            if (_chachedNewTypeInited == false)
            {
                _chachedNewType = _newTypeData.ToType();
            }
            return _chachedNewType;
        }
    }
}
#endif