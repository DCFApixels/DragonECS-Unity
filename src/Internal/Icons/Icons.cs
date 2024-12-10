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
        private Texture _metaIDIcon = null;
        [SerializeField]
        private Texture _repaireIcon = null;

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


        private Texture2D _dummy;
        private Texture2D _dummyRed;
        private Texture2D _dummyGreen;
        private Texture2D _dummyYellow;

        private Texture2D Dummy
        {
            get { InitDummies(); return _dummy; }
        }
        private Texture2D DummyRed
        {
            get { InitDummies(); return _dummyRed; }
        }
        private Texture2D DummyGreen
        {
            get { InitDummies(); return _dummyGreen; }
        }
        private Texture2D DummyYellow
        {
            get { InitDummies(); return _dummyYellow; }
        }

        private void InitDummies()
        {
            if (_dummy != null) { return; }
            EcsDebug.PrintWarning("Some icons are missing. The issue might be resolved by using \"Assets -> Reimport All\" or by deleting the \"*project_name*/Library\" folder and restarting Unity.");
            Texture2D Create(Color color_)
            {
                Texture2D result_ = new Texture2D(2, 2);
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        result_.SetPixel(i, j, color_);
                    }
                }
                result_.Apply();
                return result_;
            }
            _dummy = Create(Color.white);
            _dummyRed = Create(Color.red);
            _dummyGreen = Create(Color.green);
            _dummyYellow = Create(Color.yellow);
        }

        private static Texture Return(Texture icon, Texture dummy)
        {
            return icon == null ? dummy : icon;
        }

        internal Texture HelpIcon { get { return Return(_helpIcon, Dummy); } }
        internal Texture FileIcon { get { return Return(_fileIcon, Dummy); } }
        internal Texture MetaIDIcon { get { return Return(_metaIDIcon, Dummy); } }
        internal Texture RepaireIcon { get { return Return(_repaireIcon, Dummy); } }

        internal Texture ErrorIcon { get { return Return(_errorIcon, DummyRed); } }
        internal Texture WarningIcon { get { return Return(_warningIcon, DummyYellow); } }
        internal Texture PassIcon { get { return Return(_passIcon, DummyGreen); } }

        internal Texture UnlinkIcon { get { return Return(_unlinkIcon, Dummy); } }
        internal Texture SearchIcon { get { return Return(_searchIcon, Dummy); } }
        internal Texture CloseIcon { get { return Return(_closeIcon, DummyRed); } }
        internal Texture CloseIconOn { get { return Return(_closeIconOn, DummyRed); } }
        internal Texture AuotsetIcon { get { return Return(_auotsetIcon, Dummy); } }
        internal Texture AutosetCascadeIcon { get { return Return(_auotsetCascadeIcon, Dummy); } }
        internal Texture VisibilityIconOn { get { return Return(_visibilityIconOn, Dummy); } }
        internal Texture VisibilityIconOff { get { return Return(_visibilityIconOff, Dummy); } }
        internal Texture LabelIconType { get { return Return(_labelIconType, Dummy); } }
        internal Texture LabelIconMeta { get { return Return(_labelIconMeta, Dummy); } }
#endif
    }
}