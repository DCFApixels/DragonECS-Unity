﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [FilePath(EcsConsts.FRAMEWORK_NAME + "/" + nameof(DebugMonitorPrefs) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    public class DebugMonitorPrefs : ScriptableSingleton<DebugMonitorPrefs>
    {
        [SerializeField]
        private bool _isShowInterfaces = false;
        public bool IsShowInterfaces
        {
            get => _isShowInterfaces;
            set
            {
                _isShowInterfaces = value;
                Save(false);
            }
        }
        [SerializeField]
        private bool _isShowHidden = false;
        public bool IsShowHidden
        {
            get => _isShowHidden; 
            set
            {
                _isShowHidden = value;
                Save(false);
            }
        }
        [SerializeField]
        private bool _isShowRuntimeComponents = true;
        public bool IsShowRuntimeComponents
        {
            get => _isShowRuntimeComponents;
            set
            {
                _isShowRuntimeComponents = value;
                Save(false);
            }
        }

        [SerializeField]
        private bool _poolsToggle = false;
        public bool PoolsToggle
        {
            get => _poolsToggle;
            set
            {
                _poolsToggle = value;
                Save(false);
            }
        }
    }
}
#endif
