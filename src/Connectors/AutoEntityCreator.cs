using UnityEngine;

namespace DCFApixels.DragonECS
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

#if UNITY_EDITOR
        internal void Autoset_Editor()
        {
            _connect = GetComponentInChildren<EcsEntityConnect>();
            AutoResolveWorldProviderDependensy();
        }
#endif
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using UnityEditor;

    [CustomEditor(typeof(AutoEntityCreator))]
    [CanEditMultipleObjects]
    public class AutoEntityCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Autoset"))
            {
                foreach (var tr in targets)
                {
                    AutoEntityCreator creator = (AutoEntityCreator)tr;
                    creator.Autoset_Editor();
                    EditorUtility.SetDirty(creator);
                }
            }
        }
    }
}
#endif