#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [Serializable]
    internal abstract class WrapperBase : ScriptableObject
    {
        public abstract object Data { get; }
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

        private bool _isReleased = false;
        private static Stack<TSelf> _wrappers = new Stack<TSelf>();
        public override SerializedObject SO
        {
            get { return _so; }
        }
        public override SerializedProperty Property
        {
            get { return _property; }
        }

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
            }
            return result;
        }
        public static void Release(TSelf wrapper)
        {
            if (wrapper._isReleased)
            {
                return;
            }
            wrapper._isReleased = true;
            _wrappers.Push(wrapper);
        }

        public override void Release()
        {
            Release((TSelf)this);
        }
    }

    [Serializable]
    public class EmptyDummy
    {
        public static readonly EmptyDummy Instance = new EmptyDummy();
        private EmptyDummy() { }
    }
}
#endif
