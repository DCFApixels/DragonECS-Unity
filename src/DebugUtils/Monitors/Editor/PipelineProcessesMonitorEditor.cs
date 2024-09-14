#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(PipelineProcessMonitor))]
    internal class PipelineProcessesMonitorEditor : ExtendedEditor<PipelineProcessMonitor>
    {
        private static Type SYSTEM_INTERFACE_TYPE = typeof(IEcsProcess);

        private List<ProcessData> _processList = new List<ProcessData>();
        private Dictionary<Type, int> _processeIndexes = new Dictionary<Type, int>();
        private SystemData[] _systemsList;

        protected override void OnInit()
        {
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
                            data.meta = meta;
                            data.systemsBitMask = new BitMask(_systemsList.Length);
                            _processList.Add(data);
                        }
                        data = _processList[index];
                        data.systemsBitMask[i] = true;
                    }
                }
            }
        }
        private Vector2 _position;
        private Vector2 _cellsize = new Vector2(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
        private Vector2 _nameCellSize = new Vector2(200f, 200f);

        private (TypeMeta system, TypeMeta process) _selectedPointMeta = default;

        protected override void DrawCustom()
        {
            using (EcsGUI.CheckChanged())
            {
                IsShowHidden = EditorGUILayout.Toggle("Show Hidden", IsShowHidden);
                if (EcsGUI.Changed)
                {
                    Init();
                }
            }


            GUILayout.Label("", GUILayout.ExpandWidth(true), GUILayout.Height(400f));
            Rect rect = GUILayoutUtility.GetLastRect();

            rect.height = 400f;


            Rect rectView = new Rect(0f, 0f, _nameCellSize.x + _cellsize.x * _processList.Count, _nameCellSize.y + _cellsize.y * _systemsList.Length);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.6f));
            //GUI.Button(rect, "", EditorStyles.helpBox);

            _position = GUI.BeginScrollView(rect, _position, rectView, true, true);

            Vector2 pivod = _nameCellSize;
            rect = default;
            rect.y = _nameCellSize.y;
            rect.width = _nameCellSize.x;
            rect.height = _cellsize.x;
            rect.y -= _cellsize.y;
            //processes line
            for (int i = 0; i < _processList.Count; i++)
            {
                TypeMeta meta = _processList[i].meta;
                Rect lineRect = rect;
                lineRect.y = 0f;
                lineRect.x = _nameCellSize.x + _cellsize.x * i;
                lineRect.width = _cellsize.x;
                lineRect.height = rectView.height;
                lineRect = RectUtility.AddPadding(lineRect, 1, 0);

                Color color = meta.Color.ToUnityColor();
                color = NormalizeGridColor(i, color);
                EditorGUI.DrawRect(lineRect, color);

                if (EcsGUI.HitTest(lineRect))
                {
                    GUI.Button(lineRect, "", EditorStyles.selectionRect);
                    _selectedPointMeta.process = meta;
                }
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

                Color color = meta.Color.ToUnityColor();
                color = NormalizeGridColor(i, color);
                EditorGUI.DrawRect(lineRect, color);

                if (EcsGUI.HitTest(lineRect))
                {
                    GUI.Button(lineRect, "", EditorStyles.selectionRect);
                    _selectedPointMeta.system = meta;
                }
                GUI.Label(rect, UnityEditorUtility.GetLabel(name, i + " " + name), EditorStyles.miniBoldLabel);
                rect.y += _cellsize.y;
            }

            Texture checkIcon = EditorGUIUtility.IconContent("DotFill").image;

            //matrix
            for (int x = 0; x < _processList.Count; x++)
            {
                var process = _processList[x];
                for (int y = 0; y < _systemsList.Length; y++)
                {
                    rect = new Rect(x * _cellsize.x + _nameCellSize.x, y * _cellsize.y + _nameCellSize.y, _cellsize.x, _cellsize.y);
                    bool flag = process.systemsBitMask[y];
                    if (flag)
                    {
                        GUI.Label(rect, UnityEditorUtility.GetLabel(checkIcon));
                    }
                }
            }
            GUI.EndScrollView();

            Rect r = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
            if (_selectedPointMeta.process != null && _selectedPointMeta.system != null)
            {
                GUI.Label(r, $"{_selectedPointMeta.process.Name}-{_selectedPointMeta.system.Name}");
            }
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
            public TypeMeta meta;
            public BitMask systemsBitMask;
        }
    }
}
#endif