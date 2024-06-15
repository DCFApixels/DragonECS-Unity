#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(ComponentTemplateProperty), true)]
    internal class ComponentTemplatePropertyDrawer : PropertyDrawer
    {
        private ComponentTemplateReferenceDrawer _drawer = new ComponentTemplateReferenceDrawer();
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            return _drawer.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            _drawer.OnGUI(position, property, label);
        }
    }
    [CustomPropertyDrawer(typeof(ComponentTemplateReferenceAttribute), true)]
    internal class ComponentTemplateReferenceDrawer : PropertyDrawer
    {
        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        private float Padding => EditorGUIUtility.standardVerticalSpacing;
        private float SingleLineWithPadding => EditorGUIUtility.singleLineHeight + Padding * 4f;

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
            if (property.propertyType == SerializedPropertyType.ManagedReference == false)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
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

            int propCount = EcsGUI.GetChildPropertiesCount(property);

            return (propCount <= 0 ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(property, label)) + Padding * 4f;
        }



        public override void OnGUI(Rect position, SerializedProperty componentRefProp, GUIContent label)
        {
            if (componentRefProp.propertyType == SerializedPropertyType.ManagedReference == false)
            {
                EditorGUI.PropertyField(position, componentRefProp, label, true);
                return;
            }

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

            int propCount = EcsGUI.GetChildPropertiesCount(componentProperty);

            Color panelColor = EcsGUI.SelectPanelColor(meta, positionCountr, -1).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;


            EditorGUI.BeginChangeCheck();
            EditorGUI.DrawRect(position, alphaPanelColor);

            Rect paddingPosition = RectUtility.AddPadding(position, Padding * 2f);

            Rect optionButton = position;
            optionButton.center -= new Vector2(0, optionButton.height);
            optionButton.yMin = optionButton.yMax;
            optionButton.yMax += HeadIconsRect.height;
            optionButton.xMin = optionButton.xMax - 64;
            optionButton.center += Vector2.up * Padding * 1f;
            if (EcsGUI.HitTest(optionButton) && Event.current.type == EventType.MouseUp)
            {
                componentProperty.isExpanded = !componentProperty.isExpanded;
            }

            #region Draw Component Block 
            //Close button
            optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
            if (EcsGUI.CloseButton(optionButton))
            {
                componentRefProp.managedReferenceValue = null;
            }
            //Edit script button
            if (UnityEditorUtility.TryGetScriptAsset(componentType, out MonoScript script))
            {
                optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                EcsGUI.ScriptAssetButton(optionButton, script);
            }
            //Description icon
            if (string.IsNullOrEmpty(description) == false)
            {
                optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                EcsGUI.DescriptionIcon(optionButton, description);
            }

            if (propCount <= 0)
            {
                EcsGUI.DrawEmptyComponentProperty(paddingPosition, componentRefProp, label, false);
            }
            else
            {
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
            if (string.IsNullOrEmpty(label.text))
            {
                EditorGUI.indentLevel++;
                EditorGUI.PrefixLabel(position.AddPadding(0, 0, 0, position.height - SingleLineWithPadding), UnityEditorUtility.GetLabel(name));
                EditorGUI.indentLevel--;
            }
            else
            {
                GUI.Label(position.AddPadding(EditorGUIUtility.labelWidth, 0, 0, position.height - SingleLineWithPadding), name);
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