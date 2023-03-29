namespace DCFApixels.DragonECS
{
    public interface IEcsFixedRunSystem : IEcsSystem
    {
        public void FixedRun(EcsPipeline pipeline);
    }

    public class EcsFixedRunSystemRunner : EcsRunner<IEcsFixedRunSystem>, IEcsFixedRunSystem
    {
        void IEcsFixedRunSystem.FixedRun(EcsPipeline pipeline)
        {
            foreach (var item in targets) item.FixedRun(pipeline);
        }
    }

    public static class IEcsFixedRunSystemExtensions
    {
        public static void FixedRun(this EcsPipeline pipeline)
        {
            pipeline.GetRunner<IEcsFixedRunSystem>().FixedRun(pipeline);
        }
    }
}
