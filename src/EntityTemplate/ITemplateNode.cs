namespace DCFApixels.DragonECS
{
    public interface ITemplate : ITemplateNode
    {
        //void Add(ITemplateNode template);
        //void Remove(ITemplateNode template);
    }
    public static class ITemplateNodeExtensions
    {
        public static entlong NewEntityWithGameObject(this EcsWorld world, ITemplateNode template, string name = "Entity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            entlong e = world.NewEntityWithGameObject(name, icon);
            template.Apply(world.id, e.ID);
            return e;
        }
    }

    //[Serializable]
    //public class EntityTemplateInheritanceMatrix
    //{
    //    [SerializeReference]
    //    private ITemplateNode[] _components;
    //
    //    #region Methods
    //    public void Apply(int worldID, int entityID)
    //    {
    //        foreach (var item in _components)
    //        {
    //            item.Apply(worldID, entityID);
    //        }
    //    }
    //    #endregion
    //}
}

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal interface ITemplateInternal : ITemplate
    {
        string ComponentsPropertyName { get; }
        //EntityTemplateInheritanceMatrix InheritanceMatrix { get; }
    }
}