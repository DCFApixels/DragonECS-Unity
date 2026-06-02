#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class WorldsInspector : EditorWindow
    {
        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/WorldsInspector")]
        public static void Open()
        {
            var wnd = GetWindow<WorldsInspector>();
            wnd.titleContent = new GUIContent(UnityEditorUtility.TransformFieldName(nameof(WorldsInspector)));
            wnd.Show();

            EcsWorld.OnWorldCreated -= wnd.OnWorldCreated;
            EcsWorld.OnWorldCreated += wnd.OnWorldCreated;
        }
        private void OnWorldCreated(EcsWorld world)
        {
            Repaint();
        }

        private EcsWorld _selecedWorld;
        private WorldInfo _lastSelectedWorldInfo => UserSettingsPrefs.instance.LastSelectedWorldInInspector;


        private void OnDestroy()
        {
            EcsWorld.OnWorldCreated -= OnWorldCreated;
            if (_worldMonitorEditor)
            {
                DestroyImmediate(_worldMonitorEditor);
            }
            if (_wolrdQueriesMonitorEditor)
            {
                DestroyImmediate(_wolrdQueriesMonitorEditor);
            }
        }

        private void OnGUI()
        {
            if (EcsWorld.AllWorldsCount <= 0)
            {
                DrawNoWorlds();
                return;
            }

            if (TryGetSelecedWorld(out _) == false)
            {
                if (_lastSelectedWorldInfo.IsNull == false &&
                    EcsWorld.TryGetWorld(_lastSelectedWorldInfo.ID, out var fw) &&
                    _lastSelectedWorldInfo.Equals(new WorldInfo(fw)))
                {
                    _selecedWorld = fw;
                }
            }


            if (TryGetSelecedWorld(out var sw))
            {
                DrawWorld(sw);
            }
            else
            {
                DrawWorldSelection();
            }
        }
        private void DrawNoWorlds()
        { 
            var logorect = new Rect(0, 0, 64, 64);
            using (DragonGUI.SetAlpha(0.3f))
            {
                GUI.DrawTexture(logorect, Icons.Instance.Logo128);
            }
            GUILayout.Label("No Worlds found!", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true));
        }
        private void DrawWorldSelection()
        {
            using (DragonGUI.Layout.BeginHorizontal())
            {
                var logorect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
                using (DragonGUI.SetAlpha(0.3f))
                {
                    GUI.DrawTexture(logorect, Icons.Instance.Logo128);
                }

                using (DragonGUI.Layout.BeginVertical())
                {
                    foreach (var worldID in EcsWorld.AllWorldIDs)
                    {
                        var world = EcsWorld.GetWorld((short)worldID);

                        using (DragonGUI.Layout.BeginHorizontal())
                        {
                            if (GUILayout.Button($"World {world.ID} ({world.Name})"))
                            {
                                SelectWorld(world);
                            }
                            GUILayout.Label(world.Count.ToString(), GUILayout.MaxWidth(80f));
                        }
                    }
                }
            }
        }

        private EcsWorld _worldEditorsWorld;
        private Editor _worldMonitorEditor;
        private Editor _wolrdQueriesMonitorEditor;
        private Vector2 _worldScrollPos;
        private float _height;
        private void DrawWorld(EcsWorld world)
        {
            using (DragonGUI.Layout.BeginHorizontal())
            {
                var logorect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(false));
                using (DragonGUI.SetAlpha(0.3f))
                {
                    GUI.DrawTexture(logorect, Icons.Instance.Logo128);
                }
                if (GUILayout.Button("<- Back"))
                {
                    SelectWorld(null);
                }
            }
                

            ref var links = ref world.Get<DragonGUI.EntityLinksComponent>();
            var monitor = links.GetWorldMonitor();
            if (monitor == null)
            {
                GUILayout.Label("No any debug monitor found!", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true));
                return;
            }

            float height = 0;
            _worldScrollPos = GUILayout.BeginScrollView(_worldScrollPos, false, true, GUILayout.ExpandHeight(true));
            {
                GUILayout.Space(_height);
                float contentWidth = position.width - GUI.skin.verticalScrollbar.fixedWidth;
                GUILayout.BeginArea(new Rect(0, 0, contentWidth, _height));
                GUILayout.Space(0);
                Rect r1 = GUILayoutUtility.GetLastRect();

                if (_worldEditorsWorld != world)
                {
                    _worldEditorsWorld = world;
                    _worldMonitorEditor = Editor.CreateEditor(monitor);
                    _wolrdQueriesMonitorEditor = Editor.CreateEditor(monitor.GetComponent<WorldQueriesMonitor>());
                }
                _worldMonitorEditor.OnInspectorGUI();
                _wolrdQueriesMonitorEditor.OnInspectorGUI();

                GUILayout.Space(0);
                Rect r2 = GUILayoutUtility.GetLastRect();
                height = r2.y - r1.y;
                GUILayout.EndArea();
            }
            GUILayout.EndScrollView();

            var e = Event.current;
            if (e.type == EventType.Repaint || e.type == EventType.Layout && height > 6f)
            {
                _height = height;
            }
        }

        private bool TryGetSelecedWorld(out EcsWorld world)
        {
            if (_selecedWorld.IsNullOrDetroyed())
            {
                world = null;
                return false;
            }
            world = _selecedWorld;
            return true;
        }
        private void SelectWorld(EcsWorld world)
        {
            _selecedWorld = world;
            UserSettingsPrefs.instance.LastSelectedWorldInInspector = new WorldInfo(world);
        }

        [System.Serializable]
        internal struct WorldInfo : IEquatable<WorldInfo>
        {
            public short ID;
            public Type Type;
            public string Name;
            public bool IsNull
            {
                get { return ID == 0 && Type == null && Name == null; }
            }
            public WorldInfo(EcsWorld world)
            {
                if (world == null)
                {
                    ID = 0;
                    Type = null;
                    Name = null;
                }
                else
                {
                    ID = world.ID;
                    Type = world.GetType();
                    Name = world.Name;
                }
            }
            public bool Equals(WorldInfo other)
            {
                return ID == other.ID && Type == other.Type && Name == other.Name;
            }
        }
    }
}
#endif