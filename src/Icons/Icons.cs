using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class Icons : Config<Icons>
    {
        //Everything inside #if UNITY_EDITOR is not serialized in the release build
#if UNITY_EDITOR
        [SerializeField]
        private Texture _helpIcon;
        [SerializeField]
        private Texture _closeIcon;
        [SerializeField]
        private Texture _closeIconOn;
        [SerializeField]
        private Texture _unlinkIcon;
        [SerializeField]
        private Texture _auotsetIcon;
        [SerializeField]
        private Texture _auotsetCascadeIcon;
        [SerializeField]
        private Texture _searchIcon;
        [SerializeField]
        private Texture _visibilityIconOn;
        [SerializeField]
        private Texture _visibilityIconOff;
        [SerializeField]
        private Texture _labelIconType;
        [SerializeField]
        private Texture _labelIconMeta;

        internal Texture HelpIcon { get { return _helpIcon; } }
        internal Texture CloseIcon { get { return _closeIcon; } }
        internal Texture CloseIconOn { get { return _closeIconOn; } }
        internal Texture UnlinkIcon { get { return _unlinkIcon; } }
        internal Texture AuotsetIcon { get { return _auotsetIcon; } }
        internal Texture AutosetCascadeIcon { get { return _auotsetCascadeIcon; } }
        internal Texture SearchIcon { get { return _searchIcon; } }
        internal Texture VisibilityIconOn { get { return _visibilityIconOn; } }
        internal Texture VisibilityIconOff { get { return _visibilityIconOff; } }
        internal Texture LabelIconType { get { return _labelIconType; } }
        internal Texture LabelIconMeta { get { return _labelIconMeta; } }
#endif
    }
}
