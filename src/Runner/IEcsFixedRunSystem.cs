namespace DCFApixels.DragonECS.Unity
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
}
