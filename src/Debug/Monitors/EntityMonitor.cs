using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.Gray)]
    public class EntityMonitor : MonoBehaviour
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
