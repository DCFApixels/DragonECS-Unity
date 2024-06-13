using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class MetaColorExstensions
    {
        public static Color ToUnityColor<T>(this T self) where T : IMetaColor
        {
            return new Color(self.R / 255f, self.G / 255f, self.B / 255f, self.A / 255f);
        }
        public static Color32 ToUnityColor32<T>(this T self) where T : IMetaColor
        {
            return new Color32(self.R, self.G, self.B, self.A);
        }

        public static MetaColor ToMetaColor(this Color self)
        {
            return new MetaColor((byte)(self.r * 255), (byte)(self.g * 255), (byte)(self.b * 255), (byte)(self.a * 255));
        }

        public static MetaColor ToMetaColor(this Color32 self)
        {
            return new MetaColor(self.r, self.g, self.b, self.a);
        }
    }
}
