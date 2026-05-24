#if DISABLE_DEBUG
#undef DEBUG
#endif
using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Attributes
{
    public sealed class ReferenceDropDownAttribute : PropertyAttribute
    {
        public Type[] AllowTypes;
        public bool HideButtonIfNotNull;
        public ReferenceDropDownAttribute(bool hideButtonIfNotNull = false) : this(hideButtonIfNotNull, Array.Empty<Type>()) { }
        public ReferenceDropDownAttribute(params Type[] allowedTypes) : this(false, allowedTypes) { }
        public ReferenceDropDownAttribute(bool hideButtonIfNotNull, params Type[] allowedTypes)
        {
            HideButtonIfNotNull = hideButtonIfNotNull;
            AllowTypes = allowedTypes;
            Array.Sort(allowedTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
    public sealed class ReferenceDropDownExcludeAttribute : Attribute
    {
        public Type[] ExcludedTypes;
		[Obsolete("Empty constructor makes no sense. Specify types to exclude.", true)]
		public ReferenceDropDownExcludeAttribute() : this(Array.Empty<Type>()) { }
        public ReferenceDropDownExcludeAttribute(params Type[] excludedTypes)
        {
            ExcludedTypes = excludedTypes;
            Array.Sort(excludedTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
    public sealed class DragonMetaBlockAttribute : PropertyAttribute { }
    public sealed class InlineInspectorAttribute : PropertyAttribute
	{
		public bool IsReadOnly = true;
		public InlineInspectorAttribute(bool isReadOnly = false)
		{
			IsReadOnly = isReadOnly;
		}
	}
}