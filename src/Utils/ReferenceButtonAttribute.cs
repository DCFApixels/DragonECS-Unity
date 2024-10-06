using DCFApixels.DragonECS.Unity.Editors;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public sealed class ReferenceButtonAttribute : PropertyAttribute, IReferenceButtonAttribute
    {
        public readonly Type[] PredicateTypes;
        public readonly bool IsHideButtonIfNotNull;
        Type[] IReferenceButtonAttribute.PredicateTypes { get { return PredicateTypes; } }
        bool IReferenceButtonAttribute.IsHideButtonIfNotNull { get { return IsHideButtonIfNotNull; } }
        public ReferenceButtonAttribute(bool isHideButtonIfNotNull = false) : this(isHideButtonIfNotNull, Array.Empty<Type>()) { }
        public ReferenceButtonAttribute(params Type[] predicateTypes) : this(false, predicateTypes) { }
        public ReferenceButtonAttribute(bool isHideButtonIfNotNull, params Type[] predicateTypes)
        {
            IsHideButtonIfNotNull = isHideButtonIfNotNull;
            PredicateTypes = predicateTypes;
            Array.Sort(predicateTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
}