using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class EntityTemplate : MonoBehaviour, ITemplateInternal
    {
        [SerializeReference]
        private ITemplateComponent[] _components;
        string ITemplateInternal.ComponentsPropertyName => nameof(_components);

        public void Apply(EcsWorld world, int entityID)
        {
            foreach (var item in _components)
                item.Add(world, entityID);
        }
    }
}
