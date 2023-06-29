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
        public abstract EcsWorld WorldRaw { get; }
        public abstract EcsWorld GetRaw(Func<EcsWorld> builder = null);
    }
    [Serializable]
    public abstract class EcsWorldProvider<TWorld> : EcsWorldProviderBase where TWorld : EcsWorld
    {
        private static TWorld _world;
        public sealed override EcsWorld WorldRaw => _world;
        public override EcsWorld GetRaw(Func<EcsWorld> builder = null)
        {
            if (_world == null || _world.IsDestroyed)
            {
                if (builder != null)
                    _world = (TWorld)builder();
                else
                    _world = (TWorld)Activator.CreateInstance(typeof(TWorld));
                OnWorldCreated(_world);
            }
            return _world;
        }
        public TWorld Get(Func<TWorld> builder = null)
        {
            if (_world == null || _world.IsDestroyed)
            {
                if(builder != null)
                    _world = builder();
                else
                    _world = (TWorld)Activator.CreateInstance(typeof(TWorld));
                OnWorldCreated(_world);
            }
            return _world;
        }
        protected virtual void OnWorldCreated(TWorld world) { }

        protected static TProvider FindOrCreateSingle<TProvider>() where TProvider : EcsWorldProvider<TWorld>
        {
            string name = typeof(TProvider).Name + "Single";
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
    }
}