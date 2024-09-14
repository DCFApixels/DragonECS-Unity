#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(EcsPipelineTemplateSO.Record))]
    //[CustomPropertyDrawer(typeof(EcsPipelineTemplate.Record))]
    internal class EcsPipelineTemplateSORecordDrawer : ExtendedPropertyDrawer
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
                property.Next(true);
                float height = 0f;
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
    [CustomEditor(typeof(EcsPipelineTemplateSO))]
    internal class EcsPipelineTemplateSOEditor : ExtendedEditor<EcsPipelineTemplateSO>
    {
        private SerializedProperty _layersProp;
        private SerializedProperty _recordsProp;
        private ReorderableList _reorderableLayersList;
        private ReorderableList _reorderableRecordsList;


        protected override bool IsInit
        {
            get => _reorderableLayersList != null && _reorderableRecordsList != null;
        }

        protected override void OnStaticInit() { }
        protected override void OnInit()
        {
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

            _reorderableRecordsList = new ReorderableList(serializedObject, _recordsProp, true, false, true, true);
            _reorderableRecordsList.onAddCallback += OnReorderableRecordsListAdd;
            _reorderableRecordsList.onRemoveCallback += OnReorderableListRemove;
            _reorderableRecordsList.drawElementCallback += OnReorderableRecordsListDrawElement;
            _reorderableRecordsList.elementHeightCallback += OnReorderableRecordsListElementHeight;
            _reorderableRecordsList.onReorderCallback += OnReorderableListReorder;
        }

        private void OnReorderableListReorder(ReorderableList list)
        {
            EcsGUI.Changed = true;
        }

        #region _reorderableList
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
        private void OnReorderableLayersListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            using (EcsGUI.CheckChanged())
            {
                var elementProp = _layersProp.GetArrayElementAtIndex(index);
                elementProp.stringValue = EditorGUI.TextField(rect, elementProp.stringValue);
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
            using (EcsGUI.CheckChanged())
            {
                var prop = _recordsProp.GetArrayElementAtIndex(index);

                var targetProp = prop.FindPropertyRelative(nameof(EcsPipelineTemplateSO.Record.target));
                var paramsProp = prop.FindPropertyRelative(nameof(EcsPipelineTemplateSO.Record.parameters));

                bool isNull = targetProp.managedReferenceValue == null;
                ITypeMeta meta = isNull ? null : targetProp.managedReferenceValue.GetMeta();

                if (EcsGUI.DrawTypeMetaBlock(ref rect, prop, meta))
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
            return EcsGUI.GetTypeMetaBlockHeight(result);
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
            using (EcsGUI.CheckChanged())
            {
                EditorGUI.BeginChangeCheck();

                DrawLayoutNameList(_layersProp);
                DrawRecordList(_recordsProp);

                EcsGUI.Changed = GUILayout.Button("Validate");
                if (EcsGUI.Changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    Validate();
                    Repaint();
                }
            }
        }

        private void Validate()
        {
            foreach (var target in Targets)
            {
                target.Validate();
                EditorUtility.SetDirty(target);
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
            using (EcsGUI.Layout.BeginVertical())
            {
                GUILayout.Label(UnityEditorUtility.GetLabel(recordsProp.displayName), EditorStyles.boldLabel);
                _reorderableRecordsList.DoLayoutList();
                //EditorGUILayout.PropertyField(recordsProp, UnityEditorUtility.GetLabel(recordsProp.displayName));
            }
        }
    }
}
#endif