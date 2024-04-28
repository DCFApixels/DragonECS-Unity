﻿using DCFApixels.DragonECS.RunnersCore;
using DCFApixels.DragonECS.Unity.Internal;

namespace DCFApixels.DragonECS
{
    [MetaName(nameof(DrawGizmos))]
    [MetaColor(MetaColor.Orange)]
    public interface IEcsGizmosProcess : IEcsProcess
    {
        public void DrawGizmos();
    }
    [MetaName(nameof(LateRun))]
    [MetaColor(MetaColor.Orange)]
    public interface IEcsLateRunProcess : IEcsProcess
    {
        public void LateRun();
    }
    [MetaName(nameof(FixedRun))]
    [MetaColor(MetaColor.Orange)]
    public interface IEcsFixedRunProcess : IEcsProcess
    {
        public void FixedRun();
    }

    public static class UnityProcessExtensions
    {
        public static void DrawGizmos(this EcsPipeline pipeline)
        {
            pipeline.GetRunnerInstance<EcsLateGizmosRunner>().DrawGizmos();
        }
        public static void LateRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunnerInstance<EcsLateRunRunner>().LateRun();
        }
        public static void FixedRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunnerInstance<EcsFixedRunRunner>().FixedRun();
        }
    }
}
namespace DCFApixels.DragonECS.Unity.Internal
{

    [MetaColor(MetaColor.Orange)]
    public class EcsLateGizmosRunner : EcsRunner<IEcsGizmosProcess>, IEcsGizmosProcess
    {
#if DEBUG && !DISABLE_DEBUG
        private EcsProfilerMarker[] _markers;
#endif
        public void DrawGizmos()
        {
#if DEBUG && !DISABLE_DEBUG
            for (int i = 0; i < Process.Length; i++)
            {
                using (_markers[i].Auto())
                {
                    Process[i].DrawGizmos();
                }
            }
#else
            foreach (var item in Process) item.DrawGizmos();
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
        public void LateRun()
        {
#if DEBUG && !DISABLE_DEBUG
            for (int i = 0; i < Process.Length; i++)
            {
                using (_markers[i].Auto())
                {
                    Process[i].LateRun();
                }
            }
#else
            foreach (var item in Process) item.LateRun();
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
        public void FixedRun()
        {
#if DEBUG && !DISABLE_DEBUG
            for (int i = 0; i < Process.Length; i++)
            {
                using (_markers[i].Auto())
                {
                    Process[i].FixedRun();
                }
            }
#else
            foreach (var item in Process) item.FixedRun();
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
