using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    public class AutoEntityCreator : MonoBehaviour
    {
        [SerializeField]
        private EcsEntityConnect _connect;
        [SerializeField]
        private EcsWorldProviderBase _world;

        private bool _created;

        #region Properties
        public EcsEntityConnect Connect => _connect;
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
            _world = EcsDefaultWorldSingletonProvider.Instance;
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
            connect.ConnectWith(world.NewEntityLong(), true);
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