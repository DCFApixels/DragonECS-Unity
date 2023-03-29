namespace DCFApixels.DragonECS
{
    public interface IEcsLateRunSystem : IEcsSystem
    {
        public void LateRun(EcsPipeline systems);
    }

    public class EcsLateRunSystemRunner : EcsRunner<IEcsLateRunSystem>, IEcsLateRunSystem
    {
        void IEcsLateRunSystem.LateRun(EcsPipeline systems)
        {
            foreach (var item in targets) item.LateRun(systems);
        }
    }

    public static class IEcsLateRunSystemExtensions
    {
        public static void LateRun(this EcsPipeline systems)
        {
            systems.GetRunner<IEcsLateRunSystem>().LateRun(systems);
        }
    }
}
