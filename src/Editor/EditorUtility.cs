#if UNITY_EDITOR
using Codice.Utils;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class EcsUnityEditorUtility
    {
        public static string TransformFieldName(string name)
        {
            if (name.Length <= 0)
            {
                return name;
            }
            StringBuilder b = new StringBuilder();
            bool nextWorld = true;
            bool prewIsUpper = false;


            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetter(c) == false)
                {
                    nextWorld = true;
                    prewIsUpper = false;
                    continue;
                }

                bool isUpper = char.IsUpper(c);
                if (isUpper)
                {
                    if (nextWorld == false && prewIsUpper == false)
                    {
                        b.Append(' ');
                        nextWorld = true;
                    }
                }

                if (nextWorld)
                {
                    b.Append(char.ToUpper(c));
                }
                else
                {
                    b.Append(c);
                }
                nextWorld = false;
                prewIsUpper = isUpper;
            }

            return b.ToString();
        }
    }


    internal static class EcsGUI
    {
        internal readonly static Color GrayColor = new Color32(100, 100, 100, 255);
        internal readonly static Color GreenColor = new Color32(75, 255, 0, 255);
        internal readonly static Color RedColor = new Color32(255, 0, 75, 255);

        //private static GUILayoutOption[] _defaultParams;
        //private static bool _isInit = false;
        //private static void Init()
        //{
        //    if (_isInit)
        //    {
        //        return;
        //    }
        //    _defaultParams = new GUILayoutOption[] { GUILayout.ExpandWidth(true) };
        //    _isInit = true;
        //}

        public static class Layout
        {
            private static bool IsShowHidden
            {
                get { return DebugMonitorPrefs.instance.IsShowHidden; }
                set { DebugMonitorPrefs.instance.IsShowHidden = value; }
            }
            private static bool IsShowRuntimeComponents
            {
                get { return DebugMonitorPrefs.instance.IsShowRuntimeComponents; }
                set { DebugMonitorPrefs.instance.IsShowRuntimeComponents = value; }
            }
            public static void DrawRuntimeComponents(entlong entity)
            {
                if (entity.TryUnpack(out int entityID, out EcsWorld world))
                {
                    DrawRuntimeComponents(entityID, world);
                }
            }
            public static void DrawRuntimeComponents(int entityID, EcsWorld world)
            {
                var componentTypeIDs = world.GetComponentTypeIDs(entityID);

                GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black, 0.2f));

                IsShowRuntimeComponents = EditorGUILayout.Foldout(IsShowRuntimeComponents, "RUNTIME COMPONENTS");
                if (IsShowRuntimeComponents)
                {
                    GUILayout.Box("", EcsEditor.GetStyle(GUI.color, 0.16f), GUILayout.ExpandWidth(true));
                    IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);
                    foreach (var componentTypeID in componentTypeIDs)
                    {
                        var pool = world.GetPool(componentTypeID);
                        {
                            DrawRuntimeComponent(entityID, pool);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            private static void DrawRuntimeComponent(int entityID, IEcsPool pool)
            {
                var meta = pool.ComponentType.ToMeta();
                if (meta.IsHidden == false || IsShowHidden)
                {
                    object data = pool.GetRaw(entityID);
                    Color panelColor = meta.Color.ToUnityColor().Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);
                    GUILayout.BeginVertical(EcsEditor.GetStyle(panelColor, EscEditorConsts.COMPONENT_DRAWER_ALPHA));
                    EditorGUI.BeginChangeCheck();

                    Type componentType = pool.ComponentType;
                    ExpandMatrix expandMatrix = ExpandMatrix.Take(componentType);
                    bool changed = DrawRuntimeData(componentType, new GUIContent(meta.Name), expandMatrix, data, out object resultData);
                    if (changed)
                    {
                        pool.SetRaw(entityID, resultData);
                    }

                    GUILayout.EndVertical();
                }
            }

            private static bool DrawRuntimeData(Type fieldType, GUIContent label, ExpandMatrix expandMatrix, object data, out object outData)
            {
                Type type = data.GetType();
                bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(fieldType);
                ref bool isExpanded = ref expandMatrix.Down();
                bool changed = false;
                outData = data;

                if (isUnityObject == false && (type.IsGenericType || !type.IsSerializable))
                {
                    isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, label, EditorStyles.foldout);
                    EditorGUILayout.EndFoldoutHeaderGroup();

                    if (isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var field in type.GetFields(fieldFlags))
                        {
                            GUIContent subLabel = new GUIContent(EcsUnityEditorUtility.TransformFieldName(field.Name));
                            if (DrawRuntimeData(field.FieldType, subLabel, expandMatrix, field.GetValue(data), out object fieldData))
                            {
                                field.SetValue(data, fieldData);

                                outData = fieldData;
                                changed = true;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (isUnityObject)
                    {
                        EditorGUI.BeginChangeCheck();
                        UnityEngine.Object uobj = (UnityEngine.Object)data;
                        uobj = EditorGUILayout.ObjectField(label, uobj, fieldType, true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            outData = uobj;
                            changed = true;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        WrapperBase w = RefEditorWrapper.Take(data);

                        w.IsExpanded = isExpanded;
                        EditorGUILayout.PropertyField(w.Property, label, true);
                        isExpanded = w.IsExpanded;

                        if (EditorGUI.EndChangeCheck())
                        {
                            w.SO.ApplyModifiedProperties();
                            outData = w.Data;
                            changed = true;
                        }
                    }
                }
                
                expandMatrix.Up();
                return changed;
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
