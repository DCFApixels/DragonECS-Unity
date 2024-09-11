#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(AddParams))]
    internal class AddParamsDrawer : PropertyDrawer
    {
        private float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private float Spacing => EditorGUIUtility.standardVerticalSpacing;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !property.isExpanded ? EditorGUIUtility.singleLineHeight + Spacing : EditorGUIUtility.singleLineHeight * 4f + Spacing * 3f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var flagsProp = property.FindPropertyRelative("flags");

            AddParamsFlags flags = (AddParamsFlags)flagsProp.enumValueFlag;

            var (foldoutRect, contentRect) = position.VerticalSliceTop(SingleLineHeight + Spacing);

            var (fieldsRects, checkboxRects) = contentRect.HorizontalSliceRight(SingleLineHeight);


            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            //property.isExpanded = true;
            using (EcsGUI.UpIndentLevel())
            {

                EditorGUI.BeginChangeCheck();

                checkboxRects = checkboxRects.Move(EditorGUIUtility.standardVerticalSpacing, 0);

                Rect checkboxRect = checkboxRects;
                checkboxRect.height = SingleLineHeight;
                Rect fieldRect = fieldsRects;
                fieldRect.height = SingleLineHeight;

                //LayerName
                property.Next(true);
                using (EcsGUI.SetIndentLevel(0))
                {
                    flags = flags.SetOverwriteLayerName(EditorGUI.Toggle(checkboxRect, flags.IsOverwriteLayerName()));
                }
                using (EcsGUI.SetEnable(flags.IsOverwriteLayerName()))
                {
                    EditorGUI.PropertyField(fieldRect, property, UnityEditorUtility.GetLabel(property.displayName), true);
                }

                checkboxRect = checkboxRect.Move(0, SingleLineHeight + Spacing);
                fieldRect = fieldRect.Move(0, SingleLineHeight + Spacing);

                //SortOrder
                property.Next(false);
                using (EcsGUI.SetIndentLevel(0))
                {
                    flags = flags.SetOverwriteSortOrder(EditorGUI.Toggle(checkboxRect, flags.IsOverwriteSortOrder()));
                }
                using (EcsGUI.SetEnable(flags.IsOverwriteSortOrder()))
                {
                    EditorGUI.PropertyField(fieldRect, property, UnityEditorUtility.GetLabel(property.displayName), true);
                }

                checkboxRect = checkboxRect.Move(0, SingleLineHeight + Spacing);
                fieldRect = fieldRect.Move(0, SingleLineHeight + Spacing);

                //IsUnique
                property.Next(false);
                using (EcsGUI.SetIndentLevel(0))
                {
                    flags = flags.SetOverwriteIsUnique(EditorGUI.Toggle(checkboxRect, flags.IsOverwriteIsUnique()));
                }
                using (EcsGUI.SetEnable(flags.IsOverwriteIsUnique()))
                {
                    EditorGUI.PropertyField(fieldRect, property, UnityEditorUtility.GetLabel(property.displayName), true);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    flagsProp.enumValueFlag = (int)flags;
                    property.serializedObject.ApplyModifiedProperties();
                }

            }
        }
    }
}
#endif
