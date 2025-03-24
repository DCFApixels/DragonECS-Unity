#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class PipelineTemplateEditorBase : ExtendedEditor<IPipelineTemplate>
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

        protected abstract bool IsSO { get; }

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
            if (IsSO)
            {
                EcsGUI.Layout.ManuallySerializeButton(targets);
            }

            if (IsMultipleTargets)
            {
                GUILayout.Label("Multi-object editing not supported.", EditorStyles.helpBox);
                return;
            }

            EcsGUI.Changed = GUILayout.Button("Validate");

            DrawLayoutNameList(_layersProp);
            EditorGUILayout.PropertyField(_parametersProp, UnityEditorUtility.GetLabel(_parametersProp.displayName));
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
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
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
    internal class ScriptablePipelineTemplateEditor : PipelineTemplateEditorBase
    {
        protected override bool IsSO => true;
    }
    [CustomEditor(typeof(MonoPipelineTemplate), true)]
    internal class MonoPipelineTemplateEditor : PipelineTemplateEditorBase
    {
        protected override bool IsSO => false;
    }
}
#endif