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
                EcsGUI.EntityBar(barRect, EcsGUI.EntityStatus.Alive, slotInfo.id, slotInfo.gen, slotInfo.world);
            }
        }
    }
}
#endif