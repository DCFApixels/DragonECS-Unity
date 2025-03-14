using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EcsRootUnity))]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsRootUnitySingleton), 30)]
    public class EcsRootUnitySingleton : MonoBehaviour
    {
        [SerializeField]
        private EcsRootUnity _root;
        [SerializeField]
        private bool _dontDestroyOnLoad = true;

        private static EcsRootUnitySingleton _singletonInstance;
        public static EcsRootUnity Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
#if UNITY_6000_0_OR_NEWER
                    _singletonInstance = FindFirstObjectByType<EcsRootUnitySingleton>();
#else
                    _singletonInstance = FindObjectOfType<EcsRootUnitySingleton>();
#endif
                }
                return _singletonInstance?._root;
            }
        }
        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        private void OnValidate()
        {
            _root = GetComponent<EcsRootUnity>();
        }
    }
}
