#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class ReflectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAttribute<T>(this MemberInfo self, out T attrbiute) where T : Attribute
        {
            attrbiute = self.GetCustomAttribute<T>();
            return attrbiute != null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
        {
            return self.GetCustomAttribute<T>() != null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsConcreteType(this Type self)
        {
            if (self.IsGenericType || self.IsAbstract || self.IsInterface)
            {
                return false;
            }
            return self.IsValueType || self.IsClass;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnityObject(this Type self)
        {
            return self.IsSubclassOf(typeof(UnityObject));
        }
        public static bool HasUnitySerializableFields(this Type self)
        {
            var fields = self.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fields.Length <= 0)
            {
                return false;
            }
            foreach (var field in fields)
            {
                if (field.IsUnitySerializableField() && 
                    (field.FieldType.IsUnitySerializableLeafType() ||
                    HasUnitySerializableFields(field.FieldType)))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsUnitySerializableField(this FieldInfo self)
        {
            if ((self.IsPublic || self.HasAttribute<SerializeField>()) && self.FieldType.IsUnitySerializableType())
            {
                return true;
            }
            {
                if (self.HasAttribute<SerializeReference>())
                {
                    return true;
                }
                return false;
            }
        }
        public static bool IsUnitySerializableLeafType(this Type self)
        {
            if (self.IsPrimitive)
            {
                return true;
            }
            if (self.IsEnum)
            {
                return true;
            }
            if (self == typeof(string))
            {
                return true;
            }
            return false;
        }
        public static bool IsUnitySerializableType(this Type self)
        {
            if (self.IsAbstract)
            {
                return false;
            }
            if (self.IsUnitySerializableLeafType())
            {
                return true;
            }
            return self.HasAttribute<SerializableAttribute>();
        }

        private static MethodInfo _memberwiseCloneMethdo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static object Clone_Reflection(this object obj)
        {
            if (obj is ICloneable cloneable)
            {
                return cloneable.Clone();
            }
            return _memberwiseCloneMethdo.Invoke(obj, null);
        }
    }

    internal partial class UnityReflectionCache
    {
        private static readonly Dictionary<Type, UnityReflectionCache> _cache = new Dictionary<Type, UnityReflectionCache>();
        public static bool InitLocal(Type type, ref UnityReflectionCache localCache)
        {
            if (localCache != null && localCache.Type == type) { return false; }
            localCache = Get(type);
            return true;
        }
        public static UnityReflectionCache Get(Type type)
        {
            if(_cache.TryGetValue(type, out var result) == false)
            {
                result = new UnityReflectionCache(type);
                _cache.Add(type, result);
            }
            return result;
        }

        public readonly Type Type;
        public UnityReflectionCache(Type type)
        {
            Type = type;
        }
    }
}
#endif
