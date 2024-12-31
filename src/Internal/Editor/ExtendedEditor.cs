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

        public bool AutoChechChanges = true;
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
        private static ComponentColorMode ComponentColorMode
        {
            get { return UserSettingsPrefs.instance.ComponentColorMode; }
            set { UserSettingsPrefs.instance.ComponentColorMode = value; }
        }

        protected bool IsMultipleTargets => targets.Length > 1;

        protected virtual bool IsStaticInit { get { return _isStaticInit; } }
        protected virtual bool IsInit { get { return _isInit; } }
        public void StaticInit()
        {
            if (IsStaticInit) { return; }
            _isStaticInit = true;
            OnStaticInit();
        }
        public void Init()
        {
            if (IsInit) { return; }
            _isInit = true;
            OnInit();
        }
        protected virtual void OnStaticInit() { }
        protected virtual void OnInit() { }

        public sealed override void OnInspectorGUI()
        {
            if (AutoChechChanges)
            {
                using (EcsGUI.CheckChanged(serializedObject))
                {
                    StaticInit();
                    Init();
                    DrawCustom();
                }
                return;
            }

            StaticInit();
            Init();
            DrawCustom();
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


        //Color proColor = (Color)new Color32(56, 56, 56, 255);
        //Color plebColor = (Color)new Color32(194, 194, 194, 255);
        //protected override void OnHeaderGUI()
        //{
        //    //base.OnHeaderGUI();
        //    var rect = EditorGUILayout.GetControlRect(false, 0f);
        //    rect.height = EditorGUIUtility.singleLineHeight;
        //    rect.y -= rect.height;
        //    rect.x = 48;
        //    rect.xMax -= rect.x * 2f;
        //
        //    //GUI.skin.settings
        //    EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? proColor : plebColor);
        //
        //    //string header = (target as ComponentFolder).folderName; // <--- your variable
        //    string header = "";
        //    if (string.IsNullOrEmpty(header))
        //    {
        //        header = target.ToString() + 1;
        //    }
        //
        //    EditorGUI.LabelField(rect, header, EditorStyles.boldLabel);
        //}
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
        private static ComponentColorMode ComponentColorMode
        {
            get { return UserSettingsPrefs.instance.ComponentColorMode; }
            set { UserSettingsPrefs.instance.ComponentColorMode = value; }
        }
        protected virtual bool IsStaticInit { get { return _isStaticInit; } }
        protected virtual bool IsInit { get { return _isInit; } }
        public void StaticInit()
        {
            if (IsStaticInit) { return; }
            _isStaticInit = true;
            OnStaticInit();
        }
        public void Init()
        {
            if (IsInit) { return; }
            _isInit = true;
            OnInit();
        }
        protected virtual void OnStaticInit() { }
        protected virtual void OnInit() { }

        //private Stopwatch _stopwatch = new Stopwatch();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //_stopwatch.Restart();
            using (EcsGUI.CheckChanged(property.serializedObject))
            {
                StaticInit();
                Init();
                DrawCustom(position, property, label);
            }
            //_stopwatch.Stop();
            //var result = _stopwatch.Elapsed;
            //UnityEngine.Debug.Log($"{result.Minutes}:{result.Seconds}:{result.Milliseconds}");
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