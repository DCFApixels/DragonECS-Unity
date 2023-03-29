using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


namespace DCFApixels.DragonECS.Unity
{
    [DebugHide, DebugColor(DebugColor.Gray)]
    public class SystemsDebugSystem : IEcsPreInitSystem
    {
        private string _name;
        public SystemsDebugSystem(string name = "Systems")
        {
            _name = name;
        }

        void IEcsPreInitSystem.PreInit(EcsSystems systems)
        {
            SystemsDebugMonitor monitor = new GameObject(EcsConsts.DEBUG_PREFIX + _name).AddComponent<SystemsDebugMonitor>();
            monitor.source = this;
            monitor.systems = systems;
            monitor.systemsName = _name;
            Object.DontDestroyOnLoad(monitor.gameObject);
        }
    }

    public class SystemsDebugMonitor : MonoBehaviour
    {
        internal SystemsDebugSystem source;
        internal EcsSystems systems;
        internal string systemsName;
    }

#if UNITY_EDITOR

    namespace Editors
    {
        using System;
        using System.Linq;
        using UnityEditor;

        [CustomEditor(typeof(SystemsDebugMonitor))]
        public class SystemsDebugMonitorEditor : Editor
        {
            private DebugColorAttribute _fakeDebugColorAttribute = new DebugColorAttribute(DebugColor.White);
            private Type _debugColorAttributeType = typeof(DebugColorAttribute);
            private GUIStyle _headerStyle;
            private GUIStyle _interfacesStyle;
            private Color _interfaceColor = new Color(0.96f, 1f, 0.16f);
            private SystemsDebugMonitor Target => (SystemsDebugMonitor)target;
            public override void OnInspectorGUI()
            {
                if (Target.source == null)
                    return;
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                    _interfacesStyle = new GUIStyle(EditorStyles.miniLabel);
                    _interfacesStyle.hover.textColor = _interfaceColor;
                    _interfacesStyle.focused.textColor = _interfaceColor;
                    _interfacesStyle.active.textColor = _interfaceColor;
                    _interfacesStyle.normal.textColor = _interfaceColor;
                    _headerStyle.fontSize = 28;
                }

                GUILayout.Label("[Systems]", _headerStyle);

                DebugMonitorPrefs.instance.isShowInterfaces = EditorGUILayout.Toggle("Show Interfaces", DebugMonitorPrefs.instance.isShowInterfaces);
                DebugMonitorPrefs.instance.isShowHidden = EditorGUILayout.Toggle("Show Hidden", DebugMonitorPrefs.instance.isShowHidden);

                GUILayout.BeginVertical();
                foreach (var item in Target.systems.AllSystems)
                {
                    DrawSystem(item);
                }
                GUILayout.EndVertical();


                GUILayout.Label("[Runners]", _headerStyle);

                GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black));
                foreach (var item in Target.systems.AllRunners)
                {
                    DrawRunner(item.Value);
                }
                GUILayout.EndVertical();
            }

            private void DrawSystem(IEcsSystem system)
            {
                if(system is SystemsBlockMarkerSystem markerSystem)
                {
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black));

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<");
                    GUILayout.Label($"{markerSystem.name}", EditorStyles.boldLabel);
                    GUILayout.Label(">", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                    return;
                }

                Type type = system.GetType();

                if (CheckIsHidden(type))
                    return;

                string name = type.Name;
                Color color = (GetAttribute<DebugColorAttribute>(type) ?? _fakeDebugColorAttribute).GetUnityColor();

                //Color defaultBackgroundColor = GUI.backgroundColor;
                //GUI.backgroundColor = color;
                GUILayout.BeginVertical(EcsEditor.GetStyle(color));
                if (DebugMonitorPrefs.instance.isShowInterfaces)
                {
                    GUILayout.Label(string.Join(", ", type.GetInterfaces().Select(o => o.Name)), _interfacesStyle);
                }
                GUILayout.Label(name, EditorStyles.boldLabel);
                GUILayout.EndVertical();
                //GUI.backgroundColor = defaultBackgroundColor;
            }

            private void DrawRunner(IEcsRunner runner)
            {
                Type type = runner.GetType();
                if (CheckIsHidden(type))
                    return;

                Color color = (GetAttribute<DebugColorAttribute>(type) ?? _fakeDebugColorAttribute).GetUnityColor();
                //Color defaultBackgroundColor = GUI.backgroundColor;
                //GUI.backgroundColor = color;
                GUILayout.BeginVertical(EcsEditor.GetStyle(color));
                //GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(type.Name, EditorStyles.boldLabel);
                GUILayout.Label(string.Join(", ", runner.Targets.Cast<object>().Select(o => o.GetType().Name)));
                GUILayout.EndVertical();
                //GUI.backgroundColor = defaultBackgroundColor;
            }

            private TAttribute GetAttribute<TAttribute>(Type target) where TAttribute : Attribute
            {
                var result = target.GetCustomAttributes(_debugColorAttributeType, false);
                if (result.Length > 0)
                    return (TAttribute)result[0];
                return null;
            }

            private bool CheckIsHidden(Type target)
            {
                if (DebugMonitorPrefs.instance.isShowHidden)
                    return false;

                return target.GetCustomAttribute<DebugHideAttribute>() != null;
            }
        }
    }

#endif
}
