using DCFApixels.DragonECS.Unity.Debug;
using System.Reflection;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DebugHide, DebugColor(DebugColor.Gray)]
    public class PipelineDebugSystem : IEcsPreInitProcess
    {
        private string _monitorName;
        public PipelineDebugSystem(string monitorName = "Pipeline")
        {
            _monitorName = monitorName;
        }

        void IEcsPreInitProcess.PreInit(EcsPipeline pipeline)
        {
            PipelineDebugMonitor monitor = new GameObject(EcsConsts.DEBUG_PREFIX + _monitorName).AddComponent<PipelineDebugMonitor>();
            monitor.source = this;
            monitor.pipeline = pipeline;
            monitor.monitorName = _monitorName;

            PipelineProcessesDebugMonitor processesMonitor = new GameObject(EcsConsts.DEBUG_PREFIX + "Processes Matrix").AddComponent<PipelineProcessesDebugMonitor>();
            processesMonitor.transform.parent = monitor.transform;
            processesMonitor.source = this;
            processesMonitor.pipeline = pipeline;
            processesMonitor.monitorName = "Processes Matrix";

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

    public class PipelineProcessesDebugMonitor : DebugMonitorBase
    {
        internal PipelineDebugSystem source;
        internal EcsPipeline pipeline;
    }

#if UNITY_EDITOR
    namespace Editors
    {
        using DCFApixels.DragonECS.Internal;
        using DCFApixels.DragonECS.RunnersCore;
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using UnityEditor;

        [CustomEditor(typeof(PipelineDebugMonitor))]
        public class PipelineDebugMonitorEditor : Editor
        {
            private DebugColorAttribute _fakeDebugColorAttribute = new DebugColorAttribute(190, 190, 190);
            private Type _debugColorAttributeType = typeof(DebugColorAttribute);
            private GUIStyle _headerStyle;
            private GUIStyle _interfacesStyle;
            private Color _interfaceColor = new Color(0.96f, 1f, 0.16f);
            private PipelineDebugMonitor Target => (PipelineDebugMonitor)target;


            private GUIStyle systemsListStyle;

            public override void OnInspectorGUI()
            {
                systemsListStyle = new GUIStyle(EditorStyles.miniLabel);
                systemsListStyle.wordWrap = true;

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
                    _interfacesStyle.wordWrap = true;
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

            private void DrawSystem(IEcsProcess system)
            {
                if (system is SystemsLayerMarkerSystem markerSystem)
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

                string name = EcsEditor.GetGenericName(type);
                //Color color = (GetAttribute<DebugColorAttribute>(type) ?? _fakeDebugColorAttribute).GetUnityColor();
                Color color = EcsDebugUtility.GetColorRGB(type).ToUnityColor();

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

                //Color color = (GetAttribute<DebugColorAttribute>(type) ?? _fakeDebugColorAttribute).GetUnityColor();
                Color color = EcsDebugUtility.GetColorRGB(type).ToUnityColor();

                GUILayout.BeginVertical(EcsEditor.GetStyle(color, 0.2f));
                GUILayout.Label(EcsEditor.GetGenericName(type), EditorStyles.boldLabel);
                GUILayout.Label(string.Join(", ", runner.Targets.Cast<object>().Select(o => o.GetType().Name)), systemsListStyle);
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

        [CustomEditor(typeof(PipelineProcessesDebugMonitor))]
        public class PipelineProcessesDebugMonitorEditor : Editor
        {
            private bool _isInit = false;
            private List<ProcessData> _processesList = new List<ProcessData>();
            private Dictionary<Type, int> _processeIndexes = new Dictionary<Type, int>();

            private PipelineProcessesDebugMonitor Target => (PipelineProcessesDebugMonitor)target;
            private Type systemInterfaceType = typeof(IEcsProcess);

            private IEcsProcess[] _systems;
            private void Init()
            {
                if (_isInit)
                    return;
                bool showHidden = DebugMonitorPrefs.instance.IsShowHidden;
                _processesList.Clear();
                _processeIndexes.Clear();
                if (showHidden)
                    _systems = Target.pipeline.AllSystems.Where(o => o is SystemsLayerMarkerSystem == false).ToArray();
                else
                    _systems = Target.pipeline.AllSystems.Where(o => o.GetType().GetCustomAttribute<DebugHideAttribute>() == null).ToArray();

                int i = 0;
                foreach (var system in _systems)
                {
                    foreach (var intr in system.GetType().GetInterfaces())
                    {
                        if(systemInterfaceType.IsAssignableFrom(intr) && systemInterfaceType != intr && (showHidden || intr.GetCustomAttribute<DebugHideAttribute>() == null))
                        {
                            ProcessData data;
                            if (!_processeIndexes.TryGetValue(intr, out int index))
                            {
                                index = _processesList.Count;
                                _processeIndexes.Add(intr, index);

                                data = new ProcessData();
                                _processesList.Add(data);

                                data.name = EcsEditor.GetGenericName(intr);
                                data.interfaceType = intr;
                                data.systemsBitMask = new BitMask(_systems.Length); 
                            }
                            data = _processesList[index];
                            data.systemsBitMask[i] = true;
                        }
                    }
                    i++;
                }

                _isInit = true;
            }
            private Vector2 _position;
            private Vector2 _cellsize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
            private Vector2 _nameCellSize = new Vector2(200f, 200f);

            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();
                DebugMonitorPrefs.instance.IsShowHidden = EditorGUILayout.Toggle("Show Hidden", DebugMonitorPrefs.instance.IsShowHidden);
                if (EditorGUI.EndChangeCheck())
                {
                    _isInit = false;
                }

                Init();

                Rect rect;
                Rect lineRect;
                GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(400f));
                rect = GUILayoutUtility.GetLastRect();

                rect.height = 400f;


                Rect rectView = new Rect(0f, 0f, _nameCellSize.x + _cellsize.x * _processesList.Count, _nameCellSize.y + _cellsize.y * _systems.Length);
                _position = GUI.BeginScrollView(rect, _position, rectView, true, true);

                List<string> systeNames = new List<string>();

                var blackStyle = EcsEditor.GetStyle(Color.black, 0.04f);
                var whiteStyle = EcsEditor.GetStyle(Color.white, 0.04f);
                GUIContent label = new GUIContent();


                Vector2 pivod = _nameCellSize;
                rect = new Rect();
                rect.y = _nameCellSize.y;
                rect.width = _nameCellSize.x;
                rect.height = _cellsize.x;
                    rect.y -= _cellsize.y;
                for (int i = 0; i < _processesList.Count; i++)
                {
                    lineRect = rect;
                    lineRect.y = 0f;
                    lineRect.x = _nameCellSize.x + _cellsize.x * i;
                    lineRect.width = _cellsize.x;
                    lineRect.height = rectView.height;
                    GUI.Label(lineRect, "", i % 2 == 1 ? whiteStyle : blackStyle);

                    GUIUtility.RotateAroundPivot(90, pivod);
                    //GUIContent label = new GUIContent(_processesList[i].name, "." + _processesList[i].name);
                    label.text = _processesList[i].name;
                    label.tooltip = "." + _processesList[i].name;
                    GUI.Label(rect, label, EditorStyles.miniBoldLabel);
                    GUIUtility.RotateAroundPivot(-90, pivod);

                    pivod.x += _cellsize.x;
                    rect.x += _cellsize.x;
                }

                //GUIUtility.RotateAroundPivot(-90, _nameCellSize);
                rect = new Rect();
                rect.y = _nameCellSize.y;
                rect.width = _nameCellSize.x;
                rect.height = _cellsize.x;
                for (int i = 0; i < _systems.Length; i++)
                {
                    string name = EcsEditor.GetGenericName(_systems[i].GetType());
                    systeNames.Add(name);

                    lineRect = rect;
                    lineRect.width = rectView.width;
                    GUI.Label(lineRect, "", i % 2 == 1 ? whiteStyle : blackStyle);

                   // GUIContent label = new GUIContent(name, i + " " + name);
                    label.text = name;
                    label.tooltip = i + " " + name;
                    GUI.Label(rect, label, EditorStyles.miniBoldLabel);
                    rect.y += _cellsize.y;
                }

                for (int x = 0; x < _processesList.Count; x++)
                {
                    var process = _processesList[x];
                    for (int y = 0; y < _systems.Length; y++)
                    {
                        string systemName = systeNames[x];
                        rect = new Rect(x * _cellsize.x + _nameCellSize.x, y * _cellsize.y + _nameCellSize.y, _cellsize.x, _cellsize.y);
                        bool flag = process.systemsBitMask[y];
                        string labeltext = flag ? "^" : " ";
                        label.text = labeltext;
                        label.tooltip = $"{process.name}-{systemName}";
                        GUI.Label(rect, label);
                        //GUI.Label(rect, lable, flag ? whiteStyle : blackStyle);
                        // GUI.Label(rect, label, EditorStyles.helpBox);
                    }
                }

                GUI.EndScrollView();
            }

            private class ProcessData
            {
                public Type interfaceType;
                public string name;
                public BitMask systemsBitMask;
            }
        }
    }
#endif
}
