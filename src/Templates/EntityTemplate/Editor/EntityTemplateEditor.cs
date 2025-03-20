#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using DCFApixels.DragonECS.Unity.RefRepairer.Editors;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class EntityTemplateEditorBase : ExtendedEditor<IEntityTemplateInternal>
    {
        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        private ComponentDropDown _componentDropDown;

        private SerializedProperty _componentsProp;
        private ReorderableList _reorderableComponentsList;

        protected abstract bool IsSO { get; }

        //public virtual bool IsStaticData { get { return false; } }

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
            if (componentProperty.managedReferenceValue is ComponentTemplateBase customTemplate)
            {
                componentProperty = prop.FindPropertyRelative("component");
            }

            return componentProperty == null ? prop : componentProperty;
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
                    //DrawDamagedComponent_Replaced(prop, index);
                    EditorGUI.PropertyField(rect, prop, UnityEditorUtility.GetLabel(prop.displayName), true);
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
            if (this.target is IEntityTemplateInternal target)
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

            if (IsSO)
            {
                EcsGUI.Layout.ManuallySerializeButton(target);
            }

            if (IsMultipleTargets == false && SerializationUtility.HasManagedReferencesWithMissingTypes(target))
            {
                using (EcsGUI.Layout.BeginHorizontal(EditorStyles.helpBox))
                {
                    GUILayout.Label(UnityEditorUtility.GetLabel(Icons.Instance.WarningIcon), GUILayout.ExpandWidth(false));
                    using (EcsGUI.Layout.BeginVertical())
                    {
                        GUILayout.Label("This object contains SerializeReference types which are missing.", EditorStyles.miniLabel);
                        if (GUILayout.Button("Repaire References Tool", EditorStyles.miniButton, GUILayout.MaxWidth(200f)))
                        {
                            RefRepairerWindow.Open();
                        }
                    }
                }
            }

            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
            {
                DrawTop(Target, _componentsProp);
                _reorderableComponentsList.DoLayoutList();
            }
        }
        private void DrawTop(IEntityTemplateInternal target, SerializedProperty componentsProp)
        {
            GUILayout.Space(2f);

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
    }

    [CustomEditor(typeof(ScriptableEntityTemplate), true)]
    internal class ScriptableEntityTemplateEditor : EntityTemplateEditorBase
    {
        //public override bool IsStaticData { get { return true; } }
        protected override bool IsSO => true;
    }
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    internal class MonoEntityTemplateEditor : EntityTemplateEditorBase
    {
        //public override bool IsStaticData { get { return false; } }
        protected override bool IsSO => false;
    }
}
#endif    
