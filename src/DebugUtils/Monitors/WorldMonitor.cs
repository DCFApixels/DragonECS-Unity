using DCFApixels.DragonECS.Unity.Editors;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    internal class WorldMonitor : MonoBehaviour
    {
        private EcsWorld _world;
        public EcsWorld World
        {
            get { return _world; }
        }
        public void Set(EcsWorld world)
        {
            _world = world;
        }
    }

    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    internal class WorldMonitorSystem : IEcsInit, IEcsWorldEventListener, IEcsEntityEventListener
    {
        private EcsWorld _world;
        private WorldMonitor _monitor;
        private Transform _entityMonitorsPoolRoot;
        private EntityMonitor[] _entityMonitors;
        public EcsWorld World
        {
            get { return _world; }
        }
        public WorldMonitorSystem(EcsWorld world)
        {
            _world = world;
            _entityMonitors = new EntityMonitor[_world.Capacity];

            _world.AddListener(entityEventListener: this);
            _world.AddListener(worldEventListener: this);
        }
        public void Init()
        {
            if (_world == null)
            {
                return;
            }
            TypeMeta meta = _world.GetMeta();
            _monitor = new GameObject($"{UnityEditorUtility.TransformToUpperName(meta.Name)} ( {_world.id} )").AddComponent<WorldMonitor>();
            UnityEngine.Object.DontDestroyOnLoad(_monitor);
            _monitor.Set(_world);
            _monitor.gameObject.SetActive(false);

            _entityMonitorsPoolRoot = new GameObject("__pool__").transform;
            _entityMonitorsPoolRoot.SetParent(_monitor.transform);


            if (_world.IsNullOrDetroyed() == false)
            {
                foreach (var e in _world.Entities)
                {
                    InitNewEntity(e, false);
                }
            }
        }

        void IEcsWorldEventListener.OnWorldResize(int newSize)
        {
            Array.Resize(ref _entityMonitors, newSize);
        }
        void IEcsWorldEventListener.OnReleaseDelEntityBuffer(ReadOnlySpan<int> buffer) { }
        void IEcsWorldEventListener.OnWorldDestroy()
        {
            if (Application.isPlaying)
            {
                if (_monitor != null)
                {
                    UnityEngine.Object.Destroy(_monitor.gameObject);
                }
                if (_entityMonitorsPoolRoot != null)
                {
                    UnityEngine.Object.Destroy(_entityMonitorsPoolRoot.gameObject);
                }
            }
            _monitor = null;
            _entityMonitorsPoolRoot = null;
        }

        void IEcsEntityEventListener.OnNewEntity(int entityID)
        {
            InitNewEntity(entityID, true);
        }

        private void InitNewEntity(int entityID, bool check)
        {
            if (_monitor == null) { return; }
            ref var _entityMonitorRef = ref _entityMonitors[entityID];
            if (_entityMonitorRef == null)
            {
                _entityMonitorRef = new GameObject($"ENTITY ( {entityID} )").AddComponent<EntityMonitor>();
            }
            if (check && _entityMonitorRef.Entity.IsAlive)
            {
                throw new Exception();
            }
            _entityMonitorRef.Set(_world.GetEntityLong(entityID));
            _entityMonitorRef.transform.SetParent(_monitor.transform);
        }

        void IEcsEntityEventListener.OnDelEntity(int entityID)
        {
            if (_monitor == null) { return; }
            ref var _entityMonitorRef = ref _entityMonitors[entityID];
            if (_entityMonitorRef != null)
            {
                if (_entityMonitorRef.Entity.IsAlive)
                {
                    throw new Exception();
                }
                _entityMonitorRef.transform.SetParent(_entityMonitorsPoolRoot.transform);
                _entityMonitorRef.Set(_world.GetEntityLong(entityID));
            }
        }
    }
}
