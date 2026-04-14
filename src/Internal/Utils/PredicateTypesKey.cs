using System;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal readonly struct PredicateTypesKey : IEquatable<PredicateTypesKey>
    {
        public readonly Type TargetType;
        public readonly Type[] AllowTypes;
        public readonly Type[] WithoutTypes;
        public PredicateTypesKey(Type signleType) : this(signleType, new Type[] { signleType } , Type.EmptyTypes) { }
        public PredicateTypesKey(Type targetType, Type[] types) : this(targetType, types, Type.EmptyTypes) { }
        public PredicateTypesKey(Type targetType, Type[] types, Type[] withoutTypes)
        {
            if(targetType == null)
            {
                Throw.ArgumentNullException();
            }
            TargetType = targetType;
            AllowTypes = types;
            WithoutTypes = withoutTypes;
        }
        public bool Check(Type type)
        {
            bool isAssignable = AllowTypes.Length == 0;
            foreach (Type allowType in AllowTypes)
            {
                if (allowType.IsAssignableFrom(type))
                {
                    isAssignable = true;
                    break;
                }
            }

            if (isAssignable)
            {
                foreach (Type withoutType in WithoutTypes)
                {
                    if (withoutType.IsAssignableFrom(type))
                    {
                        isAssignable = false;
                        break;
                    }
                }
            }

            return isAssignable && TargetType.IsAssignableFrom(type);
        }
        public bool Equals(PredicateTypesKey other)
        {
            if (AllowTypes.Length != other.AllowTypes.Length) { return false; }
            if (WithoutTypes.Length != other.WithoutTypes.Length) { return false; }

            if (TargetType != other.TargetType)
            {
                return false;
            }
            for (int i = 0; i < AllowTypes.Length; i++)
            {
                if (AllowTypes[i] != other.AllowTypes[i])
                {
                    return false;
                }
            }
            for (int i = 0; i < WithoutTypes.Length; i++)
            {
                if (WithoutTypes[i] != other.WithoutTypes[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return obj is PredicateTypesKey key && Equals(key);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(TargetType, AllowTypes, WithoutTypes);
        }
    }
}