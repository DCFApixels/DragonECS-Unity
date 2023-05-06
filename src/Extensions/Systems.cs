namespace DCFApixels.DragonECS
{
    [DebugHide, DebugColor(DebugColor.Grey)]
    public class DeleteOneFrameComponentFixedSystem<TWorld, TComponent> : IEcsFixedRunProcess, IEcsInject<TWorld>
        where TWorld : EcsWorld<TWorld>
        where TComponent : struct, IEcsComponent
    {
        private TWorld _world;
        public void Inject(TWorld obj) => _world = obj;

        private sealed class Subject : EcsSubject
        {
            public EcsPool<TComponent> pool;
            public Subject(Builder b)
            {
                pool = b.Include<TComponent>();
            }
        }
        public void FixedRun(EcsPipeline pipeline)
        {
            foreach (var e in _world.Where(out Subject s))
            {
                //try
                //{
                s.pool.Del(e);
                //}
                //catch (System.Exception)
                //{
                //
                //    throw;
                //}
            }
        }
    }

    public static class DeleteOneFrameComponentFixedSystemExt
    {
        private const string AUTO_DEL_FIXED_LAYER = nameof(AUTO_DEL_FIXED_LAYER);
        public static EcsPipeline.Builder AutoDelFixed<TWorld, TComponent>(this EcsPipeline.Builder b)
            where TWorld : EcsWorld<TWorld>
            where TComponent : struct, IEcsComponent
        {
            b.Layers.Insert(EcsConsts.POST_END_LAYER, AUTO_DEL_FIXED_LAYER);
            b.AddUnique(new DeleteOneFrameComponentFixedSystem<TWorld, TComponent>(), AUTO_DEL_FIXED_LAYER);
            return b;
        }
        /// <summary> for EcsDefaultWorld </summary>
        public static EcsPipeline.Builder AutoDelFixed<TComponent>(this EcsPipeline.Builder b)
            where TComponent : struct, IEcsComponent
        {
            b.Layers.Insert(EcsConsts.POST_END_LAYER, AUTO_DEL_FIXED_LAYER);
            b.AddUnique(new DeleteOneFrameComponentFixedSystem<EcsDefaultWorld, TComponent>(), AUTO_DEL_FIXED_LAYER);
            return b;
        }
    }
}
