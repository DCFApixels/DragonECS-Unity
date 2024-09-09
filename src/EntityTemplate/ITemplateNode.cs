namespace DCFApixels.DragonECS
{
    public interface ITemplate : ITemplateNode { }

    public static class ITemplateNodeExtensions
    {
        public static entlong NewEntityWithGameObject(this EcsWorld world, ITemplateNode template, string name = "Entity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            entlong e = world.NewEntityWithGameObject(name, icon);
            template.Apply(world.id, e.ID);
            return e;
        }
    }
}

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal interface ITemplateInternal : ITemplate
    {
        string ComponentsPropertyName { get; }
    }
}