#if UNITY_EDITOR
using DCFApixels.DragonECS.Core;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(WorldQueriesMonitor))]
    internal class WorldQueriesMonitorEditor : ExtendedEditor<WorldQueriesMonitor>
    {
        private GUIStyle _headerStyle;

        private void CopyToClipboard()
        {
            const char SEPARATOR = '\t';
            var allqueries = Target.MaskQueryExecutors;
            var allpools = Target.World.AllPools.Slice(0, Target.World.PoolsCount);

            StringBuilder sb = new StringBuilder();
            int i = -1;

            //numbers
            sb.Append($"{SEPARATOR}{SEPARATOR}№");
            i = -1;
            foreach (var pool in allpools)
            {
                i++;
                sb.Append($"{SEPARATOR}{i}");
            }
            sb.Append("\r\n");
            //numbers end

            //chunks
            sb.Append($"{SEPARATOR}{SEPARATOR}Chunks");
            i = -1;
            foreach (var pool in allpools)
            {
                i++;
                sb.Append($"{SEPARATOR}{i >> 5}");
            }
            sb.Append("\r\n");
            //chunks end


            //header
            sb.Append($"№{SEPARATOR}Version{SEPARATOR}Count");

            //pools
            foreach (var pool in allpools)
            {
                sb.Append($"{SEPARATOR}");
                if (pool.IsNullOrDummy() == false)
                {
                    sb.Append(pool.ComponentType.ToMeta().TypeName);
                }
                else
                {
                    sb.Append("NULL");
                }
            }
            sb.Append("\r\n");
            //header end


            //content
            i = -1;
            foreach (var query in allqueries)
            {
                i++;

                sb.Append($"{i}{SEPARATOR}{query.Version}{SEPARATOR}{query.LastCachedCount}");

                var incs = query.Mask.Incs;
                var excs = query.Mask.Excs;
                var incsI = 0;
                var excsI = 0;
                for (int j = 0; j < allpools.Length; j++)
                {
                    var pool = allpools[j];

                    sb.Append($"{SEPARATOR}");
                    if (pool.IsNullOrDummy() == false)
                    {
                        if (incsI < incs.Length && incs[incsI] == j)
                        {
                            sb.Append($"+");
                            incsI++;
                            continue;
                        }

                        if (excsI < excs.Length && excs[excsI] == j)
                        {
                            sb.Append($"-");
                            excsI++;
                            continue;
                        }
                    }
                }
                sb.Append("\r\n");
            }

            //end

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        protected override void DrawCustom()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.fontSize = 28;
            }
            var executors = Target.MaskQueryExecutors;

            using (EcsGUI.Layout.BeginHorizontal())
            {
                GUILayout.Label("[Queries]", _headerStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Copy to Clipboard", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                {
                    CopyToClipboard();
                }
            }

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