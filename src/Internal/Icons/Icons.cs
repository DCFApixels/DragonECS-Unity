using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class Icons : Config<Icons>
    {
        //Everything inside #if UNITY_EDITOR is not serialized in the release build
#if UNITY_EDITOR
        [SerializeField]
        private Texture _helpIcon = null;
        [SerializeField]
        private Texture _fileIcon = null;

        [SerializeField]
        private Texture _errorIcon = null;
        [SerializeField]
        private Texture _warningIcon = null;
        [SerializeField]
        private Texture _passIcon = null;

        [SerializeField]
        private Texture _unlinkIcon = null;
        [SerializeField]
        private Texture _searchIcon = null;
        [SerializeField]
        private Texture _closeIcon = null;
        [SerializeField]
        private Texture _closeIconOn = null;
        [SerializeField]
        private Texture _auotsetIcon = null;
        [SerializeField]
        private Texture _auotsetCascadeIcon = null;
        [SerializeField]
        private Texture _visibilityIconOn = null;
        [SerializeField]
        private Texture _visibilityIconOff = null;
        [SerializeField]
        private Texture _labelIconType = null;
        [SerializeField]
        private Texture _labelIconMeta = null;


        internal Texture HelpIcon { get { return _helpIcon; } }
        internal Texture FileIcon { get { return _fileIcon; } }

        internal Texture ErrorIcon { get { return _errorIcon; } }
        internal Texture WarningIcon { get { return _warningIcon; } }
        internal Texture PassIcon { get { return _passIcon; } }

        internal Texture UnlinkIcon { get { return _unlinkIcon; } }
        internal Texture SearchIcon { get { return _searchIcon; } }
        internal Texture CloseIcon { get { return _closeIcon; } }
        internal Texture CloseIconOn { get { return _closeIconOn; } }
        internal Texture AuotsetIcon { get { return _auotsetIcon; } }
        internal Texture AutosetCascadeIcon { get { return _auotsetCascadeIcon; } }
        internal Texture VisibilityIconOn { get { return _visibilityIconOn; } }
        internal Texture VisibilityIconOff { get { return _visibilityIconOff; } }
        internal Texture LabelIconType { get { return _labelIconType; } }
        internal Texture LabelIconMeta { get { return _labelIconMeta; } }
#endif
    }
}
