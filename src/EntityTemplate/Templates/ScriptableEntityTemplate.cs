using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = nameof(ScriptableEntityTemplate), menuName = EcsConsts.FRAMEWORK_NAME + "/" + nameof(ScriptableEntityTemplate), order = 1)]
    public class ScriptableEntityTemplate : ScriptableObject, ITemplateInternal
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
        public void Apply(int worldID, int entityID)
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
