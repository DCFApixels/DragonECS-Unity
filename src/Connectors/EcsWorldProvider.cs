using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [Serializable]
    public abstract class EcsWorldProviderBase : ScriptableObject
    {
        public abstract bool IsEmpty { get; }
        public abstract void SetRaw(EcsWorld world);
        public abstract EcsWorld GetRaw();
        public abstract EcsWorld GetCurrentWorldRaw();
    }
    [Serializable]
    public abstract class EcsWorldProvider<TWorld> : EcsWorldProviderBase, IEcsModule
        where TWorld : EcsWorld
    {
        private TWorld _world;

        [Header("Default Configs")]
        [Header("Base")]
        [SerializeField]
        private short _worldID = -1;
        private string _worldName = "";

        [Header("Entites")]
        [SerializeField]
        private int _entitiesCapacity = EcsWorldConfig.Default.EntitiesCapacity;

        [Header("Groups")]
        [SerializeField]
        private int _groupCapacity = EcsWorldConfig.Default.GroupCapacity;

        [Header("Pools/Components")]
        [SerializeField]
        private int _poolsCapacity = EcsWorldConfig.Default.PoolsCapacity;
        [SerializeField]
        private int _poolComponentsCapacity = EcsWorldConfig.Default.PoolComponentsCapacity;
        [SerializeField]
        private int _poolRecycledComponentsCapacity = EcsWorldConfig.Default.PoolRecycledComponentsCapacity;

        #region Properties
        public sealed override bool IsEmpty
        {
            get { return _world == null; }
        }
        public short WorldID
        {
            get { return _worldID; }
        }
        public string WorldName
        {
            get { return _worldName; }
        }
        public int EntitiesCapacity
        {
            get { return _entitiesCapacity; }
        }
        public int GroupCapacity
        {
            get { return _groupCapacity; }
        }
        public int PoolsCapacity
        {
            get { return _poolsCapacity; }
        }
        public int PoolComponentsCapacity
        {
            get { return _poolComponentsCapacity; }
        }
        public int PoolRecycledComponentsCapacity
        {
            get { return _poolRecycledComponentsCapacity; }
        }
        #endregion

        #region Methods
        public sealed override void SetRaw(EcsWorld worldRaw)
        {
            Set((TWorld)worldRaw);
        }
        public void Set(TWorld world)
        {
            _world = world;
        }
        public sealed override EcsWorld GetRaw()
        {
            return Get();
        }
        public sealed override EcsWorld GetCurrentWorldRaw()
        {
            return _world;
        }
        public TWorld Get()
        {
            if (_world == null || _world.IsDestroyed)
            {
                EcsWorldConfig config = new EcsWorldConfig(_entitiesCapacity, _groupCapacity, _poolsCapacity, _poolComponentsCapacity, _poolRecycledComponentsCapacity);
                ConfigContainer configs = new ConfigContainer().Set(config);
                Set(BuildWorld(configs));
                OnWorldCreated(_world);
            }
            return _world;
        }
        protected static TProvider FindOrCreateSingleton<TProvider>() where TProvider : EcsWorldProvider<TWorld>
        {
            return FindOrCreateSingleton<TProvider>(typeof(TProvider).Name + "Singleton");
        }
        protected static TProvider FindOrCreateSingleton<TProvider>(string name) where TProvider : EcsWorldProvider<TWorld>
        {
            TProvider instance = Resources.Load<TProvider>(name);
            if (instance == null)
            {
                instance = CreateInstance<TProvider>();
#if UNITY_EDITOR
                if (AssetDatabase.IsValidFolder("Assets/Resources/") == false)
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/");
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(instance, "Assets/Resources/" + name + ".asset");
                AssetDatabase.Refresh();
#endif
            }
            return instance;
        }
        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            var wolrd = Get();
            b.Inject(wolrd);
            if (b.IsEnableWorldDebug())
            {
                b.AddUnityDebug(wolrd);
            }
        }
        #endregion

        #region Events
        //TODO переименовать в CreateWorld
        protected abstract TWorld BuildWorld(ConfigContainer configs);
        protected virtual void OnWorldCreated(TWorld world) { }
        #endregion
    }
}