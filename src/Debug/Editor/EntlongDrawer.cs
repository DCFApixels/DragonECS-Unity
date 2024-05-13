#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUI;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(entlong))]
    internal class EntlongDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            using (new DisabledScope(false))
            {
                EntitySlotInfo slotInfo = new EntitySlotInfo(property.FindPropertyRelative("_full").longValue);
                var (labelRect, barRect) = RectUtility.HorizontalSliceLeft(position, EditorGUIUtility.labelWidth * 0.65f);

                EditorGUI.LabelField(labelRect, label);
                bool isAlive = EcsWorld.GetWorld(slotInfo.world).IsAlive(slotInfo.id, slotInfo.gen);
                EcsGUI.EntityBar(barRect, false, isAlive ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive, slotInfo.id, slotInfo.gen, slotInfo.world);
            }
        }
    }
}
#endif