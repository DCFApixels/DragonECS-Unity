#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class WrapperBase<TSelf> : ScriptableObject
        where TSelf : WrapperBase<TSelf>
    {
        private SerializedObject _so;
        private SerializedProperty _property;

        private bool _isReleased = false;
        private static Stack<TSelf> _wrappers = new Stack<TSelf>();

        public SerializedObject SO
        {
            get { return _so; }
        }
        public SerializedProperty Property
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
    }
    internal class RefEditorWrapper : WrapperBase<RefEditorWrapper>
    {
        [SerializeReference]
        public object data;
    }
    internal class UnityObjEditorWrapper : WrapperBase<UnityObjEditorWrapper>
    {
        [SerializeField]
        public UnityEngine.Object data;
    }

    public static class EcsGUI
    {
        private static Color _grayColor = new Color32(100, 100, 100, 100);
        private static Color _greenColor = new Color32(75, 255, 0, 100);
        private static Color _redColor = new Color32(255, 0, 75, 100);

        private static GUIStyle _grayStyle;
        private static GUIStyle _greenStyle;
        private static GUIStyle _redStyle;
        private static GUILayoutOption[] _defaultParams;

        private static bool _isInit = false;

        private static void Init()
        {
            if (_isInit)
            {
                return;
            }

            _defaultParams = new GUILayoutOption[] { GUILayout.ExpandWidth(true) };
            _grayStyle = EcsEditor.GetStyle(_grayColor);
            _greenStyle = EcsEditor.GetStyle(_greenColor);
            _redStyle = EcsEditor.GetStyle(_redColor);
            _isInit = true;
        }


        private const string CONNECTED = "Connected";
        private const string NOT_CONNECTED = "Not connected";
        private const string UNDETERMINED_CONNECTED = "---";
        public static void DrawConnectStatus(Rect position, bool status)
        {
            Init();
            if (status)
            {
                GUI.Box(position, CONNECTED, _greenStyle);
            }
            else
            {
                GUI.Box(position, NOT_CONNECTED, _redStyle);
            }
        }

        public static void DrawUndeterminedConnectStatus(Rect position)
        {
            Init();
            GUI.Box(position, UNDETERMINED_CONNECTED, _grayStyle);
        }

        public static class Layout
        {
            public static void DrawConnectStatus(bool status, params GUILayoutOption[] options)
            {
                Init();
                if (options == null || options.Length <= 0)
                {
                    options = _defaultParams;
                }
                GUILayout.Box("", options);
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (status)
                {
                    Color color = _greenColor;
                    color.a = 0.6f;
                    EditorGUI.DrawRect(lastRect, color);
                    GUI.Box(lastRect, CONNECTED);
                }
                else
                {
                    Color color = _redColor;
                    color.a = 0.6f;
                    EditorGUI.DrawRect(lastRect, color);
                    GUI.Box(lastRect, NOT_CONNECTED);
                }
            }
            public static void DrawUndeterminedConnectStatus(params GUILayoutOption[] options)
            {
                Init();
                if (options == null || options.Length <= 0)
                {
                    options = _defaultParams;
                }
                GUILayout.Box(UNDETERMINED_CONNECTED, _grayStyle, options);
            }
            public static void DrawComponents(entlong entity)
            {
                if (entity.TryUnpack(out int entityID, out EcsWorld world))
                {
                    DrawComponents(entityID, world);
                }
            }
            public static void DrawComponents(int entityID, EcsWorld world)
            {
                var componentTypeIDs = world.GetComponentTypeIDs(entityID);

                foreach (var componentTypeID in componentTypeIDs)
                {
                    var pool = world.GetPool(componentTypeID);
                    {
                        DrawComponent(entityID, world, pool);
                    }
                }
            }
            private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            private static void DrawComponent(int entityID, EcsWorld world, IEcsPool pool)
            {
                object data = pool.GetRaw(entityID);
                var meta = data.GetMeta();

                Color panelColor = meta.Color.ToUnityColor();
                GUILayout.BeginVertical(EcsEditor.GetStyle(panelColor, 0.22f));
                EditorGUI.BeginChangeCheck();

                bool changed = DrawData(pool.ComponentType, data, out object resultData);

                if (changed)
                {
                    pool.SetRaw(entityID, resultData);
                }

                GUILayout.EndVertical();

                Rect lineRect = GUILayoutUtility.GetLastRect();
                lineRect.y = lineRect.yMax;
                lineRect.height = 3f;
                Color rectColor = panelColor;
                rectColor.a = 0.34f;
                EditorGUI.DrawRect(lineRect, rectColor);

                GUILayout.Space(2f);
            }

            private static bool DrawData(Type fieldType, object data, out object outData)
            {
                var meta = data.GetMeta();
                GUIContent label = new GUIContent(meta.Name);

                Type type = data.GetType();
                var uobj = data as UnityEngine.Object;
                if (uobj == false && type.IsGenericType)
                {
                    bool result = false;
                    foreach (var field in type.GetFields(fieldFlags))
                    {
                        if (DrawData(field.FieldType, field.GetValue(data), out object fieldData))
                        {
                            field.SetValue(data, fieldData);
                            result = true;
                        }
                    }
                    outData = data;
                    return result;
                }
                else
                {
                    if (uobj == null)
                    {
                        EditorGUI.BeginChangeCheck();

                        var w = RefEditorWrapper.Take();
                        w.data = data;
                        w.SO.Update();

                        EditorGUILayout.PropertyField(w.Property, true);
                        RefEditorWrapper.Release(w);

                        if (EditorGUI.EndChangeCheck())
                        {
                            w.SO.ApplyModifiedProperties();
                            outData = w.data;
                            return true;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();

                        var w = UnityObjEditorWrapper.Take();
                        w.data = uobj;
                        w.SO.Update();

                        EditorGUILayout.PropertyField(w.Property, true);
                        UnityObjEditorWrapper.Release(w);

                        if (EditorGUI.EndChangeCheck())
                        {
                            w.SO.ApplyModifiedProperties();
                            outData = uobj;
                            return true;
                        }
                    }

                    outData = data;
                    return false;
                }
            }
        }
    }


    [InitializeOnLoad]
    public static class EcsEditor
    {
        static EcsEditor()
        {
            colorBoxeStyles = new SparseArray<GUIStyle>();
        }
        private static SparseArray<GUIStyle> colorBoxeStyles = new SparseArray<GUIStyle>();
        public static GUIStyle GetStyle(Color color, float alphaMultiplier)
        {
            color.a *= alphaMultiplier;
            return GetStyle(color);
        }
        public static GUIStyle GetStyle(Color32 color32)
        {
            int colorCode = new Color32Union(color32).colorCode;
            if (colorBoxeStyles.TryGetValue(colorCode, out GUIStyle style))
            {
                if (style == null || style.normal.background == null)
                {
                    style = CreateStyle(color32, colorCode);
                    colorBoxeStyles[colorCode] = style;
                }
                return style;
            }

            style = CreateStyle(color32, colorCode);
            colorBoxeStyles.Add(colorCode, style);
            return style;
        }
        private static GUIStyle CreateStyle(Color32 color32, int colorCode)
        {
            GUIStyle result = new GUIStyle(GUI.skin.box);
            Color componentColor = color32;
            Texture2D texture2D = CreateTexture(2, 2, componentColor);
            result.hover.background = texture2D;
            result.focused.background = texture2D;
            result.active.background = texture2D;
            result.normal.background = texture2D;
            return result;
        }
        private static Texture2D CreateTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; ++i)
                pixels[i] = color;

            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }

        #region Utils
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        private readonly ref struct Color32Union
        {
            [FieldOffset(0)]
            public readonly int colorCode;
            [FieldOffset(0)]
            public readonly byte r;
            [FieldOffset(1)]
            public readonly byte g;
            [FieldOffset(2)]
            public readonly byte b;
            [FieldOffset(3)]
            public readonly byte a;
            public Color32Union(byte r, byte g, byte b, byte a) : this()
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
            public Color32Union(Color32 color) : this()
            {
                r = color.r;
                g = color.g;
                b = color.b;
                a = color.a;
            }
        }
        #endregion
    }

    public static class ReflectionExtensions
    {
        public static bool TryGetAttribute<T>(this MemberInfo self, out T attrbiute) where T : Attribute
        {
            attrbiute = self.GetCustomAttribute<T>();
            return attrbiute != null;
        }
        public static bool HasAttribute<T>(this MemberInfo self) where T : Attribute
        {
            return self.GetCustomAttribute<T>() != null;
        }
    }
}
#endif
