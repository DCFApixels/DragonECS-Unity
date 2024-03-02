using DCFApixels.DragonECS.Internal;
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
            systems.GetRunnerInstance<EcsLateGizmosRunner>().DrawGizmos(systems);
        }
    }

    public interface IEcsLateRunProcess : IEcsProcess
    {
        public void LateRun(EcsPipeline pipeline);
    }
    public static class IEcsLateRunSystemExtensions
    {
        public static void LateRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunnerInstance<EcsLateRunRunner>().LateRun(pipeline);
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
            pipeline.GetRunnerInstance<EcsFixedRunRunner>().FixedRun(pipeline);
        }
    }

    namespace Internal
    {
        [MetaColor(MetaColor.Orange)]
        public class EcsLateGizmosRunner : EcsRunner<IEcsGizmosProcess>, IEcsGizmosProcess
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void DrawGizmos(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < Process.Length; i++)
                {
                    using (_markers[i].Auto())
                        Process[i].DrawGizmos(pipeline);
                }
#else
            foreach (var item in targets) item.DrawGizmos(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[Process.Length];
                for (int i = 0; i < Process.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker($"{Process[i].GetType().Name}.{nameof(DrawGizmos)}");
                }
            }
#endif
        }

        [MetaColor(MetaColor.Orange)]
        public class EcsLateRunRunner : EcsRunner<IEcsLateRunProcess>, IEcsLateRunProcess
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void LateRun(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < Process.Length; i++)
                {
                    using (_markers[i].Auto())
                    {
                        Process[i].LateRun(pipeline);
                    }
                }
#else
            foreach (var item in targets) item.LateRun(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[Process.Length];
                for (int i = 0; i < Process.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker($"EcsRunner.{Process[i].GetType().Name}.{nameof(LateRun)}");
                }
            }
#endif
        }
        [MetaColor(MetaColor.Orange)]
        public class EcsFixedRunRunner : EcsRunner<IEcsFixedRunProcess>, IEcsFixedRunProcess
        {
#if DEBUG && !DISABLE_DEBUG
            private EcsProfilerMarker[] _markers;
#endif
            public void FixedRun(EcsPipeline pipeline)
            {
#if DEBUG && !DISABLE_DEBUG
                for (int i = 0; i < Process.Length; i++)
                {
                    using (_markers[i].Auto())
                    {
                        Process[i].FixedRun(pipeline);
                    }
                }
#else
            foreach (var item in targets) item.FixedRun(pipeline);
#endif
            }

#if DEBUG && !DISABLE_DEBUG
            protected override void OnSetup()
            {
                _markers = new EcsProfilerMarker[Process.Length];
                for (int i = 0; i < Process.Length; i++)
                {
                    _markers[i] = new EcsProfilerMarker($"EcsRunner.{Process[i].GetType().Name}.{nameof(FixedRun)}");
                }
            }
#endif
        }
    }
}
