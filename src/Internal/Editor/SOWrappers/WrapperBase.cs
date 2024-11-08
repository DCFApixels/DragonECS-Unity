#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [Serializable]
    internal abstract class WrapperBase : ScriptableObject
    {
        public abstract object Data { get; }
        public abstract bool IsExpanded { get; set; }
        public abstract SerializedObject SO { get; }
        public abstract SerializedProperty Property { get; }
        public abstract void Release();
    }
    [Serializable]
    internal abstract class WrapperBase<TSelf> : WrapperBase
        where TSelf : WrapperBase<TSelf>
    {

        private SerializedObject _so;
        private SerializedProperty _property;

        private bool _isDestroyed = false;
        private bool _isReleased = false;

        private static Stack<TSelf> _wrappers = new Stack<TSelf>();

        public sealed override bool IsExpanded
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Property.isExpanded; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Property.isExpanded = value; }
        }
        public sealed override SerializedObject SO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _so; }
        }
        public sealed override SerializedProperty Property
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _property; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSelf Take()
        {
            TSelf result;
            if (_wrappers.Count <= 0)
            {
                result = CreateInstance<TSelf>();
                result._so = new SerializedObject(result);
                result._property = result._so.FindProperty("data");
            }
            else
            {
                result = _wrappers.Pop();
                if (result._isDestroyed)
                {
                    result = Take();
                }
            }
            result._isReleased = false;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release(TSelf wrapper)
        {
            if (wrapper._isReleased)
            {
                throw new Exception();
            }
            wrapper._isReleased = true;
            _wrappers.Push(wrapper);
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Release()
        {
            Release((TSelf)this);
        }
    }
}
#endif
