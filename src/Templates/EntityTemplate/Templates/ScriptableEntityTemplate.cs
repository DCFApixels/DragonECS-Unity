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
    public abstract class ScriptableEntityTemplateBase : ScriptableObject, IEntityTemplate
    {
        public abstract void Apply(short worldID, int entityID);
    }

    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, nameof(ScriptableObject) + " implementation of an entity template. Templates are a set of components that are applied to entities.")]
    [CreateAssetMenu(fileName = nameof(ScriptableEntityTemplate), menuName = EcsConsts.FRAMEWORK_NAME + "/" + nameof(ScriptableEntityTemplate), order = 1)]
    [MetaID("DragonECS_7C4DBA809201D959401A5BDFB6363EC0")]
    public class ScriptableEntityTemplate : ScriptableEntityTemplateBase, IEntityTemplateInternal
    {
        [SerializeField]
        private ScriptableEntityTemplateBase[] _templates;
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
        public ReadOnlySpan<ScriptableEntityTemplateBase> GetTemplates()
        {
            return _templates;
        }
        public void SetTemplates(IEnumerable<ScriptableEntityTemplateBase> templates)
        {
            _templates = templates.ToArray();
        }
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
            foreach (var template in _templates)
            {
                template.Apply(worldID, entityID);
            }
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
                item?.OnValidate(this);
            }
        }
        #endregion
    }
}
