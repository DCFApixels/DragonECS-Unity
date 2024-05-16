﻿#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{

    [CustomPropertyDrawer(typeof(ComponentTemplateReferenceAttribute), true)]
    public class ComponentTemplateReferenceDrawer : PropertyDrawer
    {
        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        private float Padding => EditorGUIUtility.standardVerticalSpacing;

        private const float DamagedComponentHeight = 18f * 2f;

        private static bool _isInit;
        private static GenericMenu _genericMenu;
        #region Init
        private static void Init()
        {
            if (_genericMenu == null) { _isInit = false; }
            if (_isInit) { return; }

            _genericMenu = new GenericMenu();

            var componentTemplateDummies = ComponentTemplateTypeCache.Dummies;
            foreach (var dummy in componentTemplateDummies)
            {
                if (dummy.Type.GetCustomAttribute<SerializableAttribute>() == null)
                {
                    Debug.LogWarning($"Type {dummy.Type.Name} does not have the [Serializable] attribute");
                    continue;
                }
                ITypeMeta meta = dummy is ITypeMeta metaOverride ? metaOverride : dummy.Type.ToMeta();
                string name = meta.Name;
                string description = meta.Description.Text;
                MetaGroup group = meta.Group;

                if (group.Name.Length > 0)
                {
                    name = group.Name + name;
                }

                if (string.IsNullOrEmpty(description) == false)
                {
                    name = $"{name} [i]";
                }
                _genericMenu.AddItem(new GUIContent(name, description), false, SelectComponent, dummy);
            }

            _isInit = true;
        }
        [ThreadStatic]
        private static SerializedProperty currentProperty;
        private static void SelectComponent(object dummy)
        {
            currentProperty.managedReferenceValue = ((IComponentTemplate)dummy).Clone();
            currentProperty.isExpanded = false;
            currentProperty.serializedObject.ApplyModifiedProperties();
        }
        #endregion


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            IComponentTemplate template = property.managedReferenceValue as IComponentTemplate;
            if (template == null || property.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight + Padding * 2f;
            }

            try
            {
                ComponentTemplateBase customTemplate = property.managedReferenceValue as ComponentTemplateBase;
                if (customTemplate != null)
                {
                    property = property.FindPropertyRelative("component");
                }
            }
            catch (Exception)
            {
                property = null;
            }
            if (property == null)
            {
                return DamagedComponentHeight;
            }

            var propsCounter = property.Copy();
            int lastDepth = propsCounter.depth;
            bool next = propsCounter.Next(true) && lastDepth < propsCounter.depth;
            int propCount = next ? -1 : 0;
            while (next)
            {
                propCount++;
                next = propsCounter.Next(false);
            }
            bool isEmpty = propCount <= 0;

            
            return (isEmpty ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(property, label)) + Padding * 4f;
        }



        public override void OnGUI(Rect position, SerializedProperty componentRefProp, GUIContent label)
        {
            Init();
            var counter = componentRefProp.Copy();

            int positionCountr = int.MaxValue;
            while (counter.NextVisible(false))
            {
                positionCountr--;
            }

            IComponentTemplate template = componentRefProp.managedReferenceValue as IComponentTemplate;
            if (template == null || componentRefProp.managedReferenceValue == null)
            {
                DrawSelectionPopup(position, componentRefProp, label);
                return;
            }

            Type componentType;
            SerializedProperty componentProperty = componentRefProp;
            try
            {
                ComponentTemplateBase customTemplate = componentProperty.managedReferenceValue as ComponentTemplateBase;
                if (customTemplate != null)
                {
                    componentProperty = componentRefProp.FindPropertyRelative("component");
                    componentType = customTemplate.GetType().GetField("component", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FieldType;
                }
                else
                {
                    componentType = componentProperty.managedReferenceValue.GetType();
                }

                if (componentType == null || componentProperty == null)
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, componentRefProp.serializedObject.targetObject);
                DrawDamagedComponent(position, "Damaged component template.");
                return;
            }

            //сюда попадают уже валидные компоненты

            ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();
            string name = meta.Name;
            string description = meta.Description.Text;

            int propCount = EcsGUI.GetChildPropertiesCount(componentProperty, componentType, out bool isEmpty);

            Color panelColor = EcsGUI.SelectPanelColor(meta, positionCountr, -1).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;


            EditorGUI.BeginChangeCheck();
            GUI.Box(position, "", UnityEditorUtility.GetStyle(alphaPanelColor));

            Rect paddingPosition = RectUtility.AddPadding(position, Padding * 2f);

            #region Draw Component Block 
            Rect removeButtonRect = position;
            removeButtonRect.center -= new Vector2(0, removeButtonRect.height);
            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += HeadIconsRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - HeadIconsRect.width;
            removeButtonRect.center += Vector2.up * Padding * 1f;

            bool isRemoveComponent = EcsGUI.CloseButton(removeButtonRect);

            if (propCount <= 0)
            {
                label.text = $"{label.text} ({name})";
                EditorGUI.LabelField(paddingPosition, label);
                Rect emptyPos = paddingPosition;
                emptyPos.xMin += EditorGUIUtility.labelWidth;
                if (isEmpty)
                {
                    using (new EcsGUI.ContentColorScope(1f, 1f, 1f, 0.4f))
                    {
                        GUI.Label(emptyPos, "empty");
                    }
                }
                EditorGUI.BeginProperty(paddingPosition, label, componentRefProp);
                EditorGUI.EndProperty();
            }
            else
            {
                label.text = $"{label.text} ({name})";
                if (componentProperty.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUI.PropertyField(paddingPosition, componentProperty, label, true);
                }
                else
                {
                    Rect r = RectUtility.AddPadding(paddingPosition, 0, 20f, 0, 0);
                    EditorGUI.PropertyField(r, componentProperty, label, true);
                }
            }
            if (isRemoveComponent)
            {
                componentRefProp.managedReferenceValue = null;
            }
            if (string.IsNullOrEmpty(description) == false)
            {
                Rect tooltipIconRect = HeadIconsRect;
                tooltipIconRect.center = removeButtonRect.center;
                tooltipIconRect.center -= Vector2.right * tooltipIconRect.width;
                EcsGUI.DescriptionIcon(tooltipIconRect, description);
            }
            #endregion

            if (EditorGUI.EndChangeCheck())
            {
                componentProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(componentProperty.serializedObject.targetObject);
            }
        }

        private void DrawSelectionPopup(Rect position, SerializedProperty componentRefProp, GUIContent label)
        {
            EditorGUI.LabelField(position, label);
            //Rect buttonRect = RectUtility.AddPadding(position, EditorGUIUtility.labelWidth, 20f, 0f, 0f);
            Rect buttonRect = RectUtility.AddPadding(position, EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            if (GUI.Button(buttonRect, "Select"))
            {
                currentProperty = componentRefProp;
                _genericMenu.ShowAsContext();
            }
        }
        private void DrawDamagedComponent(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Warning);
        }
    }
}
#endif