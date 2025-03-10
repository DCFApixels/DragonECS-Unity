using DCFApixels.DragonECS.Unity;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static EcsConsts;

    [DisallowMultipleComponent]
    [AddComponentMenu(FRAMEWORK_NAME + "/" + nameof(AutoEntityCreator), 30)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(AUTHOR, nameof(MonoBehaviour) + ". Automatically creates an entity in the selected world and connects it to EcsEntityConnect.")]
    [MetaID("D699B3809201285A46DDF91BCF0540A7")]
    public class AutoEntityCreator : MonoBehaviour
    {
        [SerializeField]
        private EcsEntityConnect _connect;
        [SerializeField]
        private EcsWorldProviderBase _world;

        private bool _started;

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
        private void Start() { ManualStart(); }
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
            if (_started) { return; }
            ManualCreate();
            _started = true;
        }
        public void ManualCreate()
        {
            if (_world == null)
            {
                AutoResolveWorldProviderDependensy();
            }
            InitConnect(_connect, _world.GetRaw());
        }

        private void InitConnect(EcsEntityConnect connect, EcsWorld world)
        {
            if (connect.IsConnected) { return; }
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