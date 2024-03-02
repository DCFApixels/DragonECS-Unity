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
    }
    [Serializable]
    public abstract class EcsWorldProvider<TWorld> : EcsWorldProviderBase where TWorld : EcsWorld
    {
        private TWorld _world;

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
            return (TWorld)Activator.CreateInstance(typeof(TWorld), new object[] { null, -1 });
        }
        protected virtual void OnWorldCreated(TWorld world) { }
        #endregion
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsWorldProviderBase), true)]
    [CanEditMultipleObjects]
    public class EcsWorldProviderBaseEditor : Editor
    {
        private EcsWorldProviderBase Target => (EcsWorldProviderBase)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Target.IsEmpty)
            {
                var style = EcsEditor.GetStyle(new Color32(255, 0, 75, 100));
                GUILayout.Box("Is Empty", style, GUILayout.ExpandWidth(true));
            }
            else
            {
                var style = EcsEditor.GetStyle(new Color32(75, 255, 0, 100));
                EcsWorld world = Target.GetRaw();
                GUILayout.Box($"{world.GetMeta().Name} ( {world.id} )", style, GUILayout.ExpandWidth(true));
            }
        }
    }
}
#endif