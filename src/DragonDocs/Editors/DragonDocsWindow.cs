#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Editors;
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Docs.Editors
{
    internal class DragonDocsWindow : EditorWindow
    {
        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/Documentation")]
        static void Open()
        {
            var wnd = GetWindow<DragonDocsWindow>();
            wnd.titleContent = new GUIContent($"{EcsConsts.FRAMEWORK_NAME} Documentation");
            wnd.minSize = new Vector2(100f, 120f);
            wnd.Show();
        }

        private int _selectedIndex = 0;

        private Vector2 ButtonsScrolPosition;
        private Vector2 DataScrolPosition;

        private Vector2 _buttonsWidthDragStartPos = Vector2.zero;
        private float _buttonsWidthDragStartValue = 200f;
        private float _buttonsWidth = 200f;
        private DragState _dragState;
        private enum DragState
        {
            None,
            Init,
            Update,
        }

        private DragonDocsPrefs Prefs { get { return DragonDocsPrefs.instance; } }
        private static bool IsShowHidden
        {
            get { return SettingsPrefs.instance.IsShowHidden; }
            set { SettingsPrefs.instance.IsShowHidden = value; }
        }
        private static bool IsUseCustomNames
        {
            get { return SettingsPrefs.instance.IsUseCustomNames; }
            set { SettingsPrefs.instance.IsUseCustomNames = value; }
        }


        private bool _searchingSampleChanged = false;
        private string _searchingSampleEnter = string.Empty;
        private string _searchingSample = string.Empty;
        private bool[] _searchingHideMetaMap = System.Array.Empty<bool>();
        private bool[] _searchingHideGroupMap = System.Array.Empty<bool>();

        private void OnGUI()
        {
            Event current = Event.current;
            DragonDocs docs = DragonDocsPrefs.instance.Docs;
            var metas = docs.Metas;
            if (docs == null || docs.Metas.IsEmpty)
            {
                docs = DragonDocs.Generate();
                DragonDocsPrefs.instance.Save(docs);
            }
            var infos = DragonDocsPrefs.instance.Infos;
            if (_searchingHideMetaMap.Length < metas.Length)
            {
                System.Array.Resize(ref _searchingHideMetaMap, metas.Length);
            }
            if (_searchingHideGroupMap.Length < infos.Length)
            {
                System.Array.Resize(ref _searchingHideGroupMap, DragonDocsPrefs.instance.Infos.Length);
            }

            if(_selectedIndex < 0 || _selectedIndex  >= infos.Length)
            {
                _selectedIndex = 0;
            }


            DrawToolbar();

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));


            ButtonsScrolPosition = GUILayout.BeginScrollView(ButtonsScrolPosition, UnityEditorUtility.GetStyle(Color.black, 0f), GUILayout.Width(_buttonsWidth));
            var selectedGroupInfo = DrawGroups();
            GUILayout.EndScrollView();

            DrawDragger();

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * -2f);


            DataScrolPosition = GUILayout.BeginScrollView(DataScrolPosition, UnityEditorUtility.GetStyle(Color.black, 0.2f), GUILayout.ExpandWidth(true));
            DrawSelectedGroupMeta(selectedGroupInfo);
            GUILayout.EndScrollView();

            //GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * -2f);
            GUILayout.EndHorizontal();

            Rect r = GUILayoutUtility.GetLastRect();
            float h = r.height;
            r.height = EditorGUIUtility.standardVerticalSpacing;
            //EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.3f));
            r.y += h;
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.3f));

            GUI.enabled = true;

            GUILayout.Label(infos[_selectedIndex].Path);
        }

        private void DrawToolbar()
        {
            using (EcsGUI.SetColor(GUI.color * 0.8f))
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);

            if (GUILayout.Button("Update", EditorStyles.toolbarButton, GUILayout.Width(80f)))
            {
                DragonDocs docs = DragonDocs.Generate();
                DragonDocsPrefs.instance.Save(docs);
            }

            if (EcsGUI.Layout.IconButton(IsShowHidden ? Icons.Instance.VisibilityIconOn : Icons.Instance.VisibilityIconOff, 0f, IsShowHidden ? "Show Hidden" : "Don't Show Hidden", EditorStyles.toolbarButton, GUILayout.Width(EditorGUIUtility.singleLineHeight * 1.6f)))
            {
                IsShowHidden = !IsShowHidden;
            }

            if (EcsGUI.Layout.IconButton(IsUseCustomNames ? Icons.Instance.LabelIconMeta : Icons.Instance.LabelIconType, 1f, IsUseCustomNames ? "Use Meta Name" : "Use Type Name", EditorStyles.toolbarButton, GUILayout.Width(EditorGUIUtility.singleLineHeight * 1.6f)))
            {
                IsUseCustomNames = !IsUseCustomNames;
            }

            GUILayout.Label("");
            EditorGUI.BeginChangeCheck();
            _searchingSampleEnter = EditorGUILayout.TextField(_searchingSampleEnter, EditorStyles.toolbarTextField, GUILayout.ExpandHeight(true), GUILayout.MaxWidth(200f));
            if (EditorGUI.EndChangeCheck())
            {
                _searchingSampleChanged = true;
            }
            if ((_searchingSampleChanged && Event.current.keyCode == KeyCode.Return) ||
                EcsGUI.Layout.IconButton(Icons.Instance.SearchIcon, 3f, null, EditorStyles.toolbarButton, GUILayout.ExpandHeight(true), GUILayout.Width(EditorGUIUtility.singleLineHeight * 1.6f)))
            {
                Searh();
            }
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2f);
            GUILayout.EndHorizontal();

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * -2f);
        }

        private void Searh()
        {
            _searchingSample = _searchingSampleEnter; 
            _searchingSampleChanged = false;
            DragonDocs docs = DragonDocsPrefs.instance.Docs;
            var metas = docs.Metas;
            if (_searchingSample.Length <= 0)
            {
                var infos = DragonDocsPrefs.instance.Infos;
                var isExpands = DragonDocsPrefs.instance.IsExpands;

                for (int i = 0; i < _searchingHideMetaMap.Length; i++)
                {
                    _searchingHideMetaMap[i] = false;
                }
                for (int i = 0; i < _searchingHideGroupMap.Length; i++)
                {
                    _searchingHideGroupMap[i] = false;
                }

                {
                    int i = _selectedIndex;
                    var info = infos[i];
                    int depth = info.Depth;
                    while (depth > 0)
                    {
                        i--;
                        info = infos[i];

                        if (info.Depth >= depth)
                        {
                            continue;
                        }

                        depth = info.Depth;
                        isExpands[i] = true;
                    }
                }
            }
            else
            {
                var infos = DragonDocsPrefs.instance.Infos;

                for (int i = 0; i < infos.Length; i++)
                {
                    var info = infos[i];
                    int visibleCount = 0;
                    bool isUseCustomNames = IsUseCustomNames;
                    for (int j = info.StartIndex, jMax = j + info.Length; j < jMax; j++)
                    {
                        var b = (isUseCustomNames ? metas[j].Name : metas[j].TypeName).Contains(_searchingSample, System.StringComparison.InvariantCultureIgnoreCase);
                        _searchingHideMetaMap[j] = !b;

                        if (b)
                        {
                            visibleCount++;
                        }
                    }

                    _searchingHideGroupMap[i] = visibleCount == 0;
                }
            }
        }

        private void DrawSelectedGroupMeta(MetaGroupInfo info)
        {
            bool hide = IsShowHidden == false;
            var metas = Prefs.Docs.Metas;
            int iMax = info.Length;
            for (int i = 0, j = info.StartIndex; i < iMax; j++)
            {
                if (_searchingHideMetaMap[j] || (metas[j]._isHidden && hide))
                {
                    iMax--;
                }
                else
                {
                    DrawMeta(metas[j], i, 12);
                    i++;
                }
            }

            if (iMax <= 0)
            {
                GUILayout.Label(info.Length <= 0 ? "empty group" : "there are hidden items", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
        }

        private void DrawMeta(DragonDocsMeta meta, int index, int total)
        {
            using (EcsGUI.SetIndentLevel(0))
            {
                Color panelColor = EcsGUI.SelectPanelColor(meta.Color, meta.IsCustomColor, index, total).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);
                Color alphaPanelColor = panelColor;
                alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

                GUILayout.BeginVertical(UnityEditorUtility.GetStyle(alphaPanelColor));
                GUILayout.Space(1f);

                GUILayout.BeginHorizontal();
                GUILayout.TextArea(IsUseCustomNames ? meta.Name : meta.TypeName, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                if (meta.IsCustomName)
                {
                    using (EcsGUI.SetAlpha(0.64f)) using (EcsGUI.SetAlignment(GUI.skin.label, TextAnchor.MiddleRight))
                    {
                        GUILayout.TextArea(IsUseCustomNames ? meta.TypeName : meta.Name, GUI.skin.label);
                    }
                }
                GUILayout.EndHorizontal();

                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (string.IsNullOrEmpty(meta.Description) == false)
                {
                    Rect lineRect = lastRect;
                    lineRect.yMin = lineRect.yMax;
                    lineRect.yMax += 1f;
                    lineRect.y += 5f;
                    EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.12f));

                    GUILayout.Space(7f);

                    GUILayout.TextArea(meta.Description, EditorStyles.wordWrappedLabel);
                }

                if (meta._tags.Length > 0)
                {
                    Rect lineRect = GUILayoutUtility.GetLastRect();
                    lineRect.yMin = lineRect.yMax;
                    lineRect.yMax += 1f;
                    lineRect.y += 5f;
                    EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.12f));

                    GUILayout.Space(3f);

                    var tagsstring = string.Join(',', meta._tags);
                    using (EcsGUI.SetAlpha(0.5f))
                    {
                        GUILayout.TextArea(tagsstring, EditorStyles.wordWrappedMiniLabel);
                    }
                }


                GUILayout.Space(1f);
                GUILayout.EndVertical();
            }
        }

        private MetaGroupInfo DrawGroups()
        {
            Event current = Event.current;
            MetaGroupInfo result = new MetaGroupInfo("NO_NAME", "NO_NAME", 0, 0, 0);
            var infos = Prefs.Infos;
            var isExpands = Prefs.IsExpands;

            using (EcsGUI.SetIndentLevel(0))
            {
                int clippingDepth = int.MaxValue;

                for (int i = 0; i < infos.Length; i++)
                {
                    if (_searchingHideGroupMap[i]) { continue; }

                    var groupInfo = infos[i];

                    if (i == _selectedIndex)
                    {
                        result = groupInfo;
                    }

                    if (_searchingSample.Length == 0 && groupInfo.Depth > clippingDepth)
                    {
                        continue;
                    }
                    else
                    {
                        clippingDepth = int.MaxValue;
                    }

                    if(_searchingSample.Length == 0)
                    {
                        EditorGUI.indentLevel = groupInfo.Depth;
                    }

                    GUIContent label = UnityEditorUtility.GetLabel(_searchingSample.Length == 0 ? groupInfo.Name : groupInfo.Path);
                    Rect r = GUILayoutUtility.GetRect(label, EditorStyles.foldout);

                    if (i == _selectedIndex)
                    {
                        EditorGUI.DrawRect(r, new Color(0.12f, 0.5f, 1f, 0.40f));
                    }

                    using (EcsGUI.SetColor(0, 0, 0, 0)) using (EcsGUI.Disable) { GUI.Button(r, ""); }

                    bool isClick = false;
                    if (EcsGUI.HitTest(r))
                    {
                        EditorGUI.DrawRect(r, new Color(1f, 1f, 1f, 0.12f));
                        if (current.type == EventType.MouseUp)
                        {
                            isClick = true;

                            //_selectedIndex = i;
                            //current.Use();
                        }
                    }

                    if (_searchingSample.Length != 0 || (i + 1 == infos.Length || infos[i + 1].Depth <= groupInfo.Depth))
                    {
                        using (EcsGUI.SetBackgroundColor(0, 0, 0, 0))
                        {
                            EditorGUI.Foldout(r, false, label, EditorStyles.foldout);
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        isExpands[i] = EditorGUI.Foldout(r, isExpands[i], label, EditorStyles.foldout);
                        if (EditorGUI.EndChangeCheck())
                        {
                            isClick = false;
                        }
                    }

                    if (isClick)
                    {
                        _selectedIndex = i;
                        current.Use();
                    }

                    if (isExpands[i] == false)
                    {
                        clippingDepth = groupInfo.Depth;
                    }

                    if (groupInfo.Length > 0)
                    {
                        r.xMax = r.xMin;
                        r.xMin -= 2f;

                        r.yMin += 1;
                        r.yMax -= 1;
                        EditorGUI.DrawRect(r, new Color(0.2f, 0.6f, 1f));
                    }
                }
            }
            return result;
        }

        private void DrawDragger()
        {
            const float DRAG_RESIZE_WIDTH = 4f;

            Rect rect = GUILayoutUtility.GetLastRect();
            float m = DRAG_RESIZE_WIDTH;
            if (_dragState != DragState.None)
            {
                m *= 2f;
            }
            rect.xMin = rect.xMax;
            rect.x -= m / 2f;
            rect.width = m;

            EditorGUI.DrawRect(rect.AddPadding(1f, 0), new Color(0, 0, 0, 0.3f));

            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    if (EcsGUI.HitTest(rect))
                    {
                        _buttonsWidthDragStartPos = current.mousePosition;
                        _buttonsWidthDragStartValue = _buttonsWidth;
                        _dragState = DragState.Init;
                        current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (_dragState != DragState.None)
                    {
                        _dragState = DragState.None;
                        current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        switch (_dragState)
                        {
                            case DragState.Init:
                                {
                                    if ((Event.current.mousePosition - _buttonsWidthDragStartPos).sqrMagnitude > 16f)
                                    {
                                        _dragState = DragState.Update;
                                    }
                                }
                                break;
                            case DragState.Update:
                                {
                                    _buttonsWidth = _buttonsWidthDragStartValue + (Event.current.mousePosition.x - _buttonsWidthDragStartPos.x);
                                    _buttonsWidth = Mathf.Max(6f, _buttonsWidth);
                                    current.Use();//TODO кажется это можно использовать вместо лайфахака с кнопкой для моментальной реакции пир наведении курсора для кнопок с иконками
                                }
                                break;
                        }
                    }
                    break;
                case EventType.Repaint:
                    {


                        EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
                    }
                    break;
            }
        }
    }
}
#endif