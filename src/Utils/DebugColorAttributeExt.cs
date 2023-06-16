using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class DebugColorAttributeExt
    {
        public static Color GetUnityColor(this DebugColorAttribute self)
        {
            return new Color(self.r / 255f, self.g / 255f, self.b / 255f);
        }
        public static Color32 GetUnityColor32(this DebugColorAttribute self)
        {
            return new Color32(self.r, self.g, self.b, 255);
        }

        public static Color ToUnityColor(this (byte, byte, byte) self)
        {
            return new Color(self.Item1 / 255f, self.Item2 / 255f, self.Item3 / 255f);
        }
        public static Color32 ToUnityColor32(this (byte, byte, byte) self)
        {
            return new Color32(self.Item1, self.Item2, self.Item3, 255);
        }
    }
}
