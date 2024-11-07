#if UNITY_EDITOR
using DCFApixels.DragonECS.Core;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(WorldQueriesMonitor))]
    internal class WorldQueriesMonitorEditor : ExtendedEditor<WorldQueriesMonitor>
    {
        protected override void DrawCustom()
        {
            var executors = Target.MaskQueryExecutors;

            EditorGUILayout.IntField("Count: ", executors.Count);
            GUILayout.Space(20);

            //using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                int i = 0;
                foreach (var executor in executors)
                {
                    DrawQueryInfo(executor, i++);
                }
            }
        }
        public static Color GetGenericPanelColor(int index)
        {
            return (index & 1) == 0 ? new Color(0, 0, 0, 0) : new Color(0.4f, 0.4f, 0.4f, 0.2f);
        }
        private void DrawQueryInfo(MaskQueryExecutor executor, int index)
        {
            //GUILayout.Space(10f);

            //using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(GetGenericPanelColor(index))))
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                var mask = executor.Mask;
                DrawConstraint("+", mask.Incs);
                DrawConstraint("-", mask.Excs);
            }

            EditorGUILayout.LongField("Version: ", executor.Version);
            EditorGUILayout.IntField("Entites Count: ", executor.LastCachedCount);

            //var rect = GUILayoutUtility.GetLastRect();
            //
            //rect.xMax = rect.xMin;
            //rect.xMin -= 2f;
            //
            //EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.8f));
        }
        private void DrawConstraint(string title, ReadOnlySpan<int> ids)
        {
            int i = 0;

            if (ids.Length <= 0)
            {
                return;
            }

            using (EcsGUI.Layout.BeginHorizontal())
            {
                using (EcsGUI.SetAlignment(value: TextAnchor.MiddleCenter))
                using (EcsGUI.SetFontStyle(value: FontStyle.Bold))
                using (EcsGUI.SetFontSize(value: 18))
                using (EcsGUI.SetColor(Color.white, 0.3f))
                    GUILayout.Label(title, GUILayout.Width(12));

                using (EcsGUI.Layout.BeginVertical())
                {
                    foreach (var inc in ids)
                    {
                        Type type = Target.World.GetComponentType(inc);
                        TypeMeta meta = type.ToMeta();

                        Color color = EcsGUI.SelectPanelColor(meta, i, 9);

                        using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(color, 0.2f)))
                        {
                            GUILayout.Label(meta.TypeName);
                        }
                        i++;
                    }
                }
            }

            GUILayout.Space(6);
        }
    }
}
#endif