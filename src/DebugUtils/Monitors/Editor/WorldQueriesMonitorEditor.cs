#if UNITY_EDITOR
using DCFApixels.DragonECS.Core;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
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
                    sb.Append(pool.ComponentType.GetMeta().TypeName);
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
                var anys = query.Mask.Anys;
                var incsI = 0;
                var excsI = 0;
                var anysI = 0;
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

                        if (anysI < anys.Length && anys[anysI] == j)
                        {
                            sb.Append($"~");
                            anysI++;
                            continue;
                        }
                    }
                }
                sb.Append("\r\n");
            }

            //end

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        public bool HasSearchPattern = false;
        protected override void DrawCustom()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.fontSize = 28;
            }
            var executors = Target.MaskQueryExecutors;

            using (DragonGUI.Layout.BeginHorizontal())
            {
                GUILayout.Label("[Queries]", _headerStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Copy to Clipboard", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                {
                    CopyToClipboard();
                }
            }

            GUILayout.Space(10f);

            EditorGUILayout.IntField("Total Count: ", executors.Count);


            if(GUILayout.Button("Create Query"))
            {
                QueryBuilderWindow.ShowNew(Target.World);
            }

            HasSearchPattern = true;
            if (string.IsNullOrEmpty(Target.SearchPattern))
            {
                Target.SearchPattern = string.Empty;
                HasSearchPattern = false;
            }
            GUILayout.Space(10f);

            Target.SearchPattern = EditorGUILayout.TextField(Target.SearchPattern, EditorStyles.toolbarSearchField);
            string searchPattern = Target.SearchPattern;

            var r = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 3f);
            DragonGUI.DrawRect(r, Color.white.SetAlpha(0.5f));
            GUILayout.Space(10f);


            //using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                int i = 0;
                foreach (var executor in executors)
                {
                    bool cheack(ReadOnlySpan<Type> types, ReadOnlySpan<char> searchPatternRaw)
                    {
                        foreach (var type in types)
                        {
                            if(type.Name.AsSpan().Contains(searchPatternRaw, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    bool isDraw = false;
                    if (HasSearchPattern)
                    {
                        int subPuttornsCount = 0;
                        int checkPassesCount = 0;
                        foreach (var subPattern in new SearchPattern(searchPattern, SearchPattern.DefaultSeparator))
                        {
                            subPuttornsCount++;
                            if (cheack(executor.Mask.GetIncTypes_Debug(), subPattern) ||
                                cheack(executor.Mask.GetExcTypes_Debug(), subPattern) ||
                                cheack(executor.Mask.GetAnyTypes_Debug(), subPattern))
                            {
                                checkPassesCount++;
                            }
                        }
                        isDraw = subPuttornsCount <= checkPassesCount;
                    }
                    else
                    {
                        isDraw = true;
                    }
    

                    if(isDraw)
                    {
                        DrawQueryInfo(executor, i++);
                    }
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
            using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
            {
                var mask = executor.Mask;
                DrawConstraint("+", mask.Incs);
                DrawConstraint("-", mask.Excs);
                DrawConstraint("~", mask.Anys);
            }

            EditorGUILayout.LongField("Version: ", executor.Version);
            EditorGUILayout.IntField("Entites Count: ", executor.LastCachedCount);
            if (GUILayout.Button("Snapshot"))
            {
                QuerySnapshotWindow.ShowNew(executor.Snapshot());
            }

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

            using (DragonGUI.Layout.BeginHorizontal())
            {
                using (DragonGUI.SetAlignment(value: TextAnchor.MiddleCenter))
                using (DragonGUI.SetFontStyle(value: FontStyle.Bold))
                using (DragonGUI.SetFontSize(value: 18))
                using (DragonGUI.SetColor(Color.white, 0.3f))
                    GUILayout.Label(title, GUILayout.Width(12));

                using (DragonGUI.Layout.BeginVertical())
                {
                    foreach (var inc in ids)
                    {
                        Type type = Target.World.GetComponentType(inc);
                        TypeMeta meta = type.GetMeta();

                        Color color = DragonGUI.SelectPanelColor(meta, i, 9);

                        using (DragonGUI.Layout.BeginVertical(color.SetAlpha(0.2f)))
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

    internal class QueryBuilderWindow : EditorWindow
    {
        private EcsWorld _world;
        private Vector2 _scroll;
        private List<Type> _allTypes = new List<Type>();
        private List<bool> _incFlags = new List<bool>();
        private List<bool> _excFlags = new List<bool>();
        private List<bool> _anyFlags = new List<bool>();

        public static void ShowNew(EcsWorld world)
        {
            var window = CreateInstance<QueryBuilderWindow>();
            window.titleContent = new GUIContent("Create Query");
            window._world = world;
            window.LoadFromWorld(world);
            window.ShowUtility();
        }

        private void LoadFromWorld(EcsWorld world)
        {
            _allTypes.Clear();
            _incFlags.Clear();
            _excFlags.Clear();
            _anyFlags.Clear();

            var pools = world.AllPools.Slice(0, world.PoolsCount);
            foreach (var pool in pools)
            {
                if (pool.IsNullOrDummy()) continue;
                var t = pool.ComponentType;
                _allTypes.Add(t);
                _incFlags.Add(false);
                _excFlags.Add(false);
                _anyFlags.Add(false);
            }
        }

        private void OnGUI()
        {
            if (_world == null)
            {
                EditorGUILayout.LabelField("No world assigned");
                return;
            }

            EditorGUILayout.LabelField("Select component constraints:");
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _allTypes.Count; i++)
            {
                var t = _allTypes[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(t.GetMeta().TypeName, GUILayout.Width(250));
                _incFlags[i] = GUILayout.Toggle(_incFlags[i], "Inc", GUILayout.Width(50));
                _excFlags[i] = GUILayout.Toggle(_excFlags[i], "Exc", GUILayout.Width(50));
                _anyFlags[i] = GUILayout.Toggle(_anyFlags[i], "Any", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                CreateMaskAndRegister();
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateMaskAndRegister()
        {
            var incs = new List<Type>();
            var excs = new List<Type>();
            var anys = new List<Type>();
            for (int i = 0; i < _allTypes.Count; i++)
            {
                if (_incFlags[i]) incs.Add(_allTypes[i]);
                if (_excFlags[i]) excs.Add(_allTypes[i]);
                if (_anyFlags[i]) anys.Add(_allTypes[i]);
            }

            // Build EcsStaticMask using Builder API
            var builder = EcsStaticMask.New();
            if (incs.Count > 0) builder = builder.Inc(incs.ToArray());
            if (excs.Count > 0) builder = builder.Exc(excs.ToArray());
            if (anys.Count > 0) builder = builder.Any(anys.ToArray());
            var staticMask = builder.Build();

            // Force world to create concrete EcsMask and executor by requesting the executor
            _world.Where(staticMask);
        }
    }
}
#endif