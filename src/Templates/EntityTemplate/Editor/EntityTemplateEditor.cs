#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using DCFApixels.DragonECS.Unity.RefRepairer.Editors;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class EntityTemplateEditorBase : ExtendedEditor
    {
        private DragonFieldDropDown _componentDropDown;

        private SerializedProperty _componentTemplatesProp;
        private SerializedProperty _templatesProp;
        private ReorderableList _reorderableComponentsList;
        private int _reorderableComponentsListLastCount;

        private static readonly Type[] _predicateTypes = new Type[] { typeof(IComponentTemplate), typeof(IEcsComponentMember) };

        protected abstract bool IsSO { get; }

        #region Init
        protected override bool IsInit { get { return _componentDropDown != null; } }
        protected override void OnInit()
        {
            _componentDropDown = DragonFieldDropDown.Get(new PredicateTypesKey(typeof(ITemplateNode), _predicateTypes, Type.EmptyTypes));

            _componentTemplatesProp = serializedObject.FindProperty("_componentTemplates");
            _templatesProp = serializedObject.FindProperty("_templates");

            _reorderableComponentsList = new ReorderableList(serializedObject, _componentTemplatesProp, true, false, false, false);
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

            _reorderableComponentsListLastCount = _reorderableComponentsList.count;
        }

        #region ReorderableComponentsList
        private void OnReorderableComponentsListDrawNoneElement(Rect rect) { }
        private void OnReorderableListDrawEmptyElement(Rect rect, int index, bool isActive, bool isFocused) { }

        private void OnReorderableListReorder(ReorderableList list)
        {
            DragonGUI.Changed = true;
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
            SerializedProperty prop = _componentTemplatesProp.GetArrayElementAtIndex(index);
            GUIContent label = UnityEditorUtility.GetLabelTemp();
            return EditorGUI.GetPropertyHeight(prop, label) + Spacing * 2f;
            //var componentProperty = GetTargetProperty(_componentTemplatesProp.GetArrayElementAtIndex(index));
            //float result = EditorGUI.GetPropertyHeight(componentProperty);
            //return EcsGUI.GetTypeMetaBlockHeight(result) + Spacing * 2f;
        }
        private void OnReorderableComponentsListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || Event.current.type == EventType.Used) { return; }
            SerializedProperty prop = _componentTemplatesProp.GetArrayElementAtIndex(index);
            GUIContent label = UnityEditorUtility.GetLabelTemp();
            rect = rect.AddPadding(OneLineHeight + Spacing, Spacing * 2f, Spacing, Spacing);

            EditorGUI.PropertyField(rect, prop, label);
            return;
            //rect = rect.AddPadding(OneLineHeight + Spacing, Spacing * 2f, Spacing, Spacing);
            //using (EcsGUI.CheckChanged())
            //{
            //    SerializedProperty prop = _componentTemplatesProp.GetArrayElementAtIndex(index);
            //
            //    var template = prop.managedReferenceValue;
            //    if (template == null)
            //    {
            //        //DrawDamagedComponent_Replaced(prop, index);
            //        EditorGUI.PropertyField(rect, prop, UnityEditorUtility.GetLabel(prop.displayName), true);
            //        return;
            //    }
            //    IComponentTemplate componentTemplate = template as IComponentTemplate;
            //    var componentProp = GetTargetProperty(prop);
            //
            //    ITypeMeta meta = template as ITypeMeta;
            //    if(meta == null)
            //    {
            //        if (componentTemplate != null)
            //        {
            //            meta = componentTemplate.ComponentType.GetMeta();
            //        }
            //        else
            //        {
            //            meta = template.GetMeta();
            //        }
            //    }
            //
            //    if (EcsGUI.DrawTypeMetaElementBlock(ref rect, _componentTemplatesProp, index, componentProp, meta).skip)
            //    {
            //        return;
            //    }
            //
            //
            //    GUIContent label = UnityEditorUtility.GetLabel(meta.Name);
            //    if (componentProp.propertyType == SerializedPropertyType.Generic)
            //    {
            //        EditorGUI.PropertyField(rect, componentProp, label, true);
            //    }
            //    else
            //    {
            //        EditorGUI.PropertyField(rect.AddPadding(0, 20f, 0, 0), componentProp, label, true);
            //    }
            //
            //}
        }

        private void OnReorderableComponentsListAdd(ReorderableList list)
        {
            list.serializedProperty.arraySize += 1;
            list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).ResetValues();
            DragonGUI.Changed = true;
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
            DragonGUI.Changed = true;
        }
        #endregion

        #endregion

        protected override void DrawCustom()
        {
            Init();



            if (IsSO)
            {
                DragonGUI.Layout.ManuallySerializeButton(targets);
            }

            if (IsMultipleTargets)
            {
                GUILayout.Label("Multi-object editing not supported.", EditorStyles.helpBox);
                return;
            }
            else
            {
                //костыль который насильно заставляет _reorderableComponentsList пересчитать высоту
                if (_reorderableComponentsListLastCount != _reorderableComponentsList.count)
                {
                    DragonGUI.Changed = true;
                    _reorderableComponentsListLastCount = _reorderableComponentsList.count;
                }
            }

            if (IsMultipleTargets == false && SerializationUtility.HasManagedReferencesWithMissingTypes(target))
            {
                using (DragonGUI.Layout.BeginHorizontal(EditorStyles.helpBox))
                {
                    GUILayout.Label(UnityEditorUtility.GetLabel(Icons.Instance.WarningIcon), GUILayout.ExpandWidth(false));
                    using (DragonGUI.Layout.BeginVertical())
                    {
                        GUILayout.Label("This object contains SerializeReference types which are missing.", EditorStyles.miniLabel);
                        if (GUILayout.Button("Repaire References Tool", EditorStyles.miniButton, GUILayout.MaxWidth(200f)))
                        {
                            RefRepairerWindow.Open();
                        }
                    }
                }
            }


            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            using (DragonGUI.Disable)
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            while (iterator.NextVisible(false))
            {
                if ((_componentTemplatesProp != null && iterator.name == _componentTemplatesProp.name) ||
                    (_templatesProp != null && iterator.name == _templatesProp.name))
                {

                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            if (_templatesProp != null)
            {
                EditorGUILayout.PropertyField(_templatesProp, true);
            }
            using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
            {
                DrawTop(_componentTemplatesProp);
                _reorderableComponentsList.DoLayoutList();
            }
        }
        private void DrawTop(SerializedProperty componentsProp)
        {
            GUILayout.Space(2f);

            switch (DragonGUI.Layout.AddClearComponentButtons(out Rect rect))
            {
                case DragonGUI.AddClearButton.Add:
                    Init();
                    _componentDropDown.OpenForArray(rect, componentsProp, true);
                    break;
                case DragonGUI.AddClearButton.Clear:
                    Init();
                    componentsProp.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                    break;
            }
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableEntityTemplate), true)]
    internal class ScriptableEntityTemplateEditor : EntityTemplateEditorBase
    {
        //public override bool IsStaticData { get { return true; } }
        protected override bool IsSO => true;
    }
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    internal class MonoEntityTemplateEditor : EntityTemplateEditorBase
    {
        //public override bool IsStaticData { get { return false; } }
        protected override bool IsSO => false;
    }
}
#endif    
