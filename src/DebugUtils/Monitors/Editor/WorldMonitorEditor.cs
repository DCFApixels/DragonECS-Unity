#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(WorldMonitor))]
    internal class WorldMonitorEditor : ExtendedEditor<WorldMonitor>
    {
        protected override void DrawCustom()
        {
            EcsGUI.Layout.DrawWorldBaseInfo(Target.World);
        }
    }
}
#endif