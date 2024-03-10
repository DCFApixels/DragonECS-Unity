#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(WorldMonitor))]
    internal class WorldMonitorEditor : Editor
    {
        private WorldMonitor Target => (WorldMonitor)target;

        public override void OnInspectorGUI()
        {
            EcsGUI.Layout.DrawWorldBaseInfo(Target.World);
        }
    }
}
#endif