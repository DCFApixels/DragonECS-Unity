#if UNITY_EDITOR
using UnityEditor;

namespace DCFApixels.DragonECS.Editors
{
    [FilePath("DragonECS/DebugMonitorPrefs.prefs", FilePathAttribute.Location.ProjectFolder)]
    public class DebugMonitorPrefs : ScriptableSingleton<DebugMonitorPrefs>
    {
        private bool _isShowHidden = false;
        public bool _isShowInterfaces = false;

        public bool IsShowHidden
        {
            get => IsShowHidden1; set
            {
                IsShowHidden1 = value;
                Save(false);
            }
        }

        public bool IsShowHidden1
        {
            get => _isShowHidden; set
            {
                _isShowHidden = value;
                Save(false);
            }
        }
    }
}
#endif
