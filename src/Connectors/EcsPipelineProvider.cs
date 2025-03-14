#if DISABLE_DEBUG
#undef DEBUG
#endif
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(EcsWorldProvider), menuName = EcsConsts.FRAMEWORK_NAME + "/Providers/" + nameof(EcsWorldProvider), order = 1)]
    public class EcsPipelineProvider : ScriptableObject
    {
        private EcsPipeline _pipeline;
        private static EcsPipelineProvider _singletonInstance;

        #region Properties
        public bool IsEmpty
        {
            get { return _pipeline == null; }
        }
        public static EcsPipelineProvider SingletonInstance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = FindOrCreateSingleton();
                }
                return _singletonInstance;
            }
        }
        #endregion

        #region Methods
        public void Set(EcsPipeline pipeline)
        {
            _pipeline = pipeline;
        }
        public EcsPipeline GetCurrentPipeline()
        {
            return _pipeline;
        }
        protected static EcsPipelineProvider FindOrCreateSingleton()
        {
            return FindOrCreateSingleton(typeof(EcsPipelineProvider).Name + "Singleton");
        }
        protected static EcsPipelineProvider FindOrCreateSingleton(string name)
        {
            EcsPipelineProvider instance = Resources.Load<EcsPipelineProvider>(name);
            if (instance == null)
            {
                instance = CreateInstance<EcsPipelineProvider>();
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
    }
}