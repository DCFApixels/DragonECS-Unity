using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public abstract class ScriptableEntityTemplateBase : ScriptableObject, ITemplate
    {
        [SerializeField]
        private int _saveID;
        public abstract void Apply(short worldID, int entityID);

        private static IComponentTemplate _fake = null;
        protected virtual IList<IComponentTemplate> GetToRecover() { return null; }
        protected virtual ref IComponentTemplate GetToRecoverSingle() { return ref _fake; }
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
