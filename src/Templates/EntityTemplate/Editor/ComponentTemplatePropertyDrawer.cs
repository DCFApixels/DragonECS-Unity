#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(ComponentTemplateProperty), true)]
    internal class ComponentTemplatePropertyDrawer : ExtendedPropertyDrawer
    {
        private ComponentTemplateReferenceDrawer _drawer = new ComponentTemplateReferenceDrawer();
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            _drawer.StaticInit();
            _drawer.Init();
            return _drawer.GetPropertyHeight(property, label);
        }
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            var root = property.Copy();
            property.Next(true);
            _drawer.StaticInit();
            _drawer.Init();
            _drawer.Draw(position, root, property, label);
        }
    }
    [CustomPropertyDrawer(typeof(ComponentTemplateReferenceAttribute), true)]
    internal class ComponentTemplateReferenceDrawer : ExtendedPropertyDrawer<ComponentTemplateReferenceAttribute>
    {
        private const float DamagedComponentHeight = 18f * 2f;
        private static ComponentTemplatesDropDown _componentDropDown;

        #region Properties
        private float SingleLineWithPadding => OneLineHeight + Padding * 4f;
        private float Padding => Spacing;
        protected override bool IsStaticInit => _componentDropDown != null;
        #endregion

        #region Init
        protected override void OnStaticInit()
        {
            _componentDropDown = new ComponentTemplatesDropDown();
            _componentDropDown.OnSelected += SelectComponent;
        }

        [ThreadStatic]
        private static SerializedProperty currentProperty;
        private static void SelectComponent(ComponentTemplatesDropDown.Item item)
        {
            //EcsGUI.Changed = true;
            currentProperty.managedReferenceValue = item.Obj.Clone_Reflection();
            currentProperty.isExpanded = false;
            currentProperty.serializedObject.ApplyModifiedProperties();
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            #region No SerializeReference
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            #endregion

            var instance = property.managedReferenceValue;
            IComponentTemplate template = instance as IComponentTemplate;

            if (template == null || instance == null)
            {
                return EditorGUIUtility.singleLineHeight + Padding * 2f;
            }

            try
            {
                if (instance is ComponentTemplateBase customTemplate)
                {
                    property = property.FindPropertyRelative("component");
                }
            }
            catch
            {
                property = null;
            }
            if (property == null)
            {
                return DamagedComponentHeight;
            }

            int propCount = EcsGUI.GetChildPropertiesCount(property);

            return (propCount <= 0 ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(property, label)) + Padding * 4f;
        }

        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            Draw(position, property, property, label);
        }
        public void Draw(Rect position, SerializedProperty rootProperty, SerializedProperty property, GUIContent label)
        {
            #region No SerializeReference
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }
            #endregion

            var instance = property.managedReferenceValue;
            IComponentTemplate template = instance as IComponentTemplate;

            if (template == null || instance == null)
            {
                DrawSelectionPopup(position, property, label);
                return;
            }

            SerializedProperty componentProp = property;
            if (componentProp.managedReferenceValue is ComponentTemplateBase customTemplate)
            {
                componentProp = property.FindPropertyRelative("component");
            }
            if (componentProp == null)
            {
                DrawDamagedComponent(position, "Damaged component template.");
                return;
            }

            ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();

            Rect rect = position;
            if (EcsGUI.DrawTypeMetaBlock(ref rect, rootProperty, meta))
            {
                return;
            }

            label.text = meta.Name;
            if (componentProp.propertyType == SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(rect, componentProp, label, true);
            }
            else
            {
                EditorGUI.PropertyField(rect.AddPadding(0, 20f, 0, 0), componentProp, label, true);
            }
        }

        private void DrawSelectionPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label);
            Rect buttonRect = RectUtility.AddPadding(position, EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            if (GUI.Button(buttonRect, "Select"))
            {
                currentProperty = property;
                _componentDropDown.Show(buttonRect);
            }
        }
        private void DrawDamagedComponent(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Warning);
        }
    }
}
#endif