#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;
using System.Reflection;

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
            get
            {
                //return
                //    string.IsNullOrEmpty(_newTypeData.ClassName) == false &&
                //    string.IsNullOrEmpty(_newTypeData.AssemblyName) == false;
                return FindNewType() != null;
            }
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
        private Type _chachedNewType = null;
        private bool _chachedNewTypeInited = false;
        public Type FindNewType()
        {
            if (_chachedNewTypeInited == false)
            {
                if (string.IsNullOrEmpty(_newTypeData.AssemblyName) == false)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(_newTypeData.AssemblyName);
                    }
                    catch { }
                    if (assembly == null)
                    {
                        _chachedNewType = null;
                    }
                    else
                    {
                        string fullTypeName = $"{_newTypeData.NamespaceName}.{_newTypeData.ClassName}";
                        _chachedNewType = assembly.GetType(fullTypeName);
                    }
                    _chachedNewTypeInited = true;
                }
            }
            return _chachedNewType;
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
    }
}
#endif