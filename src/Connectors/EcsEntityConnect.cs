using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
#region UNITY_EDITOR
using UnityEditor;
using DCFApixels.DragonECS.Unity.Internal;
using DCFApixels.DragonECS.Unity;
#endregion

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

    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(EcsEntityConnect), 30)]
    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, nameof(MonoBehaviour) + ". Responsible for connecting the entity and GameObject using the EcsEntityConnect.ConnectWith method.")]
    public class EcsEntityConnect : MonoBehaviour
    {
        private entlong _entity;
        private EcsWorld _world;

        private static SparseArray<EcsEntityConnect> _connectedEntities = new SparseArray<EcsEntityConnect>();

        [SerializeField]
        private bool _deleteEntiityWithDestroy = false;
        [SerializeField]
        private ScriptableEntityTemplateBase[] _scriptableTemplates;
        [SerializeField]
        private MonoEntityTemplateBase[] _monoTemplates;

        private bool _isConnectInvoked = false;

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
        public IEnumerable<ScriptableEntityTemplateBase> ScriptableTemplates
        {
            get { return _scriptableTemplates; }
        }
        public IEnumerable<MonoEntityTemplateBase> MonoTemplates
        {
            get { return _monoTemplates; }
        }
        public IEnumerable<ITemplate> AllTemplates
        {
            get { return ((IEnumerable<ITemplate>)_scriptableTemplates).Concat(_monoTemplates); }
        }
        #endregion

        #region Connect
        public void ConnectWith(entlong entity, bool applyTemplates)
        {
            Disconnect();

            if (entity.TryUnpack(out int newEntityID, out EcsWorld world))
            {
                _isConnectInvoked = true;
                _entity = entity;
                _world = world;
                _connectedEntities.Add(GetInstanceID(), this);
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
            if (_isConnectInvoked == false)
            {
                return;
            }
            _isConnectInvoked = false;
            if (_world.IsNullOrDetroyed() == false && _entity.TryGetID(out int oldEntityID))
            {
                var unityGameObjects = _world.GetPool<GameObjectConnect>();
                unityGameObjects.TryDel(oldEntityID);
                _connectedEntities.Remove(GetInstanceID());
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
            entlong ent = _entity;
            Disconnect();


            if (_deleteEntiityWithDestroy == false)
            {
                return;
            }
            ent.UnpackUnchecked(out int e, out short gen, out short worldID);
            var world = EcsWorld.GetWorld(worldID);
            if (world != null && world.IsAlive(e, gen))
            {
                world.DelEntity(e);
            }


            //if (_deleteEntiityWithDestroy && ent.TryUnpack(out int id, out EcsWorld world))
            //{
            //    world.DelEntity(id);
            //}
        }
        #endregion

        #region Other
        public static EcsEntityConnect GetConnectByInstanceID(int instanceID)
        {
            return _connectedEntities[instanceID];
        }
        public static bool TryGetConnectByInstanceID(int instanceID, out EcsEntityConnect conncet)
        {
            return _connectedEntities.TryGetValue(instanceID, out conncet);
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
            IEnumerable<MonoEntityTemplateBase> result;
            if (target.MonoTemplates != null && target.MonoTemplates.Count() > 0)
            {
                result = target.MonoTemplates.Where(o => o != null).Union(GetTemplatesFor(target.transform));
            }
            else
            {
                result = GetTemplatesFor(target.transform);
            }

            target._monoTemplates = result.ToArray();
            EditorUtility.SetDirty(target);
        }
        private static IEnumerable<MonoEntityTemplateBase> GetTemplatesFor(Transform parent)
        {
            IEnumerable<MonoEntityTemplateBase> result = parent.GetComponents<MonoEntityTemplateBase>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.TryGetComponent<EcsEntityConnect>(out _))
                {
                    return Enumerable.Empty<MonoEntityTemplateBase>();
                }
                result = result.Concat(GetTemplatesFor(child));
            }
            return result;
        }
#endif
        #endregion
    }
}