using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal class CustomToggleAttribute : PropertyAttribute
    {
        public string Name;
        public bool IsInverted;
        public bool IsLeft;
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using UnityEditor;
    [CustomPropertyDrawer(typeof(CustomToggleAttribute))]
    internal class LeftToggleAttributeDrawer : ExtendedPropertyDrawer<CustomToggleAttribute>
    {
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                EditorGUI.PropertyField(position, property, label);
            }

            if (string.IsNullOrEmpty(Attribute.Name) == false)
            {
                label.text = Attribute.Name;
            }

            bool value = property.boolValue;
            if (Attribute.IsInverted)
            {
                value = !value;
            }

            if (Attribute.IsLeft)
            {
                value = EditorGUI.ToggleLeft(position, label, value);
            }

            value = EditorGUI.ToggleLeft(position, label, value);

            if (Attribute.IsInverted)
            {
                value = !value;
            }

            property.boolValue = value;
        }
    }
}
#endif