#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public class SettingsEditor : EditorWindow
    {
        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/Settings")]
        static void Open()
        {
            var wnd = GetWindow<SettingsEditor>();
            wnd.titleContent = new GUIContent($"{EcsConsts.FRAMEWORK_NAME} Settings");
            wnd.Show();
        }

        private IEnumerable<DefinesUtility.Symbols> _defineSymbols = null;
        private void InitDefines()
        {
            if (_defineSymbols != null) { return; }
            _defineSymbols = DefinesUtility.LoadDefines(typeof(EcsDefines));
            _defineSymbols = _defineSymbols.Concat(DefinesUtility.LoadDefines(typeof(EcsUnityDefines)));
        }
        private void OnGUI()
        {
            var prefs = UserSettingsPrefs.instance;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;

            //float checkBoxWidth = 20f;

            GUILayout.Space(20f);
            using (new EcsGUI.ColorScope(Color.white * 0.5f))
                GUILayout.BeginVertical(EditorStyles.helpBox);
            //using (new EcsGUI.ColorScope(Color.white * 1.2f))
            GUILayout.Label("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.xMin += 9f;
            GUI.Label(rect, "User Settings", EditorStyles.whiteLargeLabel);

            //using (prefs.DisableAutoSave())
            {
                prefs.IsShowHidden = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowHidden)),
                    prefs.IsShowHidden);

                prefs.IsShowInterfaces = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowInterfaces)),
                    prefs.IsShowInterfaces);

                prefs.IsShowRuntimeComponents = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowRuntimeComponents)),
                    prefs.IsShowRuntimeComponents);

                prefs.IsUseCustomNames = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsUseCustomNames)),
                    prefs.IsUseCustomNames);

                //prefs.IsFastModeRuntimeComponents = EditorGUILayout.ToggleLeft(
                //    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsFastModeRuntimeComponents)),
                //    prefs.IsFastModeRuntimeComponents);

                prefs.ComponentColorMode = (ComponentColorMode)EditorGUILayout.EnumPopup(UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.ComponentColorMode)), prefs.ComponentColorMode);
            }

            GUILayout.EndVertical();

            GUILayout.Space(20f);
            using (EcsGUI.SetColor(Color.white * 0.5f))
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
            }
            //using (new EcsGUI.ColorScope(Color.white * 1.2f))
            GUILayout.Label("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
            rect = GUILayoutUtility.GetLastRect();
            rect.xMin += 9f;
            GUI.Label(rect, "Scripting Define Symbols", EditorStyles.whiteLargeLabel);

            InitDefines();

            EditorGUI.BeginChangeCheck();
            foreach (var symbol in _defineSymbols)
            {
                symbol.flag = EditorGUILayout.ToggleLeft(symbol.Name, symbol.flag);
            }

            if (EditorGUI.EndChangeCheck()) { }
            if (GUILayout.Button("Apply"))
            {
                DefinesUtility.ApplyDefines(_defineSymbols);
                InitDefines();
            }
            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}
#endif