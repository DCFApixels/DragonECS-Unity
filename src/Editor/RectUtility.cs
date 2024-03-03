using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class RectUtility
    {
        public static (Rect, Rect) HorizontalSliceLerp(Rect rect, float t)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax -= rect.width * (1f - t);
            r.xMin += rect.width * t;
            return (l, r);
        }
        public static (Rect, Rect) HorizontalSliceLeft(Rect rect, float with)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax = l.xMin + with;
            r.xMin += with;
            return (l, r);
        }
        public static (Rect, Rect) HorizontalSliceRight(Rect rect, float with)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax -= with;
            r.xMin = r.xMax - with;
            return (l, r);
        }

        public static (Rect, Rect) VerticalSliceBottom(Rect rect, float height)
        {
            Rect t = rect;
            Rect b = rect;
            t.yMax -= height;
            b.yMin = b.yMax - height;
            return (t, b);
        }
    }
}
