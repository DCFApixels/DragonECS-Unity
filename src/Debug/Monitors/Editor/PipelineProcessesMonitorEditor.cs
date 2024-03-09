#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(PipelineProcessMonitor))]
    internal class PipelineProcessesMonitorEditor : Editor
    {
        private bool _isInit = false;
        private List<ProcessData> _processesList = new List<ProcessData>();
        private Dictionary<Type, int> _processeIndexes = new Dictionary<Type, int>();
        private Type systemInterfaceType = typeof(IEcsProcess);
        private IEcsProcess[] _systems;

        private PipelineProcessMonitor Target => (PipelineProcessMonitor)target;
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

        private void Init()
        {
            if (_isInit)
            {
                return;
            }

            _processesList.Clear();
            _processeIndexes.Clear();
            if (IsShowHidden)
            {
                _systems = Target.Pipeline.AllSystems.Where(o => o is SystemsLayerMarkerSystem == false).ToArray();
            }
            else
            {
                _systems = Target.Pipeline.AllSystems.Where(o => o.GetMeta().IsHidden == false).ToArray();
            }

            int i = 0;
            foreach (var system in _systems)
            {
                foreach (var interfaceType in system.GetType().GetInterfaces())
                {
                    TypeMeta meta = interfaceType.ToMeta();
                    if (systemInterfaceType.IsAssignableFrom(interfaceType) && systemInterfaceType != interfaceType && (IsShowHidden || meta.IsHidden == false))
                    {
                        ProcessData data;
                        if (!_processeIndexes.TryGetValue(interfaceType, out int index))
                        {
                            index = _processesList.Count;
                            _processeIndexes.Add(interfaceType, index);

                            data = new ProcessData();
                            _processesList.Add(data);

                            data.name = meta.Name;
                            data.interfaceType = interfaceType;
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
            IsShowHidden = EditorGUILayout.Toggle("Show Hidden", IsShowHidden);
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

            var blackStyle = UnityEditorUtility.GetStyle(Color.black, 0.04f);
            var whiteStyle = UnityEditorUtility.GetStyle(Color.white, 0.04f);
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
                label.text = _processesList[i].name;
                label.tooltip = "." + _processesList[i].name;
                GUI.Label(rect, label, EditorStyles.miniBoldLabel);
                GUIUtility.RotateAroundPivot(-90, pivod);

                pivod.x += _cellsize.x;
                rect.x += _cellsize.x;
            }

            rect = new Rect();
            rect.y = _nameCellSize.y;
            rect.width = _nameCellSize.x;
            rect.height = _cellsize.x;
            for (int i = 0; i < _systems.Length; i++)
            {
                TypeMeta meta = _systems[i].GetMeta();
                string name = meta.Name;
                systeNames.Add(name);

                lineRect = rect;
                lineRect.width = rectView.width;
                GUI.Label(lineRect, "", i % 2 == 1 ? whiteStyle : blackStyle);

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