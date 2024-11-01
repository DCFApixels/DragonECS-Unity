#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
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

        private DefineSymbolsInfo[] _defineSymbols = null;
        private void InitDefines()
        {
            string symbolsString = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            _defineSymbols =
                typeof(EcsDefines).GetFields(BindingFlags.Static | BindingFlags.Public).Select(o => new DefineSymbolsInfo(o.Name, false))
                .Concat(typeof(EcsUnityDefines).GetFields(BindingFlags.Static | BindingFlags.Public).Select(o => new DefineSymbolsInfo(o.Name, false)))
                .ToArray();
            for (int i = 0; i < _defineSymbols.Length; i++)
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
            var prefs = UserSettingsPrefs.instance;
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
            GUI.Label(rect, "User Settings", EditorStyles.whiteLargeLabel);

            //using (prefs.DisableAutoSave())
            {
                GUILayout.BeginHorizontal();
                prefs.IsShowHidden = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowHidden)),
                    prefs.IsShowHidden);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsShowInterfaces = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowInterfaces)),
                    prefs.IsShowInterfaces);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsShowRuntimeComponents = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsShowRuntimeComponents)),
                    prefs.IsShowRuntimeComponents);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                prefs.IsUseCustomNames = EditorGUILayout.ToggleLeft(
                    UnityEditorUtility.TransformFieldName(nameof(UserSettingsPrefs.IsUseCustomNames)),
                    prefs.IsUseCustomNames);
                GUILayout.EndHorizontal();

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
            if (_defineSymbols == null)
            {
                InitDefines();
            }
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < _defineSymbols.Length; i++)
            {
                GUILayout.BeginHorizontal();
                var symbol = _defineSymbols[i];

                string text = symbol.name == "DEBUG" ? symbol.name + " (Build Olny)" : symbol.name;
                symbol.isOn = EditorGUILayout.ToggleLeft(text, symbol.isOn);

                GUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) { }
            if (GUILayout.Button("Apply"))
            {
                string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                for (int i = 0; i < _defineSymbols.Length; i++)
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