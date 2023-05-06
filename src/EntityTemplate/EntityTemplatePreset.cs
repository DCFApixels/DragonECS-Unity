using UnityEngine;

namespace DCFApixels.DragonECS
{
    [CreateAssetMenu(fileName = "EntityTemplatePreset", menuName = EcsConsts.FRAMEWORK_NAME + "/EntityTemplatePreset", order = 1)]
    public class EntityTemplatePreset : ScriptableObject, ITemplateInternal
    {
        [SerializeReference]
        private ITemplateComponent[] _components;
        string ITemplateInternal.ComponentsPropertyName => nameof(_components);

        //ITemplateBrowsable[] ITemplateInternal.Components
        //{
        //    get => _components;
        //    set => _components = value;
        //}

        public void Apply(EcsWorld world, int entityID)
        {
            foreach (var item in _components)
                item.Add(world, entityID);
        }
    }
}
