using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    internal class EntityMonitor : MonoBehaviour
    {
        private entlong _entity;
        public entlong Entity
        {
            get { return _entity; }
        }
        public void Set(entlong entity)
        {
            _entity = entity;
        }
    }
}