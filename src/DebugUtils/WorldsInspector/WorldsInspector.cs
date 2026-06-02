#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public class WorldsInspector : EditorWindow
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
        private WorldInfo _lastSelectedWorldInfo;


        private void OnDestroy()
        {
            EcsWorld.OnWorldCreated -= OnWorldCreated;
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
                else
                {
                    SelectWorld(null);
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
            GUILayout.Label("No Worlds found!", EditorStyles.centeredGreyMiniLabel);
        }
        private void DrawWorldSelection()
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

        private EcsWorld _worldEditorsWorld;
        private Editor _worldMonitorEditor;
        private Editor _wolrdQueriesMonitorEditor;
        private void DrawWorld(EcsWorld world)
        {
            if (GUILayout.Button("<- Back"))
            {
                SelectWorld(null);
            }

            ref var links = ref world.Get<DragonGUI.EntityLinksComponent>();

            var monitor = links.GetWorldMonitor();
            if (monitor == null)
            {
                GUILayout.Label("No any debug monitor found!", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (_worldEditorsWorld != world)
            {
                _worldEditorsWorld = world;
                _worldMonitorEditor = Editor.CreateEditor(monitor);
                _wolrdQueriesMonitorEditor = Editor.CreateEditor(monitor.GetComponent<WorldQueriesMonitor>());
            }

            _worldMonitorEditor.OnInspectorGUI();
            _wolrdQueriesMonitorEditor.OnInspectorGUI();
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
            _lastSelectedWorldInfo = new WorldInfo(world);
        }





        private struct WorldInfo : IEquatable<WorldInfo>
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
