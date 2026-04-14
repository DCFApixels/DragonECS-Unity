#if UNITY_EDITOR
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
            var entity = Target.Entity;
            bool isAlive = entity.TryUnpackForUnityEditor(out int id, out short gen, out short worldID, out EcsWorld world);
            using (DragonGUI.SetEnable(isAlive))
            {
                if (GUILayout.Button("Delete Entity", GUILayout.Height(36f)))
                {
                    world.DelEntity(id);
                }
            }
            //EcsGUI.Layout.EntityBarForAlive(isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, id, gen, worldID);
            DragonGUI.Layout.EntityField(entity);

            var drawers = UnityEditorUtility._entityEditorBlockDrawers;
            if (drawers.Length > 0)
            {
                using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
                {
                    bool isExpand = false;
                    using (DragonGUI.CheckChanged())
                    {
                        isExpand = EditorGUILayout.Foldout(UserSettingsPrefs.instance.IsShowEntityOtherData, "Other data");
                        if (DragonGUI.Changed)
                        {
                            UserSettingsPrefs.instance.IsShowEntityOtherData = isExpand;
                        }
                    }

                    if (isExpand)
                    {
                        foreach (var drawer in drawers)
                        {
                            drawer.Draw(entity);
                        }
                    }
                }

            }

            DragonGUI.Layout.DrawRuntimeComponents(entity, false);
        }
    }
}
#endif