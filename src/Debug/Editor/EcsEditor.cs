#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DCFApixels.DragonECS.Editors
{
    public static class EcsEditor
    {
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
                if (style == null)
                    style = CreateStyle(color32, colorCode);
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
            result.normal.background = CreateTexture(2, 2, componentColor);
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
