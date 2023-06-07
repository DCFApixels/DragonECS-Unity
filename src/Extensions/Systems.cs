using DCFApixels.DragonECS.Internal;
using System.Collections.Generic;

namespace DCFApixels.DragonECS
{
    [DebugHide, DebugColor(DebugColor.Grey)]
    public class DeleteOneFrameComponentFixedSystem<TComponent> : IEcsFixedRunProcess, IEcsInject<EcsWorld>
           where TComponent : struct, IEcsComponent
    {
        private sealed class Subject : EcsSubject
        {
            public EcsPool<TComponent> pool;
            public Subject(Builder b) => pool = b.Include<TComponent>();
        }
        List<EcsWorld> _worlds = new List<EcsWorld>();
        public void Inject(EcsWorld obj) => _worlds.Add(obj);
        public void FixedRun(EcsPipeline pipeline)
        {
            for (int i = 0, iMax = _worlds.Count; i < iMax; i++)
            {
                EcsWorld world = _worlds[i];
                if (world.IsComponentTypeDeclared<TComponent>())
                {
                    foreach (var e in world.Where(out Subject s))
                        s.pool.Del(e);
                }
            }
        }
    }
    public static class DeleteOneFrameComponentFixedSystemExtensions
    {
        private const string AUTO_DEL_LAYER = nameof(AUTO_DEL_LAYER);
        public static EcsPipeline.Builder AutoDelFixed<TComponent>(this EcsPipeline.Builder b, string layerName = AUTO_DEL_LAYER)
            where TComponent : struct, IEcsComponent
        {
            if (AUTO_DEL_LAYER == layerName)
                b.Layers.Insert(EcsConsts.POST_END_LAYER, AUTO_DEL_LAYER);
            b.AddUnique(new DeleteOneFrameComponentSystem<TComponent>(), layerName);
            return b;
        }
    }
}
