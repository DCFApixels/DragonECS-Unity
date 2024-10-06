using System;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal interface IReferenceButtonAttribute
    {
        Type[] PredicateTypes { get; }
        bool IsHideButtonIfNotNull { get; }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ComponentTemplateReferenceAttribute), true)]
    [CustomPropertyDrawer(typeof(ReferenceButtonAttribute), true)]
    internal sealed class ReferenceButtonAttributeDrawer : ExtendedPropertyDrawer<IReferenceButtonAttribute>
    {
        protected override void OnInit()
        {
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsGenericType)
            {
                if (fieldType.IsGenericTypeDefinition == false)
                {
                    fieldType = fieldType.GetGenericTypeDefinition();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init();
            if (property.managedReferenceValue != null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return OneLineHeight;
            }
        }

        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsArrayElement)
            {
                label = UnityEditorUtility.GetLabelTemp();
            }
            Rect selButtnoRect = position;
            selButtnoRect.height = OneLineHeight;
            DrawSelectionPopupButton(selButtnoRect, property, label);

            if (property.managedReferenceValue != null)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(position, label);
                EditorGUI.EndProperty();
            }
        }

        private void DrawSelectionPopupButton(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect buttonRect = IsArrayElement ? position : position.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f); ;
            EcsGUI.DrawSelectReferenceButton(buttonRect, property, Attribute.PredicateTypes.Length == 0 ? new Type[1] { fieldInfo.FieldType } : Attribute.PredicateTypes, Attribute.IsHideButtonIfNotNull);
        }
    }
}
#endif