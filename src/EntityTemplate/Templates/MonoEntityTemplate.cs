using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public abstract class MonoEntityTemplateBase : MonoBehaviour, ITemplate
    {
        public abstract void Apply(short worldID, int entityID);
    }

    [DisallowMultipleComponent]
    [AddComponentMenu(EcsConsts.FRAMEWORK_NAME + "/" + nameof(MonoEntityTemplate), 30)]
    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsConsts.FRAMEWORK_GROUP, EcsUnityConsts.UNITY_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    public class MonoEntityTemplate : MonoEntityTemplateBase, ITemplateInternal
    {
        [SerializeReference]
        private IComponentTemplate[] _components;
        //[SerializeField]
        //private EntityTemplateInheritanceMatrix _inheritanceMatrix;

        #region Properties
        string ITemplateInternal.ComponentsPropertyName
        {
            get { return nameof(_components); }
        }
        //EntityTemplateInheritanceMatrix ITemplateInternal.InheritanceMatrix
        //{
        //    get { return _inheritanceMatrix; }
        //}
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
