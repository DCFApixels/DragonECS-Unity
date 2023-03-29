using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class DebugColorAttributeExt
    {
        public static Color GetUnityColor(this DebugColorAttribute self)
        {
            return new Color(self.rn, self.gn, self.bn);
        }
        public static Color32 GetUnityColor32(this DebugColorAttribute self)
        {
            return new Color32(self.r, self.g, self.b, 255);
        }
    }
}
