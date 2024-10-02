using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public abstract class ScriptableEntityTemplateBase : ScriptableObject, ITemplate
    {
        public abstract void Apply(short worldID, int entityID);
    }

    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsUnityConsts.ENTITY_BUILDING_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, nameof(ScriptableObject) + " implementation of an entity template. Templates are a set of components that are applied to entities.")]
    [CreateAssetMenu(fileName = nameof(ScriptableEntityTemplate), menuName = EcsConsts.FRAMEWORK_NAME + "/" + nameof(ScriptableEntityTemplate), order = 1)]
    public class ScriptableEntityTemplate : ScriptableEntityTemplateBase, ITemplateInternal
    {
        [SerializeReference]
        [ReferenceButton(typeof(IComponentTemplate))]
        private IComponentTemplate[] _components;

        #region Properties
        string ITemplateInternal.ComponentsPropertyName
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
                item.OnValidate(this);
            }
        }
        #endregion
    }
}
