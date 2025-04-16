using DCFApixels.DragonECS.Unity.Editors;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_A551B6809201D56AA0F1B51248174B4D")]
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
#if UNITY_EDITOR
            var world = entity.GetWorldUnchecked();
            world.Get<EcsGUI.EntityLinksComponent>().SetMonitorLink(entity.GetIDUnchecked(), this);
#endif
        }
    }
}