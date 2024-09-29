#if UNITY_EDITOR
using System;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Internal
{
    [Serializable]
    internal struct TypeDataSerializable
    {
        public string ClassName;
        public string NamespaceName;
        public string AssemblyName;
        public TypeDataSerializable(string typeName, string namespaceName, string assemblyName)
        {
            ClassName = typeName;
            NamespaceName = namespaceName;
            AssemblyName = assemblyName;
        }
        public static implicit operator TypeDataSerializable(TypeData type) { return new TypeDataSerializable(type.ClassName, type.NamespaceName, type.AssemblyName); }
        public static implicit operator TypeData(TypeDataSerializable type) { return new TypeData(type.ClassName, type.NamespaceName, type.AssemblyName); }
    }
    internal readonly struct TypeData : IEquatable<TypeData>
    {
        public readonly string ClassName;
        public readonly string NamespaceName;
        public readonly string AssemblyName;
        public TypeData(ManagedReferenceMissingType managedReferenceMissingType)
        {
            ClassName = managedReferenceMissingType.className;
            NamespaceName = managedReferenceMissingType.namespaceName;
            AssemblyName = managedReferenceMissingType.assemblyName;
        }
        public TypeData(string typeName, string namespaceName, string assemblyName)
        {
            ClassName = typeName;
            NamespaceName = namespaceName;
            AssemblyName = assemblyName;
        }
        public bool Equals(TypeData other)
        {
            return ClassName == other.ClassName &&
                NamespaceName == other.NamespaceName &&
                AssemblyName == other.AssemblyName;
        }
        public override bool Equals(object obj)
        {
            return Equals((TypeData)obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(ClassName, NamespaceName, AssemblyName);
        }
    }
}
#endif