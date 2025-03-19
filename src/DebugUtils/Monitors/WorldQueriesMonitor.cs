using DCFApixels.DragonECS.Core;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [MetaColor(MetaColor.Gray)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "...")]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_AC178504930164FD2EABFA071EF0476F")]
    internal class WorldQueriesMonitor : MonoBehaviour
    {
        private EcsWorld _world;
        private List<MaskQueryExecutor> _maskQueryExecutors = new List<MaskQueryExecutor>();
        private int _maskQueryExecutorsVersion = 0;

        public EcsWorld World
        {
            get { return _world; }
        }
        public List<MaskQueryExecutor> MaskQueryExecutors
        {
            get
            {
                _world.GetMaskQueryExecutors(_maskQueryExecutors, ref _maskQueryExecutorsVersion);
                return _maskQueryExecutors;
            }
        }
        public void Set(EcsWorld world)
        {
            _world = world;
        }
    }
}
