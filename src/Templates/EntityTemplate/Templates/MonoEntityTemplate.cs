#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [MetaID("DragonECS_C734BA8092014833C14F21E05D7B1551")]
    public class MonoEntityTemplate : MonoEntityTemplateBase, IEntityTemplateInternal
    {
        [SerializeReference]
        [ReferenceButton(true, typeof(IComponentTemplate))]
        private IComponentTemplate[] _componentTemplates;

        #region Properties
        string IEntityTemplateInternal.ComponentsPropertyName
        {
            get { return nameof(_componentTemplates); }
        }
        #endregion

        #region Methods
        public ReadOnlySpan<IComponentTemplate> GetComponentTemplates()
        {
            return _componentTemplates;
        }
        public void SetComponentTemplates(IEnumerable<IComponentTemplate> componentTemplates)
        {
            _componentTemplates = componentTemplates.ToArray();
        }
        public override void Apply(short worldID, int entityID)
        {
            foreach (var item in _componentTemplates)
            {
                item.Apply(worldID, entityID);
            }
        }
        public void Clear()
        {
            _componentTemplates = Array.Empty<IComponentTemplate>();
        }
        #endregion

        #region UnityEvents
        private void OnValidate()
        {
            if (_componentTemplates == null) { return; }
            foreach (var item in _componentTemplates)
            {
                item?.OnValidate(gameObject);
            }
        }
        private void OnDrawGizmos()
        {
            if (_componentTemplates == null) { return; }
            foreach (var item in _componentTemplates)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Always);
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (_componentTemplates == null) { return; }
            foreach (var item in _componentTemplates)
            {
                item?.OnGizmos(transform, IComponentTemplate.GizmosMode.Selected);
            }
        }
        #endregion
    }
}
