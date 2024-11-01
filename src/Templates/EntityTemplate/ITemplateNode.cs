namespace DCFApixels.DragonECS
{
    public interface IEntityTemplate : ITemplateNode { }

    public static class ITemplateNodeExtensions
    {
        public static entlong NewEntityWithGameObject(this EcsWorld world, ITemplateNode template, string name = "Entity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            entlong e = world.NewEntityWithGameObject(name, icon);
            template.Apply(world.ID, e.ID);
            return e;
        }
    }
}

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal interface IEntityTemplateInternal : IEntityTemplate
    {
        string ComponentsPropertyName { get; }
    }
}