using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class Icons : Config<Icons>
    {
        //Thank f*cking balls, everything inside #if UNITY_EDITOR is not serialized in the release build
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
        internal Texture HelpIcon { get { return _helpIcon; } }
        internal Texture CloseIcon { get { return _closeIcon; } }
        internal Texture CloseIconOn { get { return _closeIconOn; } }
        internal Texture UnlinkIcon { get { return _unlinkIcon; } }
        internal Texture AuotsetIcon { get { return _auotsetIcon; } }
        internal Texture AutosetCascadeIcon { get { return _auotsetCascadeIcon; } }
#endif
    }
}
