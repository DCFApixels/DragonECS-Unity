using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal readonly struct PredicateTypesKey : IEquatable<PredicateTypesKey>
    {
        public readonly Type[] Types;
        public readonly Type[] WithoutTypes;
        public PredicateTypesKey(Type[] types) : this(types, Type.EmptyTypes) { }
        public PredicateTypesKey(Type[] types, Type[] withoutTypes)
        {
            Types = types;
            WithoutTypes = withoutTypes;
        }
        public bool Check(Type type)
        {
            bool isAssignable = false;
            foreach (Type predicateTypes in Types)
            {
                if (predicateTypes.IsAssignableFrom(type))
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

            return isAssignable;
        }
        public bool Equals(PredicateTypesKey other)
        {
            if (Types.Length != other.Types.Length) { return false; }
            if (WithoutTypes.Length != other.WithoutTypes.Length) { return false; }
            for (int i = 0; i < Types.Length; i++)
            {
                if (Types[i] != other.Types[i])
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
            return HashCode.Combine(Types, WithoutTypes);
        }
        public static implicit operator PredicateTypesKey((Type[], Type[]) types) { return new PredicateTypesKey(types.Item1, types.Item2); }
    }
}