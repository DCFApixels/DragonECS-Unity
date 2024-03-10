using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DisallowMultipleComponent]
    public class EcsEntityConnect : MonoBehaviour
    {
        private sealed class Aspect : EcsAspect
        {
            public EcsPool<UnityGameObjectConnect> unityGameObjects;
            protected override void Init(Builder b)
            {
                unityGameObjects = b.Include<UnityGameObjectConnect>();
            }
        }

        private entlong _entity;
        private EcsWorld _world;

        [SerializeField]
        private ScriptableEntityTemplate[] _scriptableTemplates;
        [SerializeField]
        private MonoEntityTemplate[] _monoTemplates;

        #region Properties
        public entlong Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _entity; }
        }
        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _world; }
        }
        public bool IsConected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _entity.IsAlive; }
        }
        public IEnumerable<ScriptableEntityTemplate> ScriptableTemplates
        {
            get { return _scriptableTemplates; }
        }
        public IEnumerable<MonoEntityTemplate> MonoTemplates
        {
            get { return _monoTemplates; }
        }
        public IEnumerable<ITemplateInternal> AllTemplates
        {
            get { return ((IEnumerable<ITemplateInternal>)_scriptableTemplates).Concat(_monoTemplates); }
        }
        #endregion

        #region Connect
        public void ConnectWith(entlong entity, bool applyTemplates)
        {
            if (_entity.TryGetID(out int oldEntityID) && _world != null)
            {
                var a = _world.GetAspect<Aspect>();
                a.unityGameObjects.TryDel(oldEntityID);
            }
            _world = null;

            if (entity.TryUnpack(out int newEntityID, out EcsWorld world))
            {
                _entity = entity;
                _world = world;
                var a = _world.GetAspect<Aspect>();
                if (a.unityGameObjects.Has(newEntityID) == false)
                {
                    a.unityGameObjects.Add(newEntityID) = new UnityGameObjectConnect(this);
                }
                if (applyTemplates)
                {
                    ApplyTemplatesFor(world.id, newEntityID);
                }
            }
            else
            {
                _entity = entlong.NULL;
            }
        }
        #endregion

        #region ApplyTemplates
        public void ApplyTemplatesFor(short worldID, int entityID)
        {
            foreach (var template in _scriptableTemplates)
            {
                template.Apply(worldID, entityID);
            }
            foreach (var template in _monoTemplates)
            {
                template.Apply(worldID, entityID);
            }
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("Autoset")]
        internal void Autoset_Editor()
        {
            Autoset(this);
        }
        [ContextMenu("Autoset Cascade")]
        internal void AutosetCascade_Editor()
        {
            foreach (var item in GetComponentsInChildren<EcsEntityConnect>())
            {
                Autoset(item);
            }
        }
        [ContextMenu("Unlink Entity")]
        internal void UnlinkEntity_Editor()
        {
            ConnectWith(entlong.NULL, false);
        }
        [ContextMenu("Delete Entity")]
        internal void DeleteEntity_Editor()
        {
            if (_entity.TryUnpack(out int id, out EcsWorld world))
            {
                world.DelEntity(id);
            }
            UnlinkEntity_Editor();
        }

        private static void Autoset(EcsEntityConnect target)
        {
            var result = target.MonoTemplates.Where(o => o != null).Union(GetTemplatesFor(target.transform));

            target._monoTemplates = result.ToArray();
            EditorUtility.SetDirty(target);
        }
        private static IEnumerable<MonoEntityTemplate> GetTemplatesFor(Transform parent)
        {
            IEnumerable<MonoEntityTemplate> result = parent.GetComponents<MonoEntityTemplate>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.TryGetComponent<EcsEntityConnect>(out _))
                {
                    return Enumerable.Empty<MonoEntityTemplate>();
                }
                result = result.Concat(GetTemplatesFor(child));
            }
            return result;
        }
#endif
        #endregion
    }
}