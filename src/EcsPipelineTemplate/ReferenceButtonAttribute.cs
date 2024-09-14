using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal sealed class ReferenceButtonAttribute : PropertyAttribute
    {
        public readonly Type[] PredicateTypes;
        public readonly bool IsHideButtonIfNotNull;
        public ReferenceButtonAttribute(bool isHideButtonIfNotNull = false) : this(isHideButtonIfNotNull, Array.Empty<Type>()) { }
        public ReferenceButtonAttribute(params Type[] predicateTypes) : this(false, predicateTypes) { }
        public ReferenceButtonAttribute(bool isHideButtonIfNotNull, params Type[] predicateTypes)
        {
            IsHideButtonIfNotNull = isHideButtonIfNotNull;
            PredicateTypes = predicateTypes;
            Array.Sort(predicateTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using System;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReferenceButtonAttribute))]
    internal sealed class ReferenceButtonAttributeDrawer : ExtendedPropertyDrawer<ReferenceButtonAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
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
            Rect selButtnoRect = position;
            selButtnoRect.height = OneLineHeight;
            DrawSelectionPopup(selButtnoRect, property, label);

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

        private void DrawSelectionPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect buttonRect = position.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            EcsGUI.DrawSelectReferenceButton(buttonRect, property, Attribute.PredicateTypes.Length == 0 ? new Type[1] { fieldInfo.FieldType } : Attribute.PredicateTypes, Attribute.IsHideButtonIfNotNull);
        }
    }
}
#endif