#if UNITY_EDITOR
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

        private List<DefineSymbolsInfo> _defineSymbols = null;
        private void InitDefines()
        {
            string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            _defineSymbols = new List<DefineSymbolsInfo>()
            {
                nameof(EcsConsts.ENABLE_DRAGONECS_DEBUGGER),
                nameof(EcsConsts.ENABLE_DRAGONECS_ASSERT_CHEKS),
                nameof(EcsConsts.REFLECTION_DISABLED),
                nameof(EcsConsts.DISABLE_DEBUG),
                nameof(EcsConsts.ENABLE_DUMMY_SPAN),
                nameof(EcsConsts.DISABLE_CATH_EXCEPTIONS),
                nameof(EcsConsts.DISABLE_DRAGONECS_DEBUGGER),
                "DEBUG",
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
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth *= 2f;


            GUILayout.Label("Settings", EditorStyles.whiteLargeLabel);
            EditorGUI.BeginChangeCheck();
            Settings settings = new Settings();
            settings.IsShowHidden = EditorGUILayout.Toggle(nameof(SettingsPrefs.IsShowHidden), SettingsPrefs.instance.IsShowHidden);
            settings.IsShowInterfaces = EditorGUILayout.Toggle(nameof(SettingsPrefs.IsShowInterfaces), SettingsPrefs.instance.IsShowInterfaces);
            settings.IsShowRuntimeComponents = EditorGUILayout.Toggle(nameof(SettingsPrefs.IsShowRuntimeComponents), SettingsPrefs.instance.IsShowRuntimeComponents);
            if (EditorGUI.EndChangeCheck())
            {
                SettingsPrefs.instance.IsShowHidden = settings.IsShowHidden;
                SettingsPrefs.instance.IsShowInterfaces = settings.IsShowInterfaces;
                SettingsPrefs.instance.IsShowRuntimeComponents = settings.IsShowRuntimeComponents;
            }
            GUILayout.Label("Scripting Define Symbols", EditorStyles.whiteLargeLabel);
            if (_defineSymbols == null)
            {
                InitDefines();
            }
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < _defineSymbols.Count; i++)
            {
                var symbol = _defineSymbols[i];
                symbol.isOn = EditorGUILayout.Toggle(symbol.name, symbol.isOn);
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
        private struct Settings
        {
            public bool IsShowHidden;
            public bool IsShowInterfaces;
            public bool IsShowRuntimeComponents;
        }
    }
}
#endif