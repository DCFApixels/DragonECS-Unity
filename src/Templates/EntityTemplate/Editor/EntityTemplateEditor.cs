#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class EntityTemplateEditorBase<T> : ExtendedEditor<ITemplateInternal>
    {
        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        private ComponentDropDown _componentDropDown;

        private SerializedProperty _componentsProp;
        private ReorderableList _reorderableComponentsList;

        #region Init
        protected override bool IsInit { get { return _componentDropDown != null; } }
        protected override void OnInit()
        {
            _componentDropDown = new ComponentDropDown();

            _componentsProp = serializedObject.FindProperty(Target.ComponentsPropertyName);

            _reorderableComponentsList = new ReorderableList(serializedObject, _componentsProp, true, false, false, false);
            _reorderableComponentsList.onAddCallback += OnReorderableComponentsListAdd;
            _reorderableComponentsList.onRemoveCallback += OnReorderableListRemove;
            _reorderableComponentsList.drawElementCallback += OnReorderableListDrawEmptyElement;
            _reorderableComponentsList.drawElementBackgroundCallback += OnReorderableComponentsListDrawElement;
            _reorderableComponentsList.drawNoneElementCallback += OnReorderableComponentsListDrawNoneElement;
            _reorderableComponentsList.elementHeightCallback += OnReorderableComponentsListElementHeight;
            _reorderableComponentsList.onReorderCallback += OnReorderableListReorder;
            _reorderableComponentsList.showDefaultBackground = false;
            _reorderableComponentsList.footerHeight = 0f;
            _reorderableComponentsList.headerHeight = 0f;
            _reorderableComponentsList.elementHeight = 0f;
        }

        #region ReorderableComponentsList
        private void OnReorderableComponentsListDrawNoneElement(Rect rect) { }
        private void OnReorderableListDrawEmptyElement(Rect rect, int index, bool isActive, bool isFocused) { }

        private void OnReorderableListReorder(ReorderableList list)
        {
            EcsGUI.Changed = true;
        }

        private SerializedProperty GetTargetProperty(SerializedProperty prop)
        {
            IComponentTemplate template = prop.managedReferenceValue as IComponentTemplate;
            if (template == null || prop.managedReferenceValue == null)
            {
                //DrawDamagedComponent_Replaced(componentRefProp, index);
                return prop;
            }

            SerializedProperty componentProperty = prop;
            try
            {
                if (componentProperty.managedReferenceValue is ComponentTemplateBase customTemplate)
                {
                    componentProperty = prop.FindPropertyRelative("component");
                }

                if (componentProperty == null)
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, serializedObject.targetObject);
                //DrawDamagedComponent(index, "Damaged component template.");
                return prop;
            }

            return componentProperty;
        }
        private float OnReorderableComponentsListElementHeight(int index)
        {
            var componentProperty = GetTargetProperty(_componentsProp.GetArrayElementAtIndex(index));
            float result = EditorGUI.GetPropertyHeight(componentProperty);
            return EcsGUI.GetTypeMetaBlockHeight(result) + Spacing * 2f;
        }
        private void OnReorderableComponentsListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || Event.current.type == EventType.Used) { return; }
            rect = rect.AddPadding(OneLineHeight + Spacing, Spacing * 2f, Spacing, Spacing);
            using (EcsGUI.CheckChanged())
            {
                SerializedProperty prop = _componentsProp.GetArrayElementAtIndex(index);

                IComponentTemplate template = prop.managedReferenceValue as IComponentTemplate;
                if (template == null || prop.managedReferenceValue == null)
                {
                    DrawDamagedComponent_Replaced(prop, index);
                    return;
                }

                var componentProp = GetTargetProperty(prop);


                ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();

                if (EcsGUI.DrawTypeMetaElementBlock(ref rect, _componentsProp, index, componentProp, meta))
                {
                    return;
                }


                GUIContent label = UnityEditorUtility.GetLabel(meta.Name);
                if (componentProp.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUI.PropertyField(rect, componentProp, label, true);
                }
                else
                {
                    EditorGUI.PropertyField(rect.AddPadding(0, 20f, 0, 0), componentProp, label, true);
                }

            }
        }

        private void OnReorderableComponentsListAdd(ReorderableList list)
        {
            list.serializedProperty.arraySize += 1;
            list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).ResetValues();
            EcsGUI.Changed = true;
        }
        private void OnReorderableListRemove(ReorderableList list)
        {
            if (list.selectedIndices.Count <= 0)
            {
                if (list.serializedProperty.arraySize > 0)
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                }
                return;
            }
            for (int i = list.selectedIndices.Count - 1; i >= 0; i--)
            {
                list.serializedProperty.DeleteArrayElementAtIndex(list.selectedIndices[i]);
            }
            EcsGUI.Changed = true;
        }
        #endregion

        #endregion

        #region Add/Remove
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

        protected override void DrawCustom()
        {
            Init();

            if (_componentsProp == null)
            {
                return;
            }

            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                DrawTop(Target, _componentsProp);
                _reorderableComponentsList.DoLayoutList();
                //GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
                //for (int i = _componentsProp.arraySize - 1; i >= 0; i--)
                //{
                //    DrawComponentData(_componentsProp.GetArrayElementAtIndex(i), _componentsProp.arraySize, i);
                //}
            }
        }
        private void DrawTop(ITemplateInternal target, SerializedProperty componentsProp)
        {
            switch (EcsGUI.Layout.AddClearComponentButtons(out Rect rect))
            {
                case EcsGUI.AddClearButton.Add:
                    Init();
                    _componentDropDown.OpenForArray(rect, componentsProp, true);
                    break;
                case EcsGUI.AddClearButton.Clear:
                    Init();
                    componentsProp.ClearArray();
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
            bool cancelExpanded = EcsGUI.ClickTest(optionButton);

            #region Draw Component Block 
            using (EcsGUI.CheckChanged()) using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(alphaPanelColor)))
            {
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
                if (ScriptsCache.TryGetScriptAsset(meta.FindRootTypeMeta(), out MonoScript script))
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

                if (EcsGUI.Changed)
                {
                    componentProperty.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(componentProperty.serializedObject.targetObject);
                }
            }
            #endregion
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
    internal class EntityTemplatePresetEditor : EntityTemplateEditorBase<ScriptableEntityTemplate>
    {
        //protected override void DrawCustom()
        //{
        //    Draw(Target);
        //}
    }
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    internal class EntityTemplateEditor : EntityTemplateEditorBase<MonoEntityTemplate>
    {
        //protected override void DrawCustom()
        //{
        //    Draw(Target);
        //}
    }
}
#endif    
