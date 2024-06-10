using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(AutoEntityCreator), 30)]
    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsConsts.FRAMEWORK_GROUP, EcsUnityConsts.UNITY_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    public class AutoEntityCreator : MonoBehaviour
    {
        [SerializeField]
        private EcsEntityConnect _connect;
        [SerializeField]
        private EcsWorldProviderBase _world;

        private bool _created;

        #region Properties
        public EcsEntityConnect Connect
        {
            get { return _connect; }
        }
        public EcsWorldProviderBase World
        {
            get { return _world; }
        }
        #endregion

        #region UnityEvents
        private void OnValidate()
        {
            if (_world == null)
            {
                AutoResolveWorldProviderDependensy();
            }
        }
        private void Start()
        {
            CreateEntity();
        }
        #endregion

        #region Methods
        private void AutoResolveWorldProviderDependensy()
        {
            _world = AutoGetWorldProvider();
        }
        protected virtual EcsWorldProviderBase AutoGetWorldProvider()
        {
            return EcsDefaultWorldSingletonProvider.Instance;
        }
        public void ManualStart()
        {
            CreateEntity();
        }
        private void CreateEntity()
        {
            if (_created)
            {
                return;
            }
            if (_world == null)
            {
                AutoResolveWorldProviderDependensy();
            }
            else
            {
                InitConnect(_connect, _world.GetRaw());
            }
            _created = true;
        }

        private void InitConnect(EcsEntityConnect connect, EcsWorld world)
        {
            connect.ConnectWith(CreateEntity(world), true);
        }
        protected virtual entlong CreateEntity(EcsWorld world)
        {
            return world.NewEntityLong();
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("Autoset")]
        internal void Autoset_Editor()
        {
            foreach (var connect in GetComponentsInChildren<EcsEntityConnect>())
            {
                if (connect.GetComponentInParent<AutoEntityCreator>() == this)
                {
                    _connect = connect;
                    AutoResolveWorldProviderDependensy();
                    break;
                }
            }
        }
        [ContextMenu("Autoset Cascade")]
        internal void AutosetCascade_Editor()
        {
            foreach (var target in GetComponentsInChildren<AutoEntityCreator>())
            {
                target.Autoset_Editor();
            }

        }
#endif
        #endregion
    }
}