﻿#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    }
}
#endif
