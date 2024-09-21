namespace DCFApixels.DragonECS.Unity.Internal
{
    using System;

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    internal sealed class ArrayElementAttribute : Attribute { }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using global::Unity.Collections.LowLevel.Unsafe;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityObject = UnityEngine.Object;


    internal abstract class ExtendedEditor : Editor
    {
        private bool _isStaticInit = false;
        private bool _isInit = false;

        protected float OneLineHeight
        {
            get => EditorGUIUtility.singleLineHeight;
        }
        protected float Spacing
        {
            get => EditorGUIUtility.standardVerticalSpacing;
        }
        protected bool IsShowInterfaces
        {
            get { return UserSettingsPrefs.instance.IsShowInterfaces; }
            set { UserSettingsPrefs.instance.IsShowInterfaces = value; }
        }
        protected bool IsShowHidden
        {
            get { return UserSettingsPrefs.instance.IsShowHidden; }
            set { UserSettingsPrefs.instance.IsShowHidden = value; }
        }
        protected bool IsMultipleTargets => targets.Length > 1;

        protected virtual bool IsStaticInit { get { return _isStaticInit; } }
        protected virtual bool IsInit { get { return _isInit; } }
        protected void StaticInit()
        {
            if (IsStaticInit) { return; }
            _isStaticInit = true;
            OnStaticInit();
        }
        protected void Init()
        {
            if (IsInit) { return; }
            _isInit = true;
            OnInit();
        }
        protected virtual void OnStaticInit() { }
        protected virtual void OnInit() { }

        public sealed override void OnInspectorGUI()
        {
            using (EcsGUI.CheckChanged(serializedObject))
            {
                StaticInit();
                Init();
                DrawCustom();
            }
        }

        protected abstract void DrawCustom();
        protected void DrawDefault()
        {
            base.OnInspectorGUI();
        }


        protected SerializedProperty FindProperty(string name)
        {
            return serializedObject.FindProperty(name);
        }
    }
    internal abstract class ExtendedEditor<T> : ExtendedEditor
    {
        public T Target
        {
            get
            {
                var obj = target;
                return UnsafeUtility.As<UnityObject, T>(ref obj);
            }
        }
        public T[] Targets
        {
            get
            {
                var obj = targets;
                return UnsafeUtility.As<UnityObject[], T[]>(ref obj);
            }
        }
    }
    internal abstract class ExtendedPropertyDrawer : PropertyDrawer
    {
        private bool _isStaticInit = false;
        private bool _isInit = false;

        private IEnumerable<Attribute> _attributes = null;

        private bool? _isArrayElement = null;
        protected bool IsArrayElement
        {
            get
            {
                if (_isArrayElement == null)
                {
                    _isArrayElement = Attributes.Any(o => o is ArrayElementAttribute);
                }
                return _isArrayElement.Value;
            }
        }

        protected IEnumerable<Attribute> Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    _attributes = fieldInfo.GetCustomAttributes();
                }
                return _attributes;
            }
        }
        protected float OneLineHeight
        {
            get => EditorGUIUtility.singleLineHeight;
        }
        protected float Spacing
        {
            get => EditorGUIUtility.standardVerticalSpacing;
        }
        protected bool IsShowInterfaces
        {
            get { return UserSettingsPrefs.instance.IsShowInterfaces; }
            set { UserSettingsPrefs.instance.IsShowInterfaces = value; }
        }
        protected bool IsShowHidden
        {
            get { return UserSettingsPrefs.instance.IsShowHidden; }
            set { UserSettingsPrefs.instance.IsShowHidden = value; }
        }
        protected virtual bool IsStaticInit { get { return _isStaticInit; } }
        protected virtual bool IsInit { get { return _isInit; } }
        protected void StaticInit()
        {
            if (IsStaticInit) { return; }
            _isStaticInit = true;
            OnStaticInit();
        }
        protected void Init()
        {
            if (IsInit) { return; }
            _isInit = true;
            OnInit();
        }
        protected virtual void OnStaticInit() { }
        protected virtual void OnInit() { }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (EcsGUI.CheckChanged(property.serializedObject))
            {
                StaticInit();
                Init();
                DrawCustom(position, property, label);
            }
        }
        protected abstract void DrawCustom(Rect position, SerializedProperty property, GUIContent label);
    }
    internal abstract class ExtendedPropertyDrawer<TAttribute> : ExtendedPropertyDrawer
    {
        protected TAttribute Attribute
        {
            get
            {
                var obj = attribute;
                return UnsafeUtility.As<PropertyAttribute, TAttribute>(ref obj);
            }
        }
    }
}
#endif