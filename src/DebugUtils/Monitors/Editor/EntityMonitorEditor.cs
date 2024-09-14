﻿#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EntityMonitor))]
    internal class EntityMonitorEditor : ExtendedEditor<EntityMonitor>
    {
        protected override void DrawCustom()
        {
            bool isAlive = Target.Entity.TryUnpackForUnityEditor(out int id, out short gen, out short worldID, out EcsWorld world);
            using (EcsGUI.SetEnable(isAlive))
            {
                if (GUILayout.Button("Delete Entity", GUILayout.Height(36f)))
                {
                    world.DelEntity(id);
                }
            }
            EcsGUI.Layout.EntityBarForAlive(isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, id, gen, worldID);
            EcsGUI.Layout.DrawRuntimeComponents(Target.Entity, false);
        }
    }
}
#endif