using DCFApixels.DragonECS.RunnersCore;

namespace DCFApixels.DragonECS
{
    public interface IEcsGizmosProcess : IEcsProcess
    {
        public void DrawGizmos(EcsPipeline pipeline);
    }
    public static class IEcsGizmosProcessExtensions
    {
        public static void DrawGizmos(this EcsPipeline systems)
        {
            systems.GetRunner<IEcsGizmosProcess>().DrawGizmos(systems);
        }
    }

    public interface IEcsLateRunProcess : IEcsProcess
    {
        public void LateRun(EcsPipeline pipeline);
    }
    public static class IEcsLateRunSystemExtensions
    {
        public static void LateRun(this EcsPipeline systems)
        {
            systems.GetRunner<IEcsLateRunProcess>().LateRun(systems);
        }
    }
    public interface IEcsFixedRunProcess : IEcsProcess
    {
        public void FixedRun(EcsPipeline pipeline);
    }
    public static class IEcsFixedRunSystemExtensions
    {
        public static void FixedRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunner<IEcsFixedRunProcess>().FixedRun(pipeline);
        }
    }

    namespace Internal
    {
        [DebugColor(DebugColor.Orange)]
        public class EcsLateGizmosSystemRunner : EcsRunner<IEcsGizmosProcess>, IEcsGizmosProcess
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void DrawGizmos(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < targets.Length; i++)
                {
                    using (_markers[i].Auto())
                        targets[i].DrawGizmos(pipeline);
                }
#else
            foreach (var item in targets) item.DrawGizmos(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker($"{targets[i].GetType().Name}.{nameof(DrawGizmos)}");
                }
            }
#endif
        }

        [DebugColor(DebugColor.Orange)]
        public class EcsLateRunSystemRunner : EcsRunner<IEcsLateRunProcess>, IEcsLateRunProcess
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
                    _markers[i] = new EcsProfilerMarker($"EcsRunner.{targets[i].GetType().Name}.{nameof(LateRun)}");
                }
            }
#endif
        }
        [DebugColor(DebugColor.Orange)]
        public class EcsFixedRunSystemRunner : EcsRunner<IEcsFixedRunProcess>, IEcsFixedRunProcess
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
                    _markers[i] = new EcsProfilerMarker($"EcsRunner.{targets[i].GetType().Name}.{nameof(FixedRun)}");
                }
            }
#endif
        }
    }
}
