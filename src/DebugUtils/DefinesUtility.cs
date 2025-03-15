#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class DefinesUtility
    {
        public static Symbols[] LoadDefines(Type defineConstsType)
        {
            const BindingFlags REFL_FLAGS = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var fields = defineConstsType.GetFields(REFL_FLAGS).Where(o => o.FieldType == typeof(bool)).Where(o => o.GetCustomAttribute<ObsoleteAttribute>() == null);
            return fields.Select(o => new Symbols(o.Name, (bool)o.GetValue(null))).ToArray();
        }

        public static void ApplyDefines(IEnumerable<Symbols> defines)
        {
            BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
#if UNITY_6000_0_OR_NEWER
            string symbolsString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group));
#else
            string symbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
#endif

            foreach (var define in defines)
            {
                symbolsString = symbolsString.Replace(define.Name, "");
            }

            symbolsString += ";" + string.Join(';', defines.Where(o => o.flag).Select(o => o.Name));
#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group), symbolsString);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbolsString);
#endif
        }

        public class Symbols
        {
            public readonly string Name;
            public bool flag;
            public Symbols(string name, bool flag)
            {
                Name = name;
                this.flag = flag;
            }
        }
    }
}
#endif