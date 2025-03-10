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
            using (EcsGUI.SetEnable(isAlive))
            {
                if (GUILayout.Button("Delete Entity", GUILayout.Height(36f)))
                {
                    world.DelEntity(id);
                }
            }
            EcsGUI.Layout.EntityBarForAlive(isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, id, gen, worldID);

            var drawers = UnityEditorUtility._entityEditorBlockDrawers;
            if (drawers.Length > 0)
            {
                using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
                {
                    bool isExpand = false;
                    using (EcsGUI.CheckChanged())
                    {
                        isExpand = EditorGUILayout.Foldout(UserSettingsPrefs.instance.IsShowEntityOtherData, "Other data");
                        if (EcsGUI.Changed)
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
            
            EcsGUI.Layout.DrawRuntimeComponents(entity, false);
        }
    }
}
#endif