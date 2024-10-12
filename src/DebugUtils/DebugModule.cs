using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;

namespace DCFApixels.DragonECS
{
    [MetaColor(MetaColor.DragonRose)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaID("1D16B980920108B62A0971E4058A3E01")]
    public sealed class DebugModule : IEcsModule
    {
        public const string DEBUG_LAYER = EcsUnityConsts.DEBUG_LAYER;
        public EcsWorld[] _worlds;
        public DebugModule(params EcsWorld[] worlds)
        {
            _worlds = worlds;
        }
        void IEcsModule.Import(EcsPipeline.Builder b)
        {
            UnityDebugService.Activate();
            b.Layers.Insert(EcsConsts.POST_END_LAYER, DEBUG_LAYER);
            b.AddUnique(new PipelineMonitorSystem(), DEBUG_LAYER);
            foreach (var world in _worlds)
            {
                b.Add(new WorldMonitorSystem(world), DEBUG_LAYER);
            }
        }
    }

    public static class DebugModuleExt
    {
        public static EcsPipeline.Builder AddUnityDebug(this EcsPipeline.Builder self, params EcsWorld[] worlds)
        {
            self.AddModule(new DebugModule(worlds));
            return self;
        }
    }
}
