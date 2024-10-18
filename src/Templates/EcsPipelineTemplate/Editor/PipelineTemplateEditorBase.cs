﻿#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(PipelineTemplateUtility.Record))]
    internal class PipelineTemplateUtilityRecordDrawer : ExtendedPropertyDrawer
    {
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsArrayElement == false)
            {
                Rect foldoutRect;
                (foldoutRect, position) = position.VerticalSliceTop(OneLineHeight + Spacing);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
                if (property.isExpanded == false)
                {
                    return;
                }
            }

            using (EcsGUI.SetIndentLevel(IsArrayElement ? EditorGUI.indentLevel : EditorGUI.indentLevel + 1))
            {
                Rect subPosition = position;
                int depth = -1;
                float height = 0f;

                property.Next(true);
                do
                {
                    subPosition.y += height;
                    height = EditorGUI.GetPropertyHeight(property);
                    subPosition.height = height;

                    EditorGUI.PropertyField(subPosition, property, true);

                } while (property.NextDepth(false, ref depth));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float result = 0f;
            if (IsArrayElement == false)
            {
                result += OneLineHeight;
                if (property.isExpanded == false)
                {
                    return result;
                }
            }


            property.Next(true);
            int depth = -1;
            do
            {
                result += EditorGUI.GetPropertyHeight(property, true);
            } while (property.NextDepth(false, ref depth));
            return result;
        }
    }
    [CustomEditor(typeof(ScriptablePipelineTemplate))]
    internal class PipelineTemplateEditorBase : ExtendedEditor<IPipelineTemplate>
    {
        private SerializedProperty _parametersProp;
        private SerializedProperty _layersProp;
        private SerializedProperty _recordsProp;
        private ReorderableList _reorderableLayersList;
        private ReorderableList _reorderableRecordsList;
        private SystemsDropDown _systemsDropDown;

        protected override bool IsInit
        {
            get => _reorderableLayersList != null && _reorderableRecordsList != null;
        }

        protected override void OnStaticInit() { }
        protected override void OnInit()
        {
            _parametersProp = FindProperty("_parameters");
            _layersProp = FindProperty("_layers");
            _recordsProp = FindProperty("_records");

            CreateLists();
        }
        private void CreateLists()
        {
            _reorderableLayersList = new ReorderableList(serializedObject, _layersProp, true, false, true, true);
            _reorderableLayersList.onAddCallback += OnReorderableLayersListAdd;
            _reorderableLayersList.onRemoveCallback += OnReorderableListRemove;
            _reorderableLayersList.drawElementCallback += OnReorderableLayersListDrawElement;
            _reorderableLayersList.onReorderCallback += OnReorderableListReorder;

            _reorderableRecordsList = new ReorderableList(serializedObject, _recordsProp, true, false, false, false);
            _reorderableRecordsList.onAddCallback += OnReorderableRecordsListAdd;
            _reorderableRecordsList.onRemoveCallback += OnReorderableListRemove;
            _reorderableRecordsList.drawElementCallback += OnReorderableListDrawEmptyElement;
            _reorderableRecordsList.drawElementBackgroundCallback += OnReorderableRecordsListDrawElement;
            _reorderableRecordsList.drawNoneElementCallback += OnReorderableListDrawNoneElement;
            _reorderableRecordsList.elementHeightCallback += OnReorderableRecordsListElementHeight;
            _reorderableRecordsList.onReorderCallback += OnReorderableListReorder;
            _reorderableRecordsList.showDefaultBackground = false;
            _reorderableRecordsList.footerHeight = 0f;
            _reorderableRecordsList.headerHeight = 0f;
            _reorderableRecordsList.elementHeight = 0f;

            _systemsDropDown = new SystemsDropDown();
        }

        #region _reorderableList
        private void OnReorderableListDrawNoneElement(Rect rect) { }
        private void OnReorderableListDrawEmptyElement(Rect rect, int index, bool isActive, bool isFocused) { }
        private void OnReorderableListReorder(ReorderableList list)
        {
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

        #region _reorderableLayersList
        private static readonly HashSet<string> _defaultLayersSet = new HashSet<string>(PipelineTemplateUtility.DefaultLayers);
        private void OnReorderableLayersListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (EcsGUI.CheckChanged())
            {
                var elementProp = _layersProp.GetArrayElementAtIndex(index);
                string str = elementProp.stringValue;
                using (EcsGUI.SetEnable(_defaultLayersSet.Contains(str) == false))
                {
                    elementProp.stringValue = EditorGUI.TextField(rect, str);
                }
            }
        }
        private void OnReorderableLayersListAdd(ReorderableList list)
        {
            list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
            var added = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            added.stringValue = $"Layer-{DateTime.Now.Ticks}";
            added.serializedObject.ApplyModifiedProperties();
            EcsGUI.Changed = true;
        }
        #endregion

        #region _reorderableRecordsList
        private void OnReorderableRecordsListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || Event.current.type == EventType.Used) { return; }
            rect = rect.AddPadding(OneLineHeight + Spacing, Spacing * 2f, Spacing, Spacing);
            using (EcsGUI.CheckChanged())
            {
                SerializedProperty prop = _recordsProp.GetArrayElementAtIndex(index);
                var targetProp = prop.FindPropertyRelative(nameof(PipelineTemplateUtility.Record.target));

                bool isNull = targetProp.managedReferenceValue == null;
                ITypeMeta meta = isNull ? null : targetProp.managedReferenceValue.GetMeta();

                if (EcsGUI.DrawTypeMetaElementBlock(ref rect, _recordsProp, index, prop, meta))
                {
                    return;
                }
                EditorGUI.PropertyField(rect, prop, true);
            }
        }
        private float OnReorderableRecordsListElementHeight(int index)
        {
            float result;
            result = EditorGUI.GetPropertyHeight(_recordsProp.GetArrayElementAtIndex(index));
            return EcsGUI.GetTypeMetaBlockHeight(result) + Spacing * 2f;
        }

        private void OnReorderableRecordsListAdd(ReorderableList list)
        {
            list.serializedProperty.arraySize += 1;
            list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1).ResetValues();
            EcsGUI.Changed = true;
        }

        #endregion

        protected override void DrawCustom()
        {
            EcsGUI.Changed = GUILayout.Button("Validate");

            EditorGUILayout.PropertyField(_parametersProp, UnityEditorUtility.GetLabel(_parametersProp.displayName));
            DrawLayoutNameList(_layersProp);
            DrawRecordList(_recordsProp);

            if (EcsGUI.Changed)
            {
                serializedObject.ApplyModifiedProperties();
                Validate();
                Repaint();
            }
        }

        private void Validate()
        {
            foreach (var target in Targets)
            {
                target.Validate();
                EditorUtility.SetDirty((UnityEngine.Object)target);
            }
            serializedObject.Update();
        }

        private void DrawLayoutNameList(SerializedProperty layersProp)
        {
            using (EcsGUI.Layout.BeginVertical())
            {
                GUILayout.Label(UnityEditorUtility.GetLabel(layersProp.displayName), EditorStyles.boldLabel);
                _reorderableLayersList.DoLayoutList();
            }
        }
        private void DrawRecordList(SerializedProperty recordsProp)
        {
            GUILayout.Label(UnityEditorUtility.GetLabel(recordsProp.displayName), EditorStyles.boldLabel);
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
            {
                GUILayout.Space(4f);

                switch (EcsGUI.Layout.AddClearSystemButtons(out Rect dropDownRect))
                {
                    case EcsGUI.AddClearButton.Add:
                        _systemsDropDown.OpenForArray(dropDownRect, recordsProp);
                        break;
                    case EcsGUI.AddClearButton.Clear:
                        recordsProp.ClearArray();
                        recordsProp.serializedObject.ApplyModifiedProperties();
                        break;
                }
                _reorderableRecordsList.DoLayoutList();
                //EditorGUILayout.PropertyField(recordsProp, UnityEditorUtility.GetLabel(recordsProp.displayName));
            }
        }
    }

    [CustomEditor(typeof(ScriptablePipelineTemplate), true)]
    internal class ScriptablePipelineTemplateEditor : PipelineTemplateEditorBase { }
    [CustomEditor(typeof(MonoPipelineTemplate), true)]
    internal class MonoPipelineTemplateEditor : PipelineTemplateEditorBase { }
}
#endif