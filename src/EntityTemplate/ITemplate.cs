namespace DCFApixels.DragonECS
{
    public interface ITemplate
    {
        public void Apply(EcsWorld world, int entityID);
    }

    public interface ITemplateInternal : ITemplate
    {
        // internal ITemplateBrowsable[] Components { get; set; }
        internal string ComponentsPropertyName { get; }
    }

    public static class ITemplateExt
    {
        public static int NewEntity(this ITemplate self, EcsWorld world)
        {
            int e = world.NewEmptyEntity();
            self.Apply(world, e);
            return e;
        }
    }
}
