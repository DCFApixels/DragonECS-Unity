namespace DCFApixels.DragonECS
{
    public interface IEcsLateRunSystem : IEcsSystem
    {
        public void LateRun(EcsPipeline pipeline);
    }

    public class EcsLateRunSystemRunner : EcsRunner<IEcsLateRunSystem>, IEcsLateRunSystem
    {
#if DEBUG
        private EcsProfilerMarker[] _markers;
#endif
        public void LateRun(EcsPipeline pipeline)
        {
#if DEBUG
            for (int i = 0; i < targets.Length; i++)
            {
                using (_markers[i].Auto())
                    targets[i].LateRun(pipeline);
            }
#else
            foreach (var item in targets) item.LateRun(systems);
#endif
        }

#if DEBUG
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
    public class EcsFixedRunSystemRunner : EcsRunner<IEcsFixedRunSystem>, IEcsFixedRunSystem
    {
#if DEBUG
        private EcsProfilerMarker[] _markers;
#endif
        public void FixedRun(EcsPipeline pipeline)
        {
#if DEBUG
            for (int i = 0; i < targets.Length; i++)
            {
                using (_markers[i].Auto())
                    targets[i].FixedRun(pipeline);
            }
#else
            foreach (var item in targets) item.FixedRun(pipeline);
#endif
        }

#if DEBUG
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
    public static class IEcsFixedRunSystemExtensions
    {
        public static void FixedRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunner<IEcsFixedRunSystem>().FixedRun(pipeline);
        }
    }
}
