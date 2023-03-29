namespace DCFApixels.DragonECS
{
    public interface IEcsFixedRunSystem : IEcsSystem
    {
        public void FixedRun(EcsSystems systems);
    }

    public class EcsFixedRunSystemRunner : EcsRunner<IEcsFixedRunSystem>, IEcsFixedRunSystem
    {
        void IEcsFixedRunSystem.FixedRun(EcsSystems systems)
        {
            foreach (var item in targets) item.FixedRun(systems);
        }
    }

    public static class IEcsFixedRunSystemExtensions
    {
        public static void FixedRun(this EcsSystems systems)
        {
            systems.GetRunner<IEcsFixedRunSystem>().FixedRun(systems);
        }
    }
}
