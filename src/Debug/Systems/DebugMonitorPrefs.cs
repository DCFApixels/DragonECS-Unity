#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity
{
    [FilePath("DragonECS/DebugMonitorPrefs.prefs", FilePathAttribute.Location.ProjectFolder)]
    public class DebugMonitorPrefs : ScriptableSingleton<DebugMonitorPrefs>
    {
        public bool isShowHidden = false;
        public bool isShowInterfaces = false;
    }
}
#endif
