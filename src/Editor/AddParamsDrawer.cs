#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(AddParams))]
    internal class AddParamsDrawer : ExtendedPropertyDrawer
    {
        private static Dictionary<byte, string> _labelCache = new Dictionary<byte, string>()
        {
            { 0, "" },
            { 1, "Layer" },
            { 2, "Order" },
            { 3, "Layer, Order" },
            { 4, "IsUnique" },
            { 5, "Layer, IsUnique" },
            { 6, "Order, IsUnique" },
            { 7, "Layer, Order, IsUnique" },
            { (byte)AddParamsFlags.NoImport | 0, "NoImport" },
            { (byte)AddParamsFlags.NoImport | 1, "NoImport, Layer" },
            { (byte)AddParamsFlags.NoImport | 2, "NoImport, Order" },
            { (byte)AddParamsFlags.NoImport | 3, "NoImport, Layer, Order" },
            { (byte)AddParamsFlags.NoImport | 4, "NoImport, IsUnique" },
            { (byte)AddParamsFlags.NoImport | 5, "NoImport, Layer, IsUnique" },
            { (byte)AddParamsFlags.NoImport | 6, "NoImport, Order, IsUnique" },
            { (byte)AddParamsFlags.NoImport | 7, "NoImport, Layer, Order, IsUnique" },
            { byte.MaxValue, "NoImport, Layer, Order, IsUnique" },
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //return !property.isExpanded ? 
            //    EditorGUIUtility.singleLineHeight + Spacing : 
            //    EditorGUIUtility.singleLineHeight * 5f + Spacing * 4f;
            return EditorGUI.GetPropertyHeight(property);
        }
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            Event e = Event.current;

            var flagsProp = property.FindPropertyRelative("flags");

            AddParamsFlags flags = (AddParamsFlags)flagsProp.enumValueFlag;

            var (foldoutRect, contentRect) = position.VerticalSliceTop(OneLineHeight + Spacing);
            var (fieldsRects, checkboxRects) = contentRect.HorizontalSliceRight(OneLineHeight);

            Rect labelField;
            (foldoutRect, labelField) = foldoutRect.HorizontalSliceLeft(EditorGUIUtility.labelWidth);


            using (EcsGUI.CheckChanged())
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

                using (EcsGUI.SetAlignment(EditorStyles.miniLabel, TextAnchor.MiddleRight))
                {
                    if (_labelCache.TryGetValue((byte)(flags.Normalize()), out string str) == false)
                    {
                        str = _labelCache[0];
                    }
                    GUI.Label(labelField, str, EditorStyles.miniLabel);
                }

                if (property.isExpanded == false)
                {
                    goto exti;
                }

                //property.isExpanded = true;
                using (EcsGUI.UpIndentLevel())
                {
                    checkboxRects = checkboxRects.Move(EditorGUIUtility.standardVerticalSpacing, 0);

                    Rect checkboxRect = checkboxRects;
                    checkboxRect.height = OneLineHeight;
                    Rect fieldRect = fieldsRects;
                    fieldRect.height = OneLineHeight;

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

                    checkboxRect = checkboxRect.Move(0, OneLineHeight + Spacing);
                    fieldRect = fieldRect.Move(0, OneLineHeight + Spacing);

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

                    checkboxRect = checkboxRect.Move(0, OneLineHeight + Spacing);
                    fieldRect = fieldRect.Move(0, OneLineHeight + Spacing);

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

                    if (EcsGUI.Changed)
                    {
                        flagsProp.enumValueFlag = (int)flags;
                    }

                    checkboxRect = checkboxRect.Move(0, OneLineHeight + Spacing);
                    fieldRect = fieldRect.Move(0, OneLineHeight + Spacing);
                    fieldRect.xMax += OneLineHeight + Spacing;
                    property.Next(false);
                    EditorGUI.PropertyField(fieldRect, property, UnityEditorUtility.GetLabel(property.displayName), true);
                }

                exti:;

                //EcsGUI.Changed = Event.current.type == EventType.MouseUp;
                if (EcsGUI.Changed)
                {
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
#endif
