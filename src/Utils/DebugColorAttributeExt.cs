using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class DebugColorAttributeExt
    {
        public static Color GetUnityColor(this DebugColorAttribute self)
        {
            return self.color.ToUnityColor();
        }
        public static Color32 GetUnityColor32(this DebugColorAttribute self)
        {
            return self.color.ToUnityColor32();
        }

        public static Color ToUnityColor(this DebugColor self)
        {
            return new Color(self.r / 255f, self.g / 255f, self.b / 255f, self.a / 255f);
        }
        public static Color32 ToUnityColor32(this DebugColor self)
        {
            return new Color32(self.r, self.g, self.b, self.a);
        }
    }
}
