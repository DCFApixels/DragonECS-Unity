#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(entlong))]
    internal class EntlongDrawer : ExtendedPropertyDrawer
    {
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            using (EcsGUI.Disable)
            {
                EntitySlotInfo slotInfo = new EntitySlotInfo(property.FindPropertyRelative("_full").longValue);
                var (labelRect, barRect) = position.HorizontalSliceLeft(EditorGUIUtility.labelWidth * 0.65f);

                EditorGUI.LabelField(labelRect, label);
                bool isAlive = EcsWorld.GetWorld(slotInfo.world).IsAlive(slotInfo.id, slotInfo.gen);
                EcsGUI.EntityBar(barRect, false, isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, slotInfo.id, slotInfo.gen, slotInfo.world);
            }
        }
    }
}
#endif