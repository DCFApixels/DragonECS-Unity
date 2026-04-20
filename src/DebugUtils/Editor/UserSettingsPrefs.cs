#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal enum RuntimeDrawMode
    {
        Lazy,
        Live,
    }
    internal enum MetaBlockRectStyle
    {
        Clean,
        Edge,
        Fill,
    }
    internal enum MetaBlockColorMode
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
        [SerializeField]
        private bool _isShowEntityOther = false;
        public bool IsShowEntityOtherData
        {
            get => _isShowEntityOther;
            set
            {
                if (_isShowEntityOther != value)
                {
                    _isShowEntityOther = value;
                    AutoSave();
                }
            }
        }
        [SerializeField]
        private bool _isPauseOnSnapshot = true;
        public bool IsPauseOnSnapshot
        {
            get => _isPauseOnSnapshot;
            set
            {
                if (_isPauseOnSnapshot != value)
                {
                    _isPauseOnSnapshot = value;
                    AutoSave();
                }
            }
        }

        [SerializeField]
        private RuntimeDrawMode _runtimeDrawMode = RuntimeDrawMode.Live;
        public RuntimeDrawMode RuntimeDrawMode
        {
            get => _runtimeDrawMode;
            set
            {
                if (_runtimeDrawMode != value)
                {
                    _runtimeDrawMode = value;
                    AutoSave();
                }
            }
        }


        [SerializeField]
        private MetaBlockRectStyle _metaBlockRectStyle = MetaBlockRectStyle.Edge;
        public MetaBlockRectStyle MetaBlockRectStyle
        {
            get => _metaBlockRectStyle;
            set
            {
                if (_metaBlockRectStyle != value)
                {
                    _metaBlockRectStyle = value;
                    AutoSave();
                }
            }
        }
        [SerializeField]
        private MetaBlockColorMode _metaBlockColorMode = MetaBlockColorMode.Auto;
        public MetaBlockColorMode MetaBlockColorMode
        {
            get => _metaBlockColorMode;
            set
            {
                if (_metaBlockColorMode != value)
                {
                    _metaBlockColorMode = value;
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