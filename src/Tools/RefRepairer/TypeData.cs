#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Internal
{
    [Serializable]
    internal struct TypeDataSerializable
    {
        public string ClassName;
        public string NamespaceName;
        public string AssemblyName;
        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(ClassName); } }
        public TypeDataSerializable(string typeName, string namespaceName, string assemblyName)
        {
            ClassName = typeName;
            NamespaceName = namespaceName;
            AssemblyName = assemblyName;
        }
        public static implicit operator TypeDataSerializable(TypeData type) { return new TypeDataSerializable(type.ClassName, type.NamespaceName, type.AssemblyName); }
        public static implicit operator TypeData(TypeDataSerializable type) { return new TypeData(type.ClassName, type.NamespaceName, type.AssemblyName); }
        public override string ToString() { return $"{{{AssemblyName}, {NamespaceName}, {ClassName}}}"; }
    }
    internal readonly struct TypeData : IEquatable<TypeData>
    {
        public static readonly TypeData Empty = new TypeData(string.Empty, string.Empty, string.Empty);
        public readonly string ClassName;
        public readonly string NamespaceName;
        public readonly string AssemblyName;
        public TypeData(ManagedReferenceMissingType managedReferenceMissingType)
        {
            ClassName = managedReferenceMissingType.className ?? string.Empty;
            NamespaceName = managedReferenceMissingType.namespaceName ?? string.Empty;
            AssemblyName = managedReferenceMissingType.assemblyName ?? string.Empty;
        }
        public TypeData(string typeName, string namespaceName, string assemblyName)
        {
            ClassName = typeName;
            NamespaceName = namespaceName;
            AssemblyName = assemblyName;
        }
        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(ClassName); } }
        [ThreadStatic]
        private static StringBuilder sb;
        public TypeData(Type type)
        {
            string name = string.Empty;
            if (type.DeclaringType == null)
            {
                name = type.Name;
            }
            else
            {
                Type iteratorType = type;
                if (sb == null)
                {
                    sb = new StringBuilder();
                }
                sb.Clear();
                sb.Append(iteratorType.Name);
                while ((iteratorType = iteratorType.DeclaringType) != null)
                {
                    sb.Insert(0, '/');
                    sb.Insert(0, iteratorType.Name);
                }
                name = sb.ToString();
            }

            ClassName = name;
            NamespaceName = type.Namespace ?? string.Empty;
            AssemblyName = type.Assembly.GetName().Name;
        }
        public bool Equals(TypeData other)
        {
            return ClassName == other.ClassName &&
                NamespaceName == other.NamespaceName &&
                AssemblyName == other.AssemblyName;
        }
        public override bool Equals(object obj) { return Equals((TypeData)obj); }
        public override int GetHashCode()
        {
            int hash1 = ClassName.GetHashCode();
            int hash2 = NamespaceName.GetHashCode();
            int hash3 = AssemblyName.GetHashCode();
            return hash1 ^ hash2 ^ hash3;
        }
        public override string ToString() { return $"{{{AssemblyName}, {NamespaceName}, {ClassName}}}"; }
    }
    internal static class TypeDataExtensions
    {
        public static Type ToType(this TypeData sefl)
        {
            return ToType(sefl.AssemblyName, sefl.NamespaceName, sefl.ClassName);
        }
        public static Type ToType(this TypeDataSerializable sefl)
        {
            return ToType(sefl.AssemblyName, sefl.NamespaceName, sefl.ClassName);
        }
        private static Type ToType(string AssemblyName, string NamespaceName, string ClassName)
        {
            Type result = null;
            if (string.IsNullOrEmpty(AssemblyName) == false)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(AssemblyName);
                }
                catch { }
                if (assembly == null)
                {
                    result = null;
                }
                else
                {
                    string fullTypeName = $"{NamespaceName}.{ClassName}";
                    result = assembly.GetType(fullTypeName);
                }
            }
            return result;
        }
    }
}
#endif