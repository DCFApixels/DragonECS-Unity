using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public static class EcsConnect
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this Component cmp, entlong entity, bool applyTemplates)
        {
            Connect(entity, cmp, applyTemplates);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this entlong entity, Component cmp, bool applyTemplates)
        {
            if (cmp.TryGetComponent(out EcsEntityConnect connect) == false)
            {
                connect = cmp.gameObject.AddComponent<EcsEntityConnect>();
            }
            connect.ConnectWith(entity, applyTemplates);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this GameObject go, entlong entity, bool applyTemplates)
        {
            Connect(entity, go, applyTemplates);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this entlong entity, GameObject go, bool applyTemplates)
        {
            if (go.TryGetComponent(out EcsEntityConnect connect) == false)
            {
                connect = go.AddComponent<EcsEntityConnect>();
            }
            connect.ConnectWith(entity, applyTemplates);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this EcsEntityConnect connect, entlong entity, bool applyTemplates)
        {
            Connect(entity, connect, applyTemplates);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Connect(this entlong entity, EcsEntityConnect connect, bool applyTemplates)
        {
            connect.ConnectWith(entity, applyTemplates);
        }
    }

    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsEntityConnect), 30)]
    public class EcsEntityConnect : MonoBehaviour
    {
        private entlong _entity;
        private EcsWorld _world;

        [SerializeField]
        private ScriptableEntityTemplate[] _scriptableTemplates;
        [SerializeField]
        private MonoEntityTemplate[] _monoTemplates;

        private bool _isConnected = false;

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
        public bool IsConnected
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
            Disconnect();

            if (entity.TryUnpack(out int newEntityID, out EcsWorld world))
            {
                _isConnected = true;
                _entity = entity;
                _world = world;
                var goConnects = world.GetPool<GameObjectConnect>();
                if (goConnects.Has(newEntityID))
                {
                    ref readonly var goConnect = ref goConnects.Read(newEntityID);
                    if (goConnect.IsConnected)
                    {
                        goConnect.Connect.Disconnect();
                    }
                }

                goConnects.TryAddOrGet(newEntityID) = new GameObjectConnect(this);
                if (applyTemplates)
                {
                    ApplyTemplatesFor(world.id, newEntityID);
                }
            }
        }
        public void Disconnect()
        {
            if (_isConnected == false)
            {
                return;
            }
            _isConnected = false;
            if (_entity.TryGetID(out int oldEntityID) && _world != null)
            {
                var unityGameObjects = _world.GetPool<GameObjectConnect>();
                unityGameObjects.TryDel(oldEntityID);
            }
            _world = null;
            _entity = entlong.NULL;
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

        #region UnityEvents
        private void OnDestroy()
        {
            Disconnect();
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