#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(PipelineTemplateUtility.Record))]
    internal class PipelineTemplateUtilityRecordDrawer : ExtendedPropertyDrawer
    {
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsArrayElement == false)
            {
                Rect foldoutRect;
                (foldoutRect, position) = position.VerticalSliceTop(OneLineHeight + Spacing);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
                if (property.isExpanded == false)
                {
                    return;
                }
            }

            using (EcsGUI.SetIndentLevel(IsArrayElement ? EcsGUI.IndentLevel : EcsGUI.IndentLevel + 1))
            {
                Rect subPosition = position;
                int depth = -1;
                float height = 0f;

                property.Next(true);

                do
                {
                    subPosition.y += height;
                    height = EditorGUI.GetPropertyHeight(property);
                    subPosition.height = height;

                    EditorGUI.PropertyField(subPosition, property, true);

                } while (property.NextDepth(false, ref depth));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float result = 0f;
            if (IsArrayElement == false)
            {
                result += OneLineHeight;
                if (property.isExpanded == false)
                {
                    return result;
                }
            }

            property.Next(true);
            int depth = -1;
            do
            {
                result += EditorGUI.GetPropertyHeight(property, true);
            } while (property.NextDepth(false, ref depth));
            return result;
        }
    }
}
#endif