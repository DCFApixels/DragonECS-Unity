#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal enum RuntimeRefreshMode
    {
        Lazy = 0,
        Always = 1,
    }
    internal enum MetaBlockRectStyle
    {
        Clean = 0,
        Edge = 1,
        Fill = 2,
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
        private bool _showHidden = false;
        public bool ShowHidden
        {
            get => _showHidden;
            set => SetValue(ref _showHidden, value);
        }
        [SerializeField]
        private bool _showInterfaces = false;
        public bool ShowInterfaces
        {
            get => _showInterfaces;
            set => SetValue(ref _showInterfaces, value);
        }
        [SerializeField]
        private bool _showRuntimeComponents = false;
        public bool ShowRuntimeComponents
        {
            get => _showRuntimeComponents;
            set => SetValue(ref _showRuntimeComponents, value);
        }
        [SerializeField]
        private bool _showEntityAdditionalData = false;
        public bool ShowEntityAdditionalData
        {
            get => _showEntityAdditionalData;
            set => SetValue(ref _showEntityAdditionalData, value);
        }
        [SerializeField]
        private bool _useAdvancedInlineInspector = true;
        public bool UseAdvancedInlineInspector
        {
            get => _useAdvancedInlineInspector;
            set => SetValue(ref _useAdvancedInlineInspector, value);
        }
        [SerializeField]
        private bool _useCustomNames = true;
        public bool UseCustomNames
        {
            get => _useCustomNames;
            set => SetValue(ref _useCustomNames, value);
        }
        [SerializeField]
        private bool _pauseOnQuerySnapshot = true;
        public bool PauseOnQuerySnapshot
        {
            get => _pauseOnQuerySnapshot;
            set => SetValue(ref _pauseOnQuerySnapshot, value);
        }

        [SerializeField]
        private RuntimeRefreshMode _runtimeDrawMode = RuntimeRefreshMode.Always;
        public RuntimeRefreshMode RuntimeRefreshMode
        {
            get => _runtimeDrawMode;
            set => SetValue(ref _runtimeDrawMode, value);
        }
        [SerializeField]
        private MetaBlockRectStyle _metaBlockRectStyle = MetaBlockRectStyle.Fill;
        public MetaBlockRectStyle MetaBlockRectStyle
        {
            get => _metaBlockRectStyle;
            set => SetValue(ref _metaBlockRectStyle, value);
        }
        [SerializeField]
        private MetaBlockColorMode _metaBlockColorMode = MetaBlockColorMode.Auto;
        public MetaBlockColorMode MetaBlockColorMode
        {
            get => _metaBlockColorMode;
            set => SetValue(ref _metaBlockColorMode, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetValue<T>(ref T sourceValue, T comingValue)
            where T : struct, Enum
        {
            if (UnsafeUtility.EnumEquals(sourceValue, comingValue) == false)
            {
                sourceValue = comingValue;
                AutoSave();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetValue(ref bool sourceValue, bool comingValue)
        {
            if (sourceValue != comingValue)
            {
                sourceValue = comingValue;
                AutoSave();
            }
        }
        private void AutoSave()
        {
            Save(true);
        }
    }
}
#endif