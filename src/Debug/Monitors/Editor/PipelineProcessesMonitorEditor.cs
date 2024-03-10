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
        private static Type SYSTEM_INTERFACE_TYPE = typeof(IEcsProcess);

        private bool _isInit = false;
        private List<ProcessData> _processList = new List<ProcessData>();
        private Dictionary<Type, int> _processeIndexes = new Dictionary<Type, int>();
        private SystemData[] _systemsList;

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

            _processList.Clear();
            _processeIndexes.Clear();
            IEnumerable<IEcsProcess> fileretSystems = Target.Pipeline.AllSystems;
            if (IsShowHidden)
            { fileretSystems = fileretSystems.Where(o => o is SystemsLayerMarkerSystem == false); }
            else
            { fileretSystems = fileretSystems.Where(o => o.GetMeta().IsHidden == false); }
            _systemsList = fileretSystems.Select(o => new SystemData(o.GetMeta(), o)).ToArray();

            for (int i = 0; i < _systemsList.Length; i++)
            {
                var system = _systemsList[i];
                foreach (var interfaceType in system.meta.Type.GetInterfaces())
                {
                    TypeMeta meta = interfaceType.ToMeta();
                    if (SYSTEM_INTERFACE_TYPE.IsAssignableFrom(interfaceType) && SYSTEM_INTERFACE_TYPE != interfaceType && (IsShowHidden || meta.IsHidden == false))
                    {
                        ProcessData data;
                        if (_processeIndexes.TryGetValue(interfaceType, out int index) == false)
                        {
                            index = _processList.Count;
                            _processeIndexes.Add(interfaceType, index);

                            data = new ProcessData();
                            data.interfaceMeta = meta;
                            data.systemsBitMask = new BitMask(_systemsList.Length);
                            _processList.Add(data);
                        }
                        data = _processList[index];
                        data.systemsBitMask[i] = true;
                    }
                }
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

            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(400f));
            Rect rect = GUILayoutUtility.GetLastRect();

            rect.height = 400f;


            Rect rectView = new Rect(0f, 0f, _nameCellSize.x + _cellsize.x * _processList.Count, _nameCellSize.y + _cellsize.y * _systemsList.Length);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.6f));
            _position = GUI.BeginScrollView(rect, _position, rectView, true, true);
            //var blackStyle = UnityEditorUtility.GetStyle(Color.black, 0.04f);
            //var whiteStyle = UnityEditorUtility.GetStyle(Color.white, 0.04f);

            Vector2 pivod = _nameCellSize;
            rect = default;
            rect.y = _nameCellSize.y;
            rect.width = _nameCellSize.x;
            rect.height = _cellsize.x;
            rect.y -= _cellsize.y;
            //processes line
            for (int i = 0; i < _processList.Count; i++)
            {
                TypeMeta meta = _processList[i].interfaceMeta;
                Rect lineRect = rect;
                lineRect.y = 0f;
                lineRect.x = _nameCellSize.x + _cellsize.x * i;
                lineRect.width = _cellsize.x;
                lineRect.height = rectView.height;
                lineRect = RectUtility.AddPadding(lineRect, 1, 0);
                //GUI.Label(lineRect, "", i % 2 == 1 ? whiteStyle : blackStyle);
                Color color = meta.Color.ToUnityColor();
                color = NormalizeGridColor(i, color);
                EditorGUI.DrawRect(lineRect, color);

                GUIUtility.RotateAroundPivot(90, pivod);
                GUI.Label(rect, UnityEditorUtility.GetLabel(meta.Name), EditorStyles.miniBoldLabel);
                GUIUtility.RotateAroundPivot(-90, pivod);

                pivod.x += _cellsize.x;
                rect.x += _cellsize.x;
            }

            rect = default;
            rect.y = _nameCellSize.y;
            rect.width = _nameCellSize.x;
            rect.height = _cellsize.x;
            //systems line
            for (int i = 0; i < _systemsList.Length; i++)
            {
                TypeMeta meta = _systemsList[i].meta;
                string name = meta.Name;

                Rect lineRect = rect;
                lineRect.width = rectView.width;
                lineRect = RectUtility.AddPadding(lineRect, 0, 1);
                //GUI.Label(lineRect, "", i % 2 == 1 ? whiteStyle : blackStyle);
                Color color = meta.Color.ToUnityColor();
                color = NormalizeGridColor(i, color);
                EditorGUI.DrawRect(lineRect, color);

                GUI.Label(rect, UnityEditorUtility.GetLabel(name, i + " " + name), EditorStyles.miniBoldLabel);
                rect.y += _cellsize.y;
            }

            //matrix
            for (int x = 0; x < _processList.Count; x++)
            {
                var process = _processList[x];
                for (int y = 0; y < _systemsList.Length; y++)
                {
                    rect = new Rect(x * _cellsize.x + _nameCellSize.x, y * _cellsize.y + _nameCellSize.y, _cellsize.x, _cellsize.y);
                    bool flag = process.systemsBitMask[y];
                    string labeltext = flag ? "^" : " ";
                    GUI.Label(rect, UnityEditorUtility.GetLabel(labeltext, $"{process.interfaceMeta.Name}-{_systemsList[x].meta.Name}"));
                }
            }
            GUI.EndScrollView();
        }

        private static Color NormalizeGridColor(int index, Color color)
        {
            if (index % 2 == 1)
            {
                color = color / 1.4f;
                color.a = 0.3f;
            }
            else
            {
                color.a = 0.5f;
            }
            return color;
        }
        private class SystemData
        {
            public TypeMeta meta;
            public IEcsProcess system;
            public SystemData(TypeMeta meta, IEcsProcess system)
            {
                this.meta = meta;
                this.system = system;
            }
        }
        private class ProcessData
        {
            public TypeMeta interfaceMeta;
            public BitMask systemsBitMask;
        }
    }
}
#endif