using DCFApixels.DragonECS.RunnersCore;

namespace DCFApixels.DragonECS
{
    public interface IEcsLateRunSystem : IEcsSystem
    {
        public void LateRun(EcsPipeline pipeline);
    }
    public static class IEcsLateRunSystemExtensions
    {
        public static void LateRun(this EcsPipeline systems)
        {
            systems.GetRunner<IEcsLateRunSystem>().LateRun(systems);
        }
    }
    public interface IEcsFixedRunSystem : IEcsSystem
    {
        public void FixedRun(EcsPipeline pipeline);
    }
    public static class IEcsFixedRunSystemExtensions
    {
        public static void FixedRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunner<IEcsFixedRunSystem>().FixedRun(pipeline);
        }
    }

    namespace Internal
    {
        [DebugColor(DebugColor.Orange)]
        public class EcsLateRunSystemRunner : EcsRunner<IEcsLateRunSystem>, IEcsLateRunSystem
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void LateRun(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < targets.Length; i++)
                {
                    using (_markers[i].Auto())
                        targets[i].LateRun(pipeline);
                }
#else
            foreach (var item in targets) item.LateRun(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker(EcsDebug.RegisterMark($"EcsRunner.{targets[i].GetType().Name}.{nameof(LateRun)}"));
                }
            }
#endif
        }
        [DebugColor(DebugColor.Orange)]
        public class EcsFixedRunSystemRunner : EcsRunner<IEcsFixedRunSystem>, IEcsFixedRunSystem
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void FixedRun(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < targets.Length; i++)
                {
                    using (_markers[i].Auto())
                        targets[i].FixedRun(pipeline);
                }
#else
            foreach (var item in targets) item.FixedRun(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker(EcsDebug.RegisterMark($"EcsRunner.{targets[i].GetType().Name}.{nameof(FixedRun)}"));
                }
            }
#endif
        }
    }
}
