#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
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

        private List<DefineSymbolsInfo> _defineSymbols = null;
        private void InitDefines()
        {
            string symbolsString = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            _defineSymbols = new List<DefineSymbolsInfo>()
            {
                nameof(EcsConsts.DISABLE_POOLS_EVENTS),
                nameof(EcsConsts.ENABLE_DRAGONECS_DEBUGGER),
                nameof(EcsConsts.ENABLE_DRAGONECS_ASSERT_CHEKS),
                nameof(EcsConsts.REFLECTION_DISABLED),
                nameof(EcsConsts.DISABLE_DEBUG),
                nameof(EcsConsts.ENABLE_DUMMY_SPAN),
                nameof(EcsConsts.DISABLE_CATH_EXCEPTIONS),
            };
            for (int i = 0; i < _defineSymbols.Count; i++)
            {
                var symbol = _defineSymbols[i];
                if (symbolsString.Contains(symbol.name))
                {
                    symbol.isOn = true;
                }
            }
        }
        private void OnGUI()
        {
            var prefs = SettingsPrefs.instance;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;

            float checkBoxWidth = 20f;

            GUILayout.Space(20f);
            using (new EcsGUI.ColorScope(Color.white * 0.5f))
                GUILayout.BeginVertical(EditorStyles.helpBox);
            //using (new EcsGUI.ColorScope(Color.white * 1.2f))
            GUILayout.Label("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.xMin += 9f;
            GUI.Label(rect, "Settings", EditorStyles.whiteLargeLabel);

            //using (prefs.DisableAutoSave())
            {
                GUILayout.BeginHorizontal();
                prefs.IsShowHidden = EditorGUILayout.Toggle(prefs.IsShowHidden, GUILayout.Width(checkBoxWidth));
                GUILayout.Label(UnityEditorUtility.TransformFieldName(nameof(SettingsPrefs.IsShowHidden)), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsShowInterfaces = EditorGUILayout.Toggle(prefs.IsShowInterfaces, GUILayout.Width(checkBoxWidth));
                GUILayout.Label(UnityEditorUtility.TransformFieldName(nameof(SettingsPrefs.IsShowInterfaces)), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsShowRuntimeComponents = EditorGUILayout.Toggle(prefs.IsShowRuntimeComponents, GUILayout.Width(checkBoxWidth));
                GUILayout.Label(UnityEditorUtility.TransformFieldName(nameof(SettingsPrefs.IsShowRuntimeComponents)), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsUseCustomNames = EditorGUILayout.Toggle(prefs.IsUseCustomNames, GUILayout.Width(checkBoxWidth));
                GUILayout.Label(UnityEditorUtility.TransformFieldName(nameof(SettingsPrefs.IsUseCustomNames)), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                prefs.ComponentColorMode = (ComponentColorMode)EditorGUILayout.EnumPopup(UnityEditorUtility.TransformFieldName(nameof(SettingsPrefs.ComponentColorMode)), prefs.ComponentColorMode);
            }

            GUILayout.EndVertical();

            GUILayout.Space(20f);
            using (EcsGUI.SetColor(Color.white * 0.5f))
                GUILayout.BeginVertical(EditorStyles.helpBox);
            //using (new EcsGUI.ColorScope(Color.white * 1.2f))
            GUILayout.Label("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
            rect = GUILayoutUtility.GetLastRect();
            rect.xMin += 9f;
            GUI.Label(rect, "Scripting Define Symbols", EditorStyles.whiteLargeLabel);
            if (_defineSymbols == null)
            {
                InitDefines();
            }
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < _defineSymbols.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var symbol = _defineSymbols[i];
                symbol.isOn = EditorGUILayout.Toggle(symbol.isOn, GUILayout.Width(checkBoxWidth));
                if (symbol.name == "DEBUG")
                {
                    GUILayout.Label(symbol.name + " (Build Olny)", GUILayout.ExpandWidth(false));
                }
                else
                {
                    GUILayout.Label(symbol.name, GUILayout.ExpandWidth(false));
                }
                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) { }
            if (GUILayout.Button("Apply"))
            {
                string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                for (int i = 0; i < _defineSymbols.Count; i++)
                {
                    var symbol = _defineSymbols[i];
                    symbolsString = symbolsString.Replace(symbol.name, "");
                }
                symbolsString += ";" + string.Join(';', _defineSymbols.Where(o => o.isOn).Select(o => o.name));
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbolsString);
                InitDefines();
            }
            GUILayout.EndVertical();

            EditorGUIUtility.labelWidth = labelWidth;
        }
        private class DefineSymbolsInfo
        {
            public string name;
            public bool isOn;
            public DefineSymbolsInfo(string name, bool isOn)
            {
                this.name = name;
                this.isOn = isOn;
            }
            public static implicit operator DefineSymbolsInfo(string a) => new DefineSymbolsInfo(a, false);
        }
    }
}
#endif