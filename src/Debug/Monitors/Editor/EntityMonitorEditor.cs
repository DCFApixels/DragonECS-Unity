#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EntityMonitor))]
    internal class EntityMonitorEditor : Editor
    {
        private EntityMonitor Target => (EntityMonitor)target;

        public override void OnInspectorGUI()
        {
            bool isAlive = Target.Entity.TryUnpackForUnityEditor(out int id, out short gen, out short worldID, out EcsWorld world);
            EcsGUI.Layout.EntityBar(isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, id, gen, worldID);
            EcsGUI.Layout.DrawRuntimeComponents(Target.Entity, false);
        }
    }
}
#endif