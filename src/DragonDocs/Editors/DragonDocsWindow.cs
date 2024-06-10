#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Editors;
using DCFApixels.DragonECS.Unity.Internal;
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

        private void OnGUI()
        {
            Event current = Event.current;
            DragonDocs docs = DragonDocsPrefs.instance.Docs;
            if(docs == null || docs.Metas.IsEmpty)
            {
                docs = DragonDocs.Generate();
                DragonDocsPrefs.instance.Save(docs);
            }


            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));


            ButtonsScrolPosition = GUILayout.BeginScrollView(ButtonsScrolPosition, EditorStyles.helpBox, GUILayout.Width(_buttonsWidth));
            var selectedGroupInfo = DrawGroups();
            GUILayout.EndScrollView();

            DrawDragger();

            DataScrolPosition = GUILayout.BeginScrollView(DataScrolPosition, UnityEditorUtility.GetStyle(Color.black, 0.2f), GUILayout.ExpandWidth(true));
            DrawSelectedGroupMeta(selectedGroupInfo);
            GUILayout.EndScrollView();

            
            GUILayout.EndHorizontal();

            GUI.enabled = true;
            IsShowHidden = EditorGUILayout.Toggle("Show Hidden", IsShowHidden);
            if (GUILayout.Button("Update"))
            {
                docs = DragonDocs.Generate();
                DragonDocsPrefs.instance.Save(docs); 
            }
        }


        private void DrawSelectedGroupMeta(MetaGroupInfo info)
        {
            var metas = Prefs.Docs.Metas;
            for (int i = 0, j = info.StartIndex; i < info.Length; i++, j++)
            {
                DrawMeta(metas[j], i, 12);
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
                GUILayout.TextArea(meta.Name, EditorStyles.boldLabel);

                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (string.IsNullOrEmpty(meta.Description))
                {
                    using (EcsGUI.SetContentColor(1f, 1f, 1f, 0.4f))
                    {
                        Rect pos = lastRect;
                        pos.xMin = Mathf.Max(EditorGUIUtility.labelWidth, pos.xMax - 42f);
                        GUI.Label(pos, "empty");
                    }
                }
                else
                {
                    Rect lineRect = lastRect;
                    lineRect.yMin = lineRect.yMax;
                    lineRect.yMax += 1f;
                    lineRect.y += 5f;
                    EditorGUI.DrawRect(lineRect, new Color(1, 1, 1, 0.12f));

                    GUILayout.Space(7f);

                    GUILayout.TextArea(meta.Description, EditorStyles.wordWrappedLabel);
                }
                
                GUILayout.Space(1f);
                GUILayout.EndVertical();
            }
        }

        private MetaGroupInfo DrawGroups()
        {
            using (EcsGUI.SetIndentLevel(0))
            {
                Event current = Event.current;
                MetaGroupInfo result = new MetaGroupInfo("NO_NAME", "NO_NAME", 0, 0, 0);
                var infos = Prefs.Infos;
                var IsExpands = Prefs.IsExpands;

                int clippingDepth = int.MaxValue;


                for (int i = 0; i < infos.Length; i++)
                {
                    var groupInfo = infos[i];

                    if (groupInfo.Depth > clippingDepth)
                    {
                        continue;
                    }
                    else
                    {
                        clippingDepth = int.MaxValue;
                    }

                    EditorGUI.indentLevel = groupInfo.Depth;

                    GUIContent label = UnityEditorUtility.GetLabel(groupInfo.Name);
                    Rect r = GUILayoutUtility.GetRect(label, EditorStyles.foldout);

                    if (i == _selectedIndex)
                    {
                        EditorGUI.DrawRect(r, new Color(0.12f, 0.5f, 1f, 0.40f));
                        result = groupInfo;
                    }

                    bool isClick;
                    using (EcsGUI.SetColor(0, 0, 0, 0))
                    using (EcsGUI.Disable)
                    {
                        isClick = GUI.Button(r, "");
                    }
                    if (EcsGUI.HitTest(r))
                    {
                        EditorGUI.DrawRect(r, new Color(1f, 1f, 1f, 0.12f));
                        if (current.type == EventType.MouseDown)
                        {
                            _selectedIndex = i;
                        }
                    }

                    if (i + 1 == infos.Length || infos[i + 1].Depth <= groupInfo.Depth)
                    {
                        using (EcsGUI.SetBackgroundColor(0, 0, 0, 0))
                            EditorGUI.Foldout(r, false, label, EditorStyles.foldout);
                    }
                    else
                    {
                        IsExpands[i] = EditorGUI.Foldout(r, IsExpands[i], label, EditorStyles.foldout);
                        if (i == 0)
                        {
                            IsExpands[i] = true;
                        }
                    }

                    if (IsExpands[i] == false)
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

                return result;
            }
        }

        private void DrawDragger()
        {
            const float DRAG_RESIZE_WIDTH = 4f;

            Rect rect;
            float m = DRAG_RESIZE_WIDTH;
            if (_dragState != DragState.None)
            {
                m *= 200f;
            }
            rect = GUILayoutUtility.GetLastRect();
            rect.xMin = rect.xMax;
            rect.xMax = rect.xMax + m;

            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    
                    if (EcsGUI.HitTest(rect))
                    {
                        _buttonsWidthDragStartPos = current.mousePosition;
                        _buttonsWidthDragStartValue = _buttonsWidth;
                        _dragState = DragState.Init;
                    }
                    break;
                case EventType.MouseUp:
                    _dragState = DragState.None;
                    current.Use();
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