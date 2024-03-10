#if UNITY_EDITOR
using DCFApixels.DragonECS.RunnersCore;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(PipelineMonitor))]
    internal class PipelineMonitorEditor : Editor
    {
        private MetaColorAttribute _fakeDebugColorAttribute = new MetaColorAttribute(190, 190, 190);
        private Type _debugColorAttributeType = typeof(MetaColorAttribute);
        private GUIStyle _headerStyle;
        private GUIStyle _interfacesStyle;
        private Color _interfaceColor = new Color(0.96f, 1f, 0.16f);

        private GUIStyle systemsListStyle;

        private PipelineMonitor Target => (PipelineMonitor)target;
        private bool IsShowInterfaces
        {
            get { return DebugMonitorPrefs.instance.IsShowInterfaces; }
            set { DebugMonitorPrefs.instance.IsShowInterfaces = value; }
        }
        private bool IsShowHidden
        {
            get { return DebugMonitorPrefs.instance.IsShowHidden; }
            set { DebugMonitorPrefs.instance.IsShowHidden = value; }
        }

        public override void OnInspectorGUI()
        {
            systemsListStyle = new GUIStyle(EditorStyles.miniLabel);
            systemsListStyle.wordWrap = true;

            if (Target.Pipeline == null || Target.Pipeline.IsDestoryed)
            {
                return;
            }
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _interfacesStyle = new GUIStyle(EditorStyles.miniLabel);
                _interfacesStyle.hover.textColor = _interfaceColor;
                _interfacesStyle.focused.textColor = _interfaceColor;
                _interfacesStyle.active.textColor = _interfaceColor;
                _interfacesStyle.normal.textColor = _interfaceColor;
                _interfacesStyle.wordWrap = true;
                _headerStyle.fontSize = 28;
            }

            GUILayout.Label("[Systems]", _headerStyle);

            IsShowInterfaces = EditorGUILayout.Toggle("Show Interfaces", IsShowInterfaces);
            IsShowHidden = EditorGUILayout.Toggle("Show Hidden", IsShowHidden);

            GUILayout.BeginVertical();
            foreach (var item in Target.Pipeline.AllSystems)
            {
                DrawSystem(item);
            }
            GUILayout.EndVertical();


            GUILayout.Label("[Runners]", _headerStyle);

            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));
            foreach (var item in Target.Pipeline.AllRunners)
            {
                if (item.Key.IsInterface == false)
                {
                    DrawRunner(item.Value);
                }
            }
            GUILayout.EndVertical();
        }
        private void DrawSystem(IEcsProcess system)
        {
            if (system is SystemsLayerMarkerSystem markerSystem)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));

                GUILayout.BeginHorizontal();
                GUILayout.Label("<");
                GUILayout.Label($"{markerSystem.name}", EditorStyles.boldLabel);
                GUILayout.Label(">", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                return;
            }

            Type type = system.GetType();
            TypeMeta meta = type.ToMeta();

            if (CheckIsHidden(meta))
            {
                return;
            }

            string name = meta.Name;
            Color color = meta.Color.ToUnityColor();

            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(color, 0.2f));
            if (IsShowInterfaces)
            {
                GUILayout.Label(string.Join(", ", type.GetInterfaces().Select(o => o.Name)), _interfacesStyle);
            }
            GUILayout.Label(name, EditorStyles.boldLabel);
            GUILayout.EndVertical();
        }
        private void DrawRunner(IEcsRunner runner)
        {
            Type type = runner.GetType();
            TypeMeta meta = type.ToMeta();

            if (CheckIsHidden(meta))
            {
                return;
            }
            Color color = meta.Color.ToUnityColor();

            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(color, 0.2f));
            GUILayout.Label(meta.Name, EditorStyles.boldLabel);
            GUILayout.Label(string.Join(", ", runner.ProcessRaw.Cast<object>().Select(o => o.GetType().Name)), systemsListStyle);
            GUILayout.EndVertical();
        }
        private bool CheckIsHidden(TypeMeta meta)
        {
            if (IsShowHidden)
                return false;

            return meta.IsHidden;
        }
    }
}
#endif