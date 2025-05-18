using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;

namespace DCFApixels.DragonECS
{
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaID("DragonECS_1D16B980920108B62A0971E4058A3E01")]
    public sealed class UnityDebugModule : IEcsModule
    {
        public EcsWorld[] _worlds;
        public UnityDebugModule(params EcsWorld[] worlds)
        {
            _worlds = worlds;
        }
        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            UnityDebugService.Activate();
            b.Layers.Add(EcsUnityConsts.DEBUG_LAYER).Before(EcsConsts.POST_END_LAYER);
            b.AddUnique(new PipelineMonitorSystem(), EcsUnityConsts.DEBUG_LAYER);
            foreach (var world in _worlds)
            {
                b.Add(new WorldMonitorSystem(world), EcsUnityConsts.DEBUG_LAYER);
            }
        }
    }

    public static class DebugModuleExt
    {
        public static EcsPipeline.Builder AddUnityDebug(this EcsPipeline.Builder self, params EcsWorld[] worlds)
        {
            self.AddModule(new UnityDebugModule(worlds));
            return self;
        }
    }
}
