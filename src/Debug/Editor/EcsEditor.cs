#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public static class EcsEditor
    {
        public static GUIStyle GetStyle(Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            Color componentColor = color;
            componentColor.a = 0.15f;
            style.normal.background = CreateTexture(2, 2, componentColor);

            return style;
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
    }
}
#endif
