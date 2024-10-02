using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class ColorUtility
    {
        public static Color Desaturate(this Color self, float t)
        {
            float r = self.r;
            float g = self.g;
            float b = self.b;
            //float gray = r * 0.299f + g * 0.587f + b * 0.114f;
            float gray = r * 0.3333333f + g * 0.3333333f + b * 0.3333333f;
            r = r + (gray - r) * (1 - t);
            g = g + (gray - g) * (1 - t);
            b = b + (gray - b) * (1 - t);
            return new Color(r, g, b, self.a);
        }
        public static Color SetAlpha(this Color self, float a)
        {
            self.a = a;
            return self;
        }
    }
}