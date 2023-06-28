using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class EcsEntityConnect : MonoBehaviour
    {
        private sealed class Aspect : EcsAspect
        {
            public readonly EcsPool<UnityGameObject> unityGameObjects;
            public Aspect(Builder b)
            {
                unityGameObjects = b.Include<UnityGameObject>();
            }
        }

        private entlong _entity;
        private EcsWorld _world;

        [SerializeField]
        private EntityTemplatePreset[] _entityTemplatePresets;
        [SerializeField]
        private EntityTemplate[] _entityTemplates;

        internal void SetTemplates_Editor(EntityTemplate[] tempaltes)
        {
            _entityTemplates = tempaltes;
        }

        #region Properties
        public entlong Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity;
        }
        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _world;
        }
        public bool IsAlive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity.IsAlive;
        }
        #endregion

        public void ConnectWith(entlong entity, bool applyTemplates = false)
        {
            if (_entity.TryGetID(out int oldE) && _world != null)
            {
                var s = _world.GetAspect<Aspect>();
                s.unityGameObjects.Del(oldE);
            }
            _world = null;

            if (entity.TryGetID(out int newE))
            {
                _entity = entity;
                _world = _entity.World;
                var s = _world.GetAspect<Aspect>();
                if (!s.unityGameObjects.Has(newE)) s.unityGameObjects.Add(newE) = new UnityGameObject(gameObject);

                if (applyTemplates)
                    ApplyTemplates();
            }
            else
            {
                _entity = entlong.NULL;
            }
        }
        public void ApplyTemplates() => ApplyTemplatesFor(_entity.ID);
        public void ApplyTemplatesFor(int entityID)
        {
            foreach (var t in _entityTemplatePresets)
                t.Apply(_world, entityID);
            foreach (var t in _entityTemplates)
                t.Apply(_world, entityID);
        }
    }
}



#if UNITY_EDITOR

namespace DCFApixels.DragonECS.Editors
{
    using System.Collections.Generic;
    using UnityEditor;

    [CustomEditor(typeof(EcsEntityConnect))]
    public class EcsEntityEditor : Editor
    {
        private EcsEntityConnect Target => (EcsEntityConnect)target;
        private GUIStyle _greenStyle;
        private GUIStyle _redStyle;

        private bool _isInit = false;

        private void Init()
        {
            if (_isInit)
                return;

            _greenStyle = EcsEditor.GetStyle(new Color32(75, 255, 0, 100));
            _redStyle = EcsEditor.GetStyle(new Color32(255, 0, 75, 100));

            _isInit = true;
        }

        public override void OnInspectorGUI()
        {
            Init();
            if (Target.IsAlive)
                GUILayout.Box("Connected", _greenStyle, GUILayout.ExpandWidth(true));
            else
                GUILayout.Box("Not connected", _redStyle, GUILayout.ExpandWidth(true));

            if (Target.Entity.TryGetID(out int id))
                EditorGUILayout.IntField(id);
            else
                EditorGUILayout.IntField(0);
            GUILayout.Label(Target.Entity.ToString());

            base.OnInspectorGUI();

            if (GUILayout.Button("Autoset Templates"))
            {
                Target.SetTemplates_Editor(Target.GetComponents<EntityTemplate>());

                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Autoset Templates Cascade"))
            {
                foreach (var item in Target.GetComponentsInChildren<EcsEntityConnect>())
                {
                    item.SetTemplates_Editor(item.GetComponents<EntityTemplate>());
                    EditorUtility.SetDirty(item);
                }
            }

            if (Target.IsAlive)
            {
                List<object> comps = new List<object>();
                Target.World.GetComponents(Target.Entity.ID, comps);
                GUILayout.TextArea(string.Join("\r\n", comps));
            }
        }
    }
}
#endif