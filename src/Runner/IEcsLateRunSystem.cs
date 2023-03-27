namespace DCFApixels.DragonECS.Unity
{
    public interface IEcsLateRunSystem : IEcsSystem
    {
        public void LateRun(EcsSystems systems);
    }

    public class EcsLateRunSystemRunner : EcsRunner<IEcsLateRunSystem>, IEcsLateRunSystem
    {
        void IEcsLateRunSystem.LateRun(EcsSystems systems)
        {
            foreach (var item in targets) item.LateRun(systems);
        }
    }

    public static class IEcsLateRunSystemExtensions
    {
        public static void LateRun(this EcsSystems systems)
        {
            systems.GetRunner<IEcsLateRunSystem>().LateRun(systems);
        }
    }
}
