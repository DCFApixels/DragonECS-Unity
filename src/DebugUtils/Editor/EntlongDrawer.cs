#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(entlong))]
    internal class EntlongDrawer : ExtendedPropertyDrawer
    {
        private float heightCache = 0;
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect labelRect, hyperlinkButtonRect;
            (labelRect, position) = position.HorizontalSliceLeft(EditorGUIUtility.labelWidth * 0.65f);
            (position, hyperlinkButtonRect) = position.HorizontalSliceRight(18f);

            bool drawFoldout = property.hasMultipleDifferentValues == false;

            bool isExpanded = false;
            if (drawFoldout)
            {
                EditorGUI.BeginChangeCheck();
                isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);
                if (EditorGUI.EndChangeCheck())
                {
                    property.isExpanded = isExpanded;
                }
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            EntitySlotInfo entity = new EntitySlotInfo(property.FindPropertyRelative("_full").longValue);
            EcsWorld.TryGetWorld(entity.world, out EcsWorld world);

            if (drawFoldout && isExpanded)
            {
                using (EcsGUI.UpIndentLevel())
                {
                    if (world != null && world.IsAlive(entity.id, entity.gen))
                    {
                        EcsGUI.Layout.DrawRuntimeComponents(entity.id, world, false, false);
                        if (Event.current.type == EventType.Layout)
                        {
                            heightCache = GUILayoutUtility.GetLastRect().height;
                        }
                    }
                }
            }
            EcsGUI.EntityField(position, property);
            using (EcsGUI.SetEnable(world != null))
            {
                EcsGUI.EntityHyperlinkButton(hyperlinkButtonRect, world, entity.id);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            const float UNITY_HEIGHT_CONSTANT = 18f;
            if (property.hasMultipleDifferentValues)
            {
                return UNITY_HEIGHT_CONSTANT;
            }
            return Mathf.Max(heightCache, UNITY_HEIGHT_CONSTANT);
        }
    }
}
#endif