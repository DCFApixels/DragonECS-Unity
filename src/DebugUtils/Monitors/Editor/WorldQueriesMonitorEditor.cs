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
        private const char _searchPatternSeparator = '/';

        public readonly struct SearchPattern
        {
            private readonly string _pattern;
            private readonly char _separator;
            public SearchPattern(string pattern, char separator)
            {
                _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
                _separator = separator;
            }
            public Enumerator GetEnumerator() => new Enumerator(_pattern, _separator);
            public ref struct Enumerator
            {
                private readonly string _pattern;
                private readonly char _separator;
                private int _start;
                private int _currentStart;
                private int _currentLength;

                public Enumerator(string pattern, char separator)
                {
                    _pattern = pattern;
                    _separator = separator;
                    _start = 0;
                    _currentStart = -1;
                    _currentLength = 0;
                }

                public ReadOnlySpan<char> Current
                {
                    get
                    {
                        if (_currentStart < 0)
                            throw new InvalidOperationException("Enumeration not started or already finished");
                        return _pattern.AsSpan(_currentStart, _currentLength);
                    }
                }

                public bool MoveNext()
                {
                    if (_pattern == null || _start > _pattern.Length)
                        return false;

                    int len = _pattern.Length;
                    while (_start <= len)
                    {
                        int i = _start;
                        while (i < len && _pattern[i] != _separator)
                            i++;

                        int subLen = i - _start;
                        if (subLen > 0) // возвращаем только непустые подстроки
                        {
                            _currentStart = _start;
                            _currentLength = subLen;
                            _start = i + 1;
                            return true;
                        }

                        // пустая подстрока — пропускаем разделитель и продолжаем
                        _start = i + 1;
                    }

                    return false;
                }
            }
        }

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

        public bool HasSearchPattern = false;
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


            HasSearchPattern = true;
            if (string.IsNullOrEmpty(Target.SearchPattern))
            {
                Target.SearchPattern = string.Empty;
                HasSearchPattern = false;
            }

            Target.SearchPattern = EditorGUILayout.TextField("Search: ", Target.SearchPattern);


            string searchPattern = Target.SearchPattern;

            GUILayout.Space(20);

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
                        foreach (var subPattern in new SearchPattern(searchPattern, _searchPatternSeparator))
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
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
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
                        TypeMeta meta = type.GetMeta();

                        Color color = EcsGUI.SelectPanelColor(meta, i, 9);

                        using (EcsGUI.Layout.BeginVertical(color.SetAlpha(0.2f)))
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