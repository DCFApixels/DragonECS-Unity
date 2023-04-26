namespace DCFApixels.DragonECS
{
    public sealed class DebugModule : IEcsModule
    {
        public const string DEBUG_SYSTEMS_BLOCK = nameof(DEBUG_SYSTEMS_BLOCK);
        public EcsWorld[] _worlds;
        public DebugModule(params EcsWorld[] worlds)
        {
            _worlds = worlds;
        }

        void IEcsModule.ImportSystems(EcsPipeline.Builder b)
        {
            b.InsertSystemsBlock(DEBUG_SYSTEMS_BLOCK, EcsConsts.POST_END_SYSTEMS_BLOCK);
            b.Add(new PipelineDebugSystem(), DEBUG_SYSTEMS_BLOCK);
            foreach (var world in _worlds)
            {
                b.Add(new WorldDebugSystem(world), DEBUG_SYSTEMS_BLOCK);
            }
        }
    }
}
