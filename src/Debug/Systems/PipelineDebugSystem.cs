using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Debug;
using System.Reflection;
using UnityEngine;


namespace DCFApixels.DragonECS
{
    [DebugHide, DebugColor(DebugColor.Gray)]
    public class PipelineDebugSystem : IEcsPreInitSystem
    {
        private string _monitorName;
        public PipelineDebugSystem(string monitorName = "Pipeline")
        {
            _monitorName = monitorName;
        }

        void IEcsPreInitSystem.PreInit(EcsPipeline pipeline)
        {
            PipelineDebugMonitor monitor = new GameObject(EcsConsts.DEBUG_PREFIX + _monitorName).AddComponent<PipelineDebugMonitor>();
            monitor.source = this;
            monitor.pipeline = pipeline;
            monitor.monitorName = _monitorName;

            //foreach (var item in pipeline.AllSystems) //Вырезано пока не сделаю TODO в SystemDebugMonitor
            //{
            //    DebugNameAttribute debugName = item.GetType().GetCustomAttribute<DebugNameAttribute>();
            //    string name = debugName == null ? item.GetType().Name : debugName.name;
            //    SystemDebugMonitor.CreateMonitor(monitor.transform, item, name);
            //}
        }
    }

    public class PipelineDebugMonitor : DebugMonitorBase
    {
        internal PipelineDebugSystem source;
        internal EcsPipeline pipeline;
    }

#if UNITY_EDITOR
    namespace Editors
    {
        using DCFApixels.DragonECS.RunnersCore;
        using System;
        using System.Linq;
        using UnityEditor;

        [CustomEditor(typeof(PipelineDebugMonitor))]
        public class PipelineDebugMonitorEditor : Editor
        {
            private DebugColorAttribute _fakeDebugColorAttribute = new DebugColorAttribute(DebugColor.White);
            private Type _debugColorAttributeType = typeof(DebugColorAttribute);
            private GUIStyle _headerStyle;
            private GUIStyle _interfacesStyle;
            private Color _interfaceColor = new Color(0.96f, 1f, 0.16f);
            private PipelineDebugMonitor Target => (PipelineDebugMonitor)target;
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

                DebugMonitorPrefs.instance.IsShowInterfaces = EditorGUILayout.Toggle("Show Interfaces", DebugMonitorPrefs.instance.IsShowInterfaces);
                DebugMonitorPrefs.instance.IsShowHidden = EditorGUILayout.Toggle("Show Hidden", DebugMonitorPrefs.instance.IsShowHidden);

                GUILayout.BeginVertical();
                foreach (var item in Target.pipeline.AllSystems)
                {
                    DrawSystem(item);
                }
                GUILayout.EndVertical();


                GUILayout.Label("[Runners]", _headerStyle);

                GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black, 0.2f));
                foreach (var item in Target.pipeline.AllRunners)
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
                    GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black, 0.2f));

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

                GUILayout.BeginVertical(EcsEditor.GetStyle(color, 0.2f));
                if (DebugMonitorPrefs.instance.IsShowInterfaces)
                {
                    GUILayout.Label(string.Join(", ", type.GetInterfaces().Select(o => o.Name)), _interfacesStyle);
                }
                GUILayout.Label(name, EditorStyles.boldLabel);
                GUILayout.EndVertical();
            }

            private void DrawRunner(IEcsRunner runner)
            {
                Type type = runner.GetType();
                if (CheckIsHidden(type))
                    return;

                Color color = (GetAttribute<DebugColorAttribute>(type) ?? _fakeDebugColorAttribute).GetUnityColor();
                GUILayout.BeginVertical(EcsEditor.GetStyle(color, 0.2f));
                GUILayout.Label(type.Name, EditorStyles.boldLabel);
                GUILayout.Label(string.Join(", ", runner.Targets.Cast<object>().Select(o => o.GetType().Name)), EditorStyles.miniLabel);
                GUILayout.EndVertical();
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
                if (DebugMonitorPrefs.instance.IsShowHidden)
                    return false;

                return target.GetCustomAttribute<DebugHideAttribute>() != null;
            }
        }
    }

#endif
}
