using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class EcsDefaultWorldBuilder
    {
        public static EcsDefaultWorld Build()
        {
            return new EcsDefaultWorld();
        }
    }

    namespace Project.Unity
    {
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
                connect.ConnectWith(world.NewEntityLong());
                connect.ApplyTemplates();
            }
        }
    }
}