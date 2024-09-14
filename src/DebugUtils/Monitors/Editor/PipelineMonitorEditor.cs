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
    internal class PipelineMonitorEditor : ExtendedEditor<PipelineMonitor>
    {
        private GUIStyle _headerStyle;
        private GUIStyle _interfacesStyle;
        private Color _interfaceColor = new Color(0.96f, 1f, 0.16f);

        private GUIStyle systemsListStyle;

        protected override void DrawCustom()
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

            using (EcsGUI.Layout.BeginVertical())
            {
                foreach (var item in Target.Pipeline.AllSystems)
                {
                    DrawSystem(item);
                }
            }


            GUILayout.Label("[Runners]", _headerStyle);

            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                foreach (var item in Target.Pipeline.AllRunners)
                {
                    if (item.Key.IsInterface == false)
                    {
                        DrawRunner(item.Value);
                    }
                }
            }
        }
        private void DrawSystem(IEcsProcess system)
        {
            if (system is SystemsLayerMarkerSystem markerSystem)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));

                using (EcsGUI.Layout.BeginHorizontal()) using (var scope = EcsGUI.SetAlignment(GUI.skin.label))
                {

                    scope.Target.alignment = TextAnchor.UpperLeft;
                    GUILayout.Label("<", GUILayout.ExpandWidth(true));
                    scope.Target.alignment = TextAnchor.UpperRight;
                    using (EcsGUI.SetAlpha(0.64f))
                    {
                        GUILayout.Label($"{markerSystem.layerNameSpace}", GUILayout.ExpandWidth(false));
                    }

                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * -4f);

                    scope.Target.alignment = TextAnchor.UpperLeft;
                    using (EcsGUI.SetFontStyle(scope.Target, FontStyle.Bold))
                    {
                        GUILayout.Label($"{markerSystem.layerName}", GUILayout.ExpandWidth(false));
                    }
                    scope.Target.alignment = TextAnchor.UpperRight;
                    GUILayout.Label(">", GUILayout.ExpandWidth(true));
                }
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


            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(color, 0.2f)))
            {
                if (IsShowInterfaces)
                {
                    GUILayout.Label(string.Join(", ", type.GetInterfaces().Select(o => o.Name)), _interfacesStyle);
                }
                GUILayout.Label(name, EditorStyles.boldLabel);
            }
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

            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(color, 0.2f)))
            {
                GUILayout.Label(meta.Name, EditorStyles.boldLabel);
                GUILayout.Label(string.Join(", ", runner.ProcessRaw.Cast<object>().Select(o => o.GetType().Name)), systemsListStyle);
            }
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