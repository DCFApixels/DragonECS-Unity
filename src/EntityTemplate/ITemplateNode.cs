namespace DCFApixels.DragonECS
{
    public interface ITemplateNode
    {
        void Apply(int worldID, int entityID);
    }
    public interface ITemplate : ITemplateNode
    {
        //void Add(ITemplateNode template);
        //void Remove(ITemplateNode template);
    }

    public interface ITemplateInternal : ITemplate
    {
        string ComponentsPropertyName { get; }
        //EntityTemplateInheritanceMatrix InheritanceMatrix { get; }
    }

    public static class ITemplateExtensions
    {
        public static int NewEntity(this EcsWorld world, ITemplateNode template)
        {
            int e = world.NewEntity();
            template.Apply(world.id, e);
            return e;
        }
        public static entlong NewEntityLong(this EcsWorld world, ITemplateNode template)
        {
            entlong e = world.NewEntityLong();
            template.Apply(world.id, e.ID);
            return e;
        }
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
