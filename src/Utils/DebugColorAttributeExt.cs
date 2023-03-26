using UnityEngine;

namespace DCFApixels.DragonECS.Unity
{
    public static class DebugColorAttributeExt
    {
        public static Color GetUnityColor(this DebugColorAttribute self)
        {
            return new Color(self.r / 255f, self.g / 255f, self.b / 255f);
        }
    }
}
