namespace DCFApixels.DragonECS
{
    public sealed class DebugModule : IEcsModule
    {
        public const string DEBUG_LAYER = nameof(DEBUG_LAYER);
        public EcsWorld[] _worlds;

        public DebugModule(params EcsWorld[] worlds)
        {
            _worlds = worlds;
        }

        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            b.Layers.Insert(EcsConsts.POST_END_LAYER, DEBUG_LAYER);
            //b.Add(new PipelineDebugSystem(), DEBUG_LAYER);
            foreach (var world in _worlds)
            {
                //b.Add(new WorldDebugSystem(world), DEBUG_LAYER);
            }
        }
    }
}
