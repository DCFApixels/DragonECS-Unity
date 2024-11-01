using DCFApixels.DragonECS.RunnersCore;
using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;

namespace DCFApixels.DragonECS
{
    [MetaName(nameof(DrawGizmos))]
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "The process to run when EcsPipeline.DrawGizmos() is called.")]
    [MetaID("5DDBBB80920163B891A5BF52F9718A30")]
    public interface IEcsGizmosProcess : IEcsProcess
    {
        public void DrawGizmos();
    }
    [MetaName(nameof(LateRun))]
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "The process to run when EcsPipeline.LateRun() is called.")]
    [MetaID("BCE8BB8092015F2442B767645FD7F6CA")]
    public interface IEcsLateRunProcess : IEcsProcess
    {
        public void LateRun();
    }
    [MetaName(nameof(FixedRun))]
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "The process to run when EcsPipeline.FixedRun() is called.")]
    [MetaID("54F4BB8092010228FAAD78F9D2352117")]
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
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("2DD8BC809201633E2761D5AEF65B7090")]
    public class EcsLateGizmosRunner : EcsRunner<IEcsGizmosProcess>, IEcsGizmosProcess
    {
        private RunHelper _helper;
        protected override void OnSetup()
        {
            _helper = new RunHelper(this);
        }
        public void DrawGizmos()
        {
            _helper.Run(p => p.DrawGizmos());
        }
    }

    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("EDE8BC809201603B47C3A9D1EFD4EE95")]
    public class EcsLateRunRunner : EcsRunner<IEcsLateRunProcess>, IEcsLateRunProcess
    {
        private RunHelper _helper;
        protected override void OnSetup()
        {
            _helper = new RunHelper(this);
        }
        public void LateRun()
        {
            _helper.Run(p => p.LateRun());
        }
    }

    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.PROCESSES_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("45F7BC809201866AA05F6DC096A47F01")]
    public class EcsFixedRunRunner : EcsRunner<IEcsFixedRunProcess>, IEcsFixedRunProcess
    {
        private RunHelper _helper;
        protected override void OnSetup()
        {
            _helper = new RunHelper(this);
        }
        public void FixedRun()
        {
            _helper.Run(p => p.FixedRun());
        }
    }
}