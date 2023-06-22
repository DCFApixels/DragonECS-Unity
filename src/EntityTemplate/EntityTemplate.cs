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

        private void OnDrawGizmos()
        {
            if (_components == null) return;
            foreach (var item in _components)
            {
                if (item is ITemplateComponentGizmos g)
                    g.OnGizmos(transform, ITemplateComponentGizmos.Mode.Always);
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (_components == null) return;
            foreach (var item in _components)
            {
                if (item is ITemplateComponentGizmos g)
                    g.OnGizmos(transform, ITemplateComponentGizmos.Mode.Selected);
            }
        }
        public void Clear()
        {
            _components = new ITemplateComponent[0];
        }
    }
}
