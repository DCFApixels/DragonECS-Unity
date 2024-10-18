using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
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
    [MetaID("7C4DBA809201D959401A5BDFB6363EC0")]
    public class ScriptableEntityTemplate : ScriptableEntityTemplateBase, IEntityTemplateInternal
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
                item?.OnValidate(this);
            }
        }
        #endregion
    }
}
