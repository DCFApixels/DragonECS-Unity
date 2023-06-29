#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Editors
{
    public static class EcsGUI
    {
        private static GUIStyle _greenStyle;
        private static GUIStyle _redStyle;

        private static bool _isInit = false;
        private static void Init()
        {
            if (_isInit)
                return;

            _greenStyle = EcsEditor.GetStyle(new Color32(75, 255, 0, 100));
            _redStyle = EcsEditor.GetStyle(new Color32(255, 0, 75, 100));
            _isInit = true;
        }

        public static void DrawConnectStatus(Rect position, bool status)
        {
            Init();
            if (status)
                GUI.Box(position, "Connected", _greenStyle);
            else
                GUI.Box(position, "Not connected", _redStyle);
        }


        public static class Layout
        {
            public static void DrawConnectStatus(bool status, params GUILayoutOption[] options)
            {
                Init();
                if (status)
                    GUILayout.Box("Connected", _greenStyle, GUILayout.ExpandWidth(true));
                else
                    GUILayout.Box("Not connected", _redStyle, GUILayout.ExpandWidth(true));
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

        public static string GetGenericName(Type type) => EcsDebugUtility.GetGenericTypeName(type);

        public static string GetName<T>() => GetName(typeof(T));
        public static string GetName(Type type) => EcsDebugUtility.GetName(type);

        public static string GetDescription<T>() => GetDescription(typeof(T));
        public static string GetDescription(Type type) => EcsDebugUtility.GetDescription(type);

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
}
#endif
