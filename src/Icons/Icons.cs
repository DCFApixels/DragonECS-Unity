#if UNITY_EDITOR
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public class Icons : Config<Icons>
    {
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
    }
}
#endif