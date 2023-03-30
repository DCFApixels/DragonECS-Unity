#if UNITY_EDITOR
using UnityEngine;

namespace DCFApixels.DragonECS.Editors
{
    public static class EcsEditor
    {
        public static GUIStyle GetStyle(Color color, float alphaMultiplier)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            Color componentColor = color;
            componentColor.a *= alphaMultiplier;
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
