﻿using System;
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
    }
    [Serializable]
    public abstract class EcsWorldProvider<TWorld> : EcsWorldProviderBase where TWorld : EcsWorld
    {
        private TWorld _world;

        [SerializeField]
        public short _worldID = -1;

        [Header("Default Configs")]
        [Header("Entites")]
        [SerializeField]
        private int EntitiesCapacity = EcsWorldConfig.Default.EntitiesCapacity;

        [Header("Groups")]
        [SerializeField]
        private int GroupCapacity = EcsWorldConfig.Default.GroupCapacity;

        [Header("Pools/Components")]
        [SerializeField]
        private int PoolsCapacity = EcsWorldConfig.Default.PoolsCapacity;
        [SerializeField]
        private int PoolComponentsCapacity = EcsWorldConfig.Default.PoolComponentsCapacity;
        [SerializeField]
        private int PoolRecycledComponentsCapacity = EcsWorldConfig.Default.PoolRecycledComponentsCapacity;

        #region Properties
        public sealed override bool IsEmpty
        {
            get { return _world == null; }
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
        public TWorld Get()
        {
            if (_world == null || _world.IsDestroyed)
            {
                _world = BuildWorld();
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
        #endregion

        #region Events
        protected virtual TWorld BuildWorld()
        {
            EcsWorldConfig config = new EcsWorldConfig(EntitiesCapacity, GroupCapacity, PoolsCapacity, PoolComponentsCapacity, PoolRecycledComponentsCapacity);
            ConfigContainer configs = new ConfigContainer().Set(config);
            return (TWorld)Activator.CreateInstance(typeof(TWorld), new object[] { configs, _worldID });
        }
        protected virtual void OnWorldCreated(TWorld world) { }
        #endregion
    }
}