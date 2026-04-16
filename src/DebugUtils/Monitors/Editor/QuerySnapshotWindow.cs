#if UNITY_EDITOR
using DCFApixels.DragonECS.Core.Unchecked;
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;
using static DCFApixels.DragonECS.Unity.Editors.DragonGUI;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class QuerySnapshotWindow : EditorWindow
    {
        private EcsWorld _world;
        private StructList<entlong> _list;

        private readonly Color _selectionColor = new Color(0.12f, 0.5f, 1f, 0.40f);
        private readonly Color _isAliveColor = new Color(0.2f, 0.6f, 1f);
        private readonly Color _hoverColor = new Color(1f, 1f, 1f, 0.12f);

        public static void ShowNew(EcsSpan entites)
        {
            var newWin = CreateWindow<QuerySnapshotWindow>("Query Snapshot");
            newWin.Setup(entites);
            newWin.ShowUtility();

            if (UserSettingsPrefs.instance.IsPauseOnSnapshot)
            {
                Debug.Break();
            }
        }
        private void Setup(EcsSpan entites)
        {
            _world = entites.World;
            _list = new StructList<entlong>(entites.Count);
            _list._count = entites.Longs.ToArray(ref _list._items);
        }
        private Vector2 _scrollState;
        private void OnGUI()
        {
            if (_world.IsDestroyed) { _world = null; }
            if (_world == null)
            {
                Close();
                return;
            }
            int selectedEntity = -1;
            var selectedGO = Selection.activeGameObject;
            if (selectedGO != null &&
                selectedGO.TryGetComponent<EntityMonitor>(out var selectedMonitor) &&
                selectedMonitor.Entity.TryUnpack(_world, out selectedEntity) == false)
            {
                selectedEntity = -1;
            }
            SelectEvent selectEvent = default;


            var line = EditorGUIUtility.singleLineHeight;
            var space = EditorGUIUtility.standardVerticalSpacing;
            var step = line + space;
            Event current = Event.current;

            if (hasFocus && current.type == EventType.KeyUp && current.isKey)
            {
                if (current.keyCode == KeyCode.DownArrow)
                {
                    selectEvent.Type = SelectEventType.Down;
                }
                if (current.keyCode == KeyCode.UpArrow)
                {
                    selectEvent.Type = SelectEventType.Up;
                }
            }

            var rect = position;
            rect.x = 0;
            rect.y = 0;

            Rect hyperlinkButtonRect;
            Rect topLineRect;
            (topLineRect, rect) = rect.VerticalSliceTop(line + space);
            topLineRect = topLineRect.AddPadding(0, 0, 0, space);
            (topLineRect, hyperlinkButtonRect) = topLineRect.HorizontalSliceRight(18f);
            EditorGUI.IntField(topLineRect, "World: ", _world.ID);
            using (DragonGUI.SetEnable(_world != null))
            {
                DragonGUI.WorldHyperlinkButton(hyperlinkButtonRect, _world);
            }

            (topLineRect, rect) = rect.VerticalSliceTop(line + space);
            UserSettingsPrefs.instance.IsPauseOnSnapshot = EditorGUI.ToggleLeft(topLineRect, "Pause On Snapshot", UserSettingsPrefs.instance.IsPauseOnSnapshot);

            var viewRect = rect;
            viewRect.x = 0;
            viewRect.y = 0;
            viewRect.height = (line + space) * _list.Count;
            viewRect.xMax -= GUI.skin.verticalScrollbar.fixedWidth;


            var lineRect = viewRect;
            lineRect.height = line;
            Rect statusR;
            (statusR, lineRect) = lineRect.HorizontalSliceLeft(3f);

            _scrollState = GUI.BeginScrollView(rect, _scrollState, viewRect, false, true);
            var checkRect = rect;
            checkRect.position = Vector2.zero;

            bool foundSelected = false;
            for (int i = 0; i < _list.Count; i++)
            {
                EntitySlotInfo entity = (EntitySlotInfo)_list[i];
                bool isAlive = _world.IsAlive(entity.id, entity.gen);
                bool selected = selectedEntity == entity.id && isAlive;
                foundSelected |= selected;
                bool isClick = false;


                bool visible = lineRect.Overlaps(checkRect.AddOffset(_scrollState));
                if (visible)
                {
                    using (DragonGUI.SetAlpha(0)) { GUI.Label(lineRect, string.Empty, GUI.skin.button); }
                    if (DragonGUI.HitTest(lineRect))
                    {
                        EditorGUI.DrawRect(lineRect, _hoverColor);
                        if (current.type == EventType.MouseUp)
                        {
                            isClick = true;
                        }
                    }
                    if (selected)
                    {
                        DragonGUI.DrawRect(lineRect, _selectionColor);
                    }
                    if (isAlive)
                    {
                        DragonGUI.DrawRect(statusR, _isAliveColor);
                    }
                    var (labelR, infoR) = lineRect.HorizontalSliceLeft(45f);
                    infoR.width = Mathf.Min(infoR.width, 200f);
                    var (lR, rR) = infoR.HorizontalSliceLerp(0.5f);
                    GUI.Label(labelR, "Entity", GUI.skin.label);
                    EditorGUI.IntField(lR, entity.id, GUI.skin.label);
                    EditorGUI.IntField(rR, entity.gen, GUI.skin.label);

                    if (isClick && isAlive)
                    {
                        selectEvent.Type = SelectEventType.Click;
                        selectEvent.EntityID = entity.id;
                        selectEvent.Index = i;
                    }
                }

                if (isAlive)
                {
                    switch (selectEvent.Type)
                    {
                        case SelectEventType.Up:
                            {
                                if (foundSelected == false)
                                {
                                    selectEvent.EntityID = entity.id;
                                    selectEvent.Index = i;
                                }
                            }
                            break;
                        case SelectEventType.Down:
                            {
                                if (foundSelected && selected == false && selectEvent.EntityID == 0)
                                {
                                    selectEvent.EntityID = entity.id;
                                    selectEvent.Index = i;
                                }
                            }
                            break;
                    }
                }

                lineRect.y += step;
                statusR.y += step;
            }
            GUI.EndScrollView();


            if (selectEvent.IsSelected && selectedEntity != selectEvent.EntityID)
            {
                float top = selectEvent.Index * step;
                float bottom = top + step;

                if (top < _scrollState.y)
                {
                    _scrollState.y = top;
                }
                else if (bottom > _scrollState.y + checkRect.height)
                {
                    _scrollState.y = bottom - checkRect.height;
                }
                GUIUtility.keyboardControl = 0;

                SelectEntity(selectEvent.EntityID);
            }


            if (selectEvent.IsSelected)
            {
                Repaint();
                return;
            }
        }

        private void SelectEntity(int entityID)
        {
            var monitor = _world.Get<EntityLinksComponent>().GetMonitorLink(entityID);
            EditorGUIUtility.PingObject(monitor);
            Selection.activeObject = monitor;
        }
        private void OnDestroy() { }


        private struct SelectEvent
        {
            public SelectEventType Type;
            public int EntityID;
            public int Index;
            public bool IsSelected { get { return Type != SelectEventType.None && EntityID != 0; } }
        }
        private enum SelectEventType
        {
            None,
            Click,
            Up,
            Down,
        }
    }
}
#endif