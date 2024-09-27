using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class RectUtility
    {
        public static (Rect, Rect) HorizontalSliceLerp(in this Rect rect, float t)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax -= rect.width * (1f - t);
            r.xMin += rect.width * t;
            return (l, r);
        }
        public static (Rect, Rect) HorizontalSliceLeft(in this Rect rect, float with)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax = l.xMin + with;
            r.xMin += with;
            return (l, r);
        }
        public static (Rect, Rect) HorizontalSliceRight(in this Rect rect, float with)
        {
            Rect l = rect;
            Rect r = rect;
            l.xMax -= with;
            r.xMin = r.xMax - with;
            return (l, r);
        }

        public static (Rect, Rect) VerticalSliceTop(in this Rect rect, float height)
        {
            Rect t = rect;
            Rect b = rect;
            t.yMax = t.yMin + height;
            b.yMin += height;
            return (t, b);
        }
        public static (Rect, Rect) VerticalSliceBottom(in this Rect rect, float height)
        {
            Rect t = rect;
            Rect b = rect;
            t.yMax -= height;
            b.yMin = b.yMax - height;
            return (t, b);
        }

        #region DebugRect
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float AsFloat(uint value) => *(float*)&value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Q32ToFloat(uint value) => AsFloat((value >> 9) | 0x3F80_0000) - 1f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint NextXorShiftState(uint state)
        {
            unchecked
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return state;
            };
        }
        public static void DebugRect_Editor(params Rect[] rects)
        {
#if UNITY_EDITOR
            uint colorState = NextXorShiftState(3136587146);
            foreach (var rect in rects)
            {
                colorState = NextXorShiftState(colorState);
                Color color = Color.HSVToRGB(Q32ToFloat(colorState), 1, 1);
                color.a = 0.3f;
                GUI.Box(rect, "", EditorStyles.selectionRect);
                EditorGUI.DrawRect(rect, color);
            }
#endif
        }
        #endregion

        public static Rect AddPadding(in this Rect rect, float verticalHorizontal)
        {
            return AddPadding(rect, verticalHorizontal, verticalHorizontal, verticalHorizontal, verticalHorizontal);
        }
        public static Rect AddPadding(in this Rect rect, float horizontal, float vertical)
        {
            return AddPadding(rect, horizontal, horizontal, vertical, vertical);
        }
        public static Rect AddPadding(in this Rect rect, float left, float right, float top, float bottom)
        {
            Rect result = rect;
            result.xMin += left;
            result.xMax -= right;
            result.yMin += top;
            result.yMax -= bottom;
            return result;
        }
        public static Rect Move(in this Rect rect, Vector2 addVector)
        {
            Rect result = rect;
            result.center += addVector;
            return result;
        }
        public static Rect MoveTo(in this Rect rect, Vector2 center)
        {
            Rect result = rect;
            result.center = center;
            return result;
        }
        public static Rect Move(in this Rect rect, float addX, float addY)
        {
            return Move(rect, new Vector2(addX, addY));
        }
    }
}
