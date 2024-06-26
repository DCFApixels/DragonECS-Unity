﻿#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class EntityTemplateEditorBase : Editor
    {
        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        private GenericMenu _genericMenu;
        private bool _isInit = false;

        private static ComponentColorMode AutoColorMode
        {
            get { return SettingsPrefs.instance.ComponentColorMode; }
            set { SettingsPrefs.instance.ComponentColorMode = value; }
        }

        #region Init
        private void Init()
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

                _genericMenu.AddItem(new GUIContent(name, description), false, OnAddComponent, dummy);
            }

            _isInit = true;
        }
        #endregion

        #region Add/Remove
        private void OnAddComponent(object obj)
        {
            Type componentType = obj.GetType();
            if (this.target is ITemplateInternal target)
            {
                SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
                for (int i = 0; i < componentsProp.arraySize; i++)
                {
                    if (componentsProp.GetArrayElementAtIndex(i).managedReferenceValue.GetType() == componentType)
                    {
                        return;
                    }
                }

                int index = componentsProp.arraySize;
                componentsProp.InsertArrayElementAtIndex(index);
                componentsProp.GetArrayElementAtIndex(index).managedReferenceValue = ((IComponentTemplate)obj).Clone();

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this.target);
            }
        }
        private void OnRemoveComponentAt(int index)
        {
            if (this.target is ITemplateInternal target)
            {
                SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
                componentsProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this.target);
            }
        }
        #endregion

        protected void Draw(ITemplateInternal target)
        {
            Init();
            SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
            if (componentsProp == null)
            {
                return;
            }

            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));
            DrawTop(target);
            GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
            for (int i = componentsProp.arraySize - 1; i >= 0; i--)
            {
                DrawComponentData(componentsProp.GetArrayElementAtIndex(i), componentsProp.arraySize, i);
            }
            GUILayout.EndVertical();
        }
        private void DrawTop(ITemplateInternal target)
        {
            switch (EcsGUI.Layout.AddClearComponentButtons())
            {
                case EcsGUI.AddClearComponentButton.AddComponent:
                    Init();
                    _genericMenu.ShowAsContext();
                    break;
                case EcsGUI.AddClearComponentButton.Clear:
                    Init();
                    serializedObject.FindProperty(target.ComponentsPropertyName).ClearArray();
                    serializedObject.ApplyModifiedProperties();
                    break;
            }
        }

        private void DrawComponentData(SerializedProperty componentRefProp, int total, int index)
        {
            IComponentTemplate template = componentRefProp.managedReferenceValue as IComponentTemplate;
            if (template == null || componentRefProp.managedReferenceValue == null)
            {
                DrawDamagedComponent_Replaced(componentRefProp, index);
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
                Debug.LogException(e, serializedObject.targetObject);
                DrawDamagedComponent(index, "Damaged component template.");
                return;
            }


            //сюда попадают уже валидные компоненты


            ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();
            string name = meta.Name;
            string description = meta.Description.Text;

            int propCount = EcsGUI.GetChildPropertiesCount(componentProperty, componentType, out bool isEmpty);

            float padding = EditorGUIUtility.standardVerticalSpacing;

            Color panelColor = EcsGUI.SelectPanelColor(meta, index, total).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

            Rect optionButton = GUILayoutUtility.GetLastRect();
            optionButton.yMin = optionButton.yMax;
            optionButton.yMax += HeadIconsRect.height;
            optionButton.xMin = optionButton.xMax - 64;
            optionButton.center += Vector2.up * padding * 2f;
            bool cancelExpanded = EcsGUI.HitTest(optionButton) && Event.current.type == EventType.MouseUp;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(alphaPanelColor));

            #region Draw Component Block 
            //Close button
            optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
            if (EcsGUI.CloseButton(optionButton))
            {
                OnRemoveComponentAt(index);
                return;
            }
            //Canceling isExpanded
            if (cancelExpanded)
            {
                componentProperty.isExpanded = !componentProperty.isExpanded;
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
                EcsGUI.Layout.DrawEmptyComponentProperty(componentRefProp, name, isEmpty);
            }
            else
            {
                GUIContent label = UnityEditorUtility.GetLabel(name);
                if (componentProperty.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.PropertyField(componentProperty, label, true);
                }
                else
                {
                    Rect r = RectUtility.AddPadding(GUILayoutUtility.GetRect(label, EditorStyles.objectField), 0, 20f, 0, 0);
                    EditorGUI.PropertyField(r, componentProperty, label, true);
                }
            }
            #endregion

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                componentProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(componentProperty.serializedObject.targetObject);
            }
        }
        private void DrawDamagedComponent_Replaced(SerializedProperty componentRefProp, int index)
        {
            DrawDamagedComponent(index, $"Damaged component template. If the problem occurred after renaming a component or initializer. use MovedFromAttrubute");
        }
        private void DrawDamagedComponent(int index, string message)
        {
            Rect removeButtonRect = GUILayoutUtility.GetLastRect();

            GUILayout.BeginHorizontal();

            float padding = EditorGUIUtility.standardVerticalSpacing;

            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += HeadIconsRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - HeadIconsRect.width;
            removeButtonRect.center += Vector2.up * padding * 2f;

            bool isRemoveComponent = EcsGUI.CloseButton(removeButtonRect);

            EditorGUILayout.HelpBox(message, MessageType.Warning);

            if (isRemoveComponent)
            {
                OnRemoveComponentAt(index);
            }

            GUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(ScriptableEntityTemplate), true)]
    internal class EntityTemplatePresetEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    internal class EntityTemplateEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
}
#endif    
