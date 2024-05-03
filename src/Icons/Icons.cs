#if UNITY_EDITOR
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public class Icons : ScriptableObject
    {
        private static object _lock = new object();
        private static Icons _instance;
        public static Icons Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = CreateInstance<Icons>();
                    }
                    return _instance;
                }
            }
        }

        [SerializeField]
        internal Texture _helpIcon;
        [SerializeField]
        internal Texture _closeIcon;
        [SerializeField]
        internal Texture _closeIconOn;
        [SerializeField]
        internal Texture _unlinkIcon;
        [SerializeField]
        internal Texture _auotsetIcon;
        [SerializeField]
        internal Texture _auotsetCascadeIcon;
    }
}
#endif