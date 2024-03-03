using UnityEngine;

namespace DCFApixels.DragonECS
{
    [MetaTags(MetaTags.HIDDEN), MetaColor(MetaColor.Gray)]
    public class EntityMonitor : MonoBehaviour, IEcsProcess
    {
        private entlong _entity;
        private int _entityID;
        private short _gen;
        private EcsWorld _world;

        public EcsWorld World
        {
            get { return _world; }
        }
        public int EntityID
        {
            get { return _entityID; }
        }

        public EntityMonitor(entlong entity)
        {
            _entity = entity;
            if (_entity.TryUnpack(out _entityID, out _gen, out _world))
            {

            }
        }
    }
}
