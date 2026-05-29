using DCFApixels.DragonECS.Unity.Editors;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
	[MetaColor(MetaColor.Gray)]
	[MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
	[MetaDescription(EcsConsts.AUTHOR, "...")]
	[MetaTags(MetaTags.HIDDEN)]
	[MetaID("DragonECS_DDE9B6809201381D86DEF36AD06601A9")]
	internal class WorldMonitor : MonoBehaviour
	{
		private EcsWorld _world;
		public EcsWorld World
		{
			get { return _world; }
		}
		public void Set(EcsWorld world)
		{
			_world = world;
#if UNITY_EDITOR
			world.Get<DragonGUI.EntityLinksComponent>().SetWorldMonitor(this);
#endif
		}
	}

	[MetaColor(MetaColor.Gray)]
	[MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
	[MetaDescription(EcsConsts.AUTHOR, "...")]
	[MetaTags(MetaTags.HIDDEN)]
	[MetaID("DragonECS_B5FBB680920179310BEBB305817462B5")]
	internal class WorldMonitorSystem : IEcsInit, IEcsWorldEventListener, IEcsEntityEventListener, IEcsDestroy, IEcsRun
	{
		private EcsWorld _world;
		private WorldMonitor _monitor;
		private WorldQueriesMonitor _queriesMonitor;
		private Transform _entityMonitorsPoolRoot;
		private EntityMonitor[] _entityMonitors;
		private EcsGroup _migratedGroup;
		public EcsWorld World
		{
			get { return _world; }
		}
		public WorldMonitorSystem(EcsWorld world)
		{
			_world = world;
			_entityMonitors = new EntityMonitor[_world.Capacity];

			_world.AddListener(entityEventListener: this);
			_world.AddListener(worldEventListener: this);

			_migratedGroup = EcsGroup.New(_world);
		}
		public void Init()
		{
			Init_Internal();
		}
		private bool _isInit = false;
		public void Init_Internal()
		{
			if (_isInit) { return; }
			_isInit = true;

			if (_world == null)
			{
				return;
			}
			TypeMeta meta = _world.GetMeta();
			_monitor = new GameObject($"{UnityEditorUtility.TransformToUpperName(meta.Name)} ( {_world.ID} )").AddComponent<WorldMonitor>();
			UnityEngine.Object.DontDestroyOnLoad(_monitor);
			_queriesMonitor = _monitor.gameObject.AddComponent<WorldQueriesMonitor>();
			_monitor.Set(_world);
			_queriesMonitor.Set(_world);
			_monitor.gameObject.SetActive(false);

			_entityMonitorsPoolRoot = new GameObject("__pool__").transform;
			_entityMonitorsPoolRoot.SetParent(_monitor.transform);

			_world.EntityMetaChanged -= OnEntityMetaChanged;
			_world.EntityMetaChanged += OnEntityMetaChanged;


			if (_world.IsNullOrDetroyed() == false)
			{
				foreach (var e in _world.Entities)
				{
					InitNewEntity(e, false);
				}
			}
		}

		private void OnEntityMetaChanged(EcsWorld.EntitySlotMeta meta)
		{
			if (_monitor == null) { return; }
			var metaName = meta.Name;
			ref var monitor = ref _entityMonitors[meta.EntityID];
			if (string.IsNullOrEmpty(metaName) == false)
			{
				monitor.SetMetaName(meta.Name);
			}
			monitor.Color = meta.Color.ToUnityColor();
		}

		void IEcsWorldEventListener.OnWorldResize(int newSize)
		{
			Array.Resize(ref _entityMonitors, newSize);
		}
		void IEcsWorldEventListener.OnReleaseDelEntityBuffer(ReadOnlySpan<int> buffer) { }
		void IEcsWorldEventListener.OnWorldDestroy()
		{
			if (Application.isPlaying)
			{
				if (_monitor != null)
				{
					UnityEngine.Object.Destroy(_monitor.gameObject);
				}
				if (_entityMonitorsPoolRoot != null)
				{
					UnityEngine.Object.Destroy(_entityMonitorsPoolRoot.gameObject);
				}
			}
			_monitor = null;
			_entityMonitorsPoolRoot = null;
		}

		void IEcsEntityEventListener.OnNewEntity(int entityID)
		{
			InitNewEntity(entityID, true);
		}

		private void InitNewEntity(int entityID, bool check)
		{
			if (_monitor == null) { return; }
			ref var entityMonitor = ref _entityMonitors[entityID];
			if (entityMonitor == null)
			{
				entityMonitor = new GameObject("").AddComponent<EntityMonitor>();
			}
			if (check && entityMonitor.Entity.IsAlive)
			{
				throw new Exception();
			}
			var ent = _world.GetEntityLong(entityID);
			if(ent != entityMonitor.Entity)
			{
				entityMonitor.Set(ent);
				entityMonitor.transform.SetParent(_monitor.transform);
			}
		}

		void IEcsEntityEventListener.OnDelEntity(int entityID)
		{
			if (_monitor == null) { return; }
			ref var _entityMonitorRef = ref _entityMonitors[entityID];
			if (_entityMonitorRef != null)
			{
				if (_entityMonitorRef.Entity.IsAlive)
				{
					throw new Exception();
				}
				_entityMonitorRef.transform.SetParent(_entityMonitorsPoolRoot.transform);
				_entityMonitorRef.Set(_world.GetEntityLong(entityID));
			}
		}

		public void OnMigrateEntity(int entityID)
		{
			_migratedGroup.Add(entityID);
		}

		public void Destroy()
		{
			_world.EntityMetaChanged -= OnEntityMetaChanged;
		}

		public void Run()
		{
			foreach (var entityID in _migratedGroup)
			{
				var count = _world.GetComponentsCount(entityID);
				ref var monitor = ref _entityMonitors[entityID];
				if (monitor == null) { return; }
				if (count == 1)
				{
					if (monitor.IsDefaultName)
					{
						var id = _world.GetFirstComponentTypeIDFor(entityID);
						var pool = _world.FindPoolInstance(id);
						if (pool != null)
						{
							monitor.SetMetaName(pool.ComponentType.GetMeta().Name);
						}
					}
				}
				else
				{
					var meta = _world.GetEntitySlotMeta(entityID);
					monitor.SetMetaName(meta.Name);
				}
			}
		}
	}
}
