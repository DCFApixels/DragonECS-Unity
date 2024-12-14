using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public abstract class MonoEntityTemplateBase : MonoBehaviour, IEntityTemplate
    {
        public abstract void Apply(short worldID, int entityID);
    }

    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(MonoEntityTemplate), 30)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, nameof(MonoBehaviour) + " implementation of an entity template. Templates are a set of components that are applied to entities.")]
    [MetaID("C734BA8092014833C14F21E05D7B1551")]
    public class MonoEntityTemplate : MonoEntityTemplateBase, IEntityTemplateInternal
    {
        [SerializeReference]
        [ReferenceButton(true, typeof(IComponentTemplate))]
        private IComponentTemplate[] _components;

        #region Properties
        string IEntityTemplateInternal.ComponentsPropertyName
        {
            get { return nameof(_components); }
        }
        #endregion

        #region Methods
        public override void Apply(short worldID, int entityID)
        {
            foreach (var item in _components)
            {
                item.Apply(worldID, entityID);
            }
        }
        public void Clear()
        {
            _components = Array.Empty<IComponentTemplate>();
        }
        #endregion

        #region UnityEvents
        private void OnValidate()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnValidate(gameObject);
            }
        }
        private void OnDrawGizmos()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Always);
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (_components == null) { return; }
            foreach (var item in _components)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Selected);
            }
        }
        #endregion
    }
}
