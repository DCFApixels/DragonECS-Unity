#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal enum ComponentColorMode
    {
        Generic = 0,
        Auto = 1,
        Rainbow = 2,
    }
    [FilePath(EcsUnityConsts.USER_SETTINGS_FOLDER + "/" + nameof(UserSettingsPrefs) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    internal class UserSettingsPrefs : ScriptableSingleton<UserSettingsPrefs>
    {
        [SerializeField]
        private bool _isUseCustomNames = true;
        public bool IsUseCustomNames
        {
            get => _isUseCustomNames;
            set
            {
                if (_isUseCustomNames != value)
                {
                    _isUseCustomNames = value;
                    AutoSave();
                }
            }
        }
        [SerializeField]
        private bool _isShowInterfaces = false;
        public bool IsShowInterfaces
        {
            get => _isShowInterfaces;
            set
            {
                if (_isShowInterfaces != value)
                {
                    _isShowInterfaces = value;
                    AutoSave();
                }
            }
        }
        [SerializeField]
        private bool _isShowHidden = false;
        public bool IsShowHidden
        {
            get => _isShowHidden;
            set
            {
                if (_isShowHidden != value)
                {
                    _isShowHidden = value;
                    AutoSave();
                }
            }
        }
        [SerializeField]
        private bool _isShowRuntimeComponents = false;
        public bool IsShowRuntimeComponents
        {
            get => _isShowRuntimeComponents;
            set
            {
                if (_isShowRuntimeComponents != value)
                {
                    _isShowRuntimeComponents = value;
                    AutoSave();
                }
            }
        }

        //[SerializeField]
        //private bool _poolsToggle = false;
        //public bool PoolsToggle
        //{
        //    get => _poolsToggle;
        //    set
        //    {
        //        _isChanged = _poolsToggle != value;
        //        _poolsToggle = value;
        //        AutoSave();
        //    }
        //}

        [SerializeField]
        private ComponentColorMode _componentColorMode = ComponentColorMode.Auto;
        public ComponentColorMode ComponentColorMode
        {
            get => _componentColorMode;
            set
            {
                if (_componentColorMode != value)
                {
                    _componentColorMode = value;
                    AutoSave();
                }
            }
        }

        private void AutoSave()
        {
            Save(true);
        }
    }
}
#endif