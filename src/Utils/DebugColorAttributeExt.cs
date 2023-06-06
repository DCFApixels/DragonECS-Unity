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
    }
}
