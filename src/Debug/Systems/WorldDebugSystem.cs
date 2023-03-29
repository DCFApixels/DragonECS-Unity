using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class WorldDebugSystem : IEcsRunSystem 
    {
        private IEcsWorld _ecsWorld;

        public WorldDebugSystem(IEcsWorld ecsWorld)
        {
            _ecsWorld = ecsWorld;
        }

        public void Run(EcsSystems systems)
        {
        }
    }
}
