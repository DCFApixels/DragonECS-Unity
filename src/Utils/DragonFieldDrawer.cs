#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Attributes;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReferenceDropDownAttribute), true)]
    [CustomPropertyDrawer(typeof(DragonMetaBlockAttribute), true)]
    [CustomPropertyDrawer(typeof(InlineInspectorAttribute), true)]
    internal class DragonFieldDrawer : ExtendedPropertyDrawer
    {
        private const float DamagedComponentHeight = 18f * 2f;
        private DragonFieldDropDown _dropDown;
        private PredicateTypesKey? _predicateOverride;

        private ReferenceDropDownAttribute ReferenceDropDownAttribute;
        private ReferenceDropDownExcludeAttribute ReferenceDropDownExcludeAttribute;
        private DragonMetaBlockAttribute DragonMetaBlockAttribute;
        private InlineInspectorAttribute InlineInspectorAttribute;

        private bool _hasSerializableData = true;
        private bool _isForceRepaint = false;
        private bool _isSerializeReference;

        // this is a damn hack to prevent the drawer from being called recursively when multiple attributes are attached to it
        #region UnrecursiveLabel
        private static GUIContent _unrecursiveLabel;
        private bool IsRecursive(GUIContent label)
        {
            return ReferenceEquals(label, _unrecursiveLabel);
        }
        #endregion

        #region InlineInshector Cache
        private bool _cachedInlineInspectorHeightInit = false;
        private float _cachedInlineInspectorHeight = 0f;
        private Editor _cachedInlineInspectorEditor;
        private Editor GetCachedInlineInspectorEditor(UnityObject selected, UnityObject target)
        {
            if (_cachedInlineInspectorEditor != null && _cachedInlineInspectorEditor.target != target)
            {
                UnityObject.DestroyImmediate(_cachedInlineInspectorEditor);
                _cachedInlineInspectorHeightInit = false;
                _cachedInlineInspectorHeight = -_cachedInlineInspectorHeight;
            }
            if (_cachedInlineInspectorEditor == null && target != null)
            {
                _cachedInlineInspectorEditor = Editor.CreateEditor(target);
            }
            return _cachedInlineInspectorEditor;
        }
        private float GetCachedInlineInspectorHeight(UnityObject selected, UnityObject target)
        {
            float result = Mathf.Abs(_cachedInlineInspectorHeight);
            if (_cachedInlineInspectorHeightInit == false || _cachedInlineInspectorHeight <= 0f)
            {
                var editor = GetCachedInlineInspectorEditor(selected, target);
                result = Mathf.Abs(_cachedInlineInspectorHeight);
                if (editor != null)
                {
                    EditorGUILayout.BeginVertical();
                    editor.OnInspectorGUI();
                    EditorGUILayout.EndVertical();

                    if (Event.current.type == EventType.Repaint)
                    {
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        if (lastRect.height > 0)
                        {
                            result = lastRect.height;
                            _cachedInlineInspectorHeight = result;
                            _isForceRepaint = true;
                        }
                    }
                }
                _cachedInlineInspectorHeightInit = true;
            }
            return result;
        }
        #endregion

        #region SerializeReference Cache
        private Type _cachedManagedType;
        private long _cachedManagedTypeID;
        private Type GetCachedManagedType(SerializedProperty sp)
        {
            var cid = sp.managedReferenceId;
            if (_cachedManagedType == null || _cachedManagedTypeID != cid)
            {
                //bool mrNull = sp.managedReferenceId == ManagedReferenceUtility.RefIdNull;
                //if(mrNull)
                //{
                //    _cachedManagedType = null;
                //}
                //else
                {
                    _cachedManagedTypeID = cid;
                    _cachedManagedType = sp.managedReferenceValue.GetType();
                }
            }
            return _cachedManagedType;
        }
        #endregion

        #region Properties
        private float Padding => Spacing;
        private bool IsDrawDropDown => ReferenceDropDownAttribute != null;
        private bool IsDrawMetaBlock => DragonMetaBlockAttribute != null;
        private bool UseAdvancedInlineInspector => UserSettingsPrefs.instance.UseAdvancedInlineInspector;
        #endregion

        #region Init
        protected override void OnStaticInit()
        {
            if (_unrecursiveLabel == null)
            {
                _unrecursiveLabel = new GUIContent();
            }
        }
        protected override void OnInit(SerializedProperty sp)
        {
            _isSerializeReference = sp.propertyType == SerializedPropertyType.ManagedReference;
            PredicateTypesKey key;
            _hasSerializableData = true;


            if (fieldInfo != null)
            {
                foreach (var atrRaw in Attributes)
                {
                    switch (atrRaw)
                    {
                        case ReferenceDropDownAttribute atr: ReferenceDropDownAttribute = atr; break;
                        case ReferenceDropDownExcludeAttribute atr: ReferenceDropDownExcludeAttribute = atr; break;
                        case DragonMetaBlockAttribute atr: DragonMetaBlockAttribute = atr; break;
                        case InlineInspectorAttribute atr: InlineInspectorAttribute = atr; break;
                    }
                }
            }
            if (_predicateOverride == null && PropertyType != null)
            {
                var targetType = PropertyType;
                if (ReferenceDropDownAttribute != null)
                {
                    Type[] withOutTypes = ReferenceDropDownExcludeAttribute != null ? ReferenceDropDownExcludeAttribute.ExcludedTypes : Type.EmptyTypes;

                    bool allAssignableTypes = targetType != typeof(ITemplateNode);

                    var types = ReferenceDropDownAttribute.AllowTypes;
                    if (types == null || types.Length == 0)
                    {
                        if (allAssignableTypes)
                        {
                            types = new Type[] { targetType };
                        }
                        else
                        {
                            types = new Type[] { typeof(IComponentTemplate), typeof(IEcsComponentMember) };
                        }
                    }
                    key = new PredicateTypesKey(targetType, types, withOutTypes);
                }
                else
                {
                    key = new PredicateTypesKey(targetType, new Type[] { targetType });
                }
                _predicateOverride = key;
            }

            if (IsDrawDropDown)
            {
                _dropDown = DragonFieldDropDown.Get(_predicateOverride.Value);
                _dropDown.OnSelected += SelectComponent;
            }
        }

        private bool CheckIsInilineInspector(SerializedProperty prop)
        {
            return InlineInspectorAttribute != null && prop.propertyType == SerializedPropertyType.ObjectReference;
        }

        [ThreadStatic]
        private static SerializedProperty currentProperty;
        private static void SelectComponent(DragonFieldDropDown.Item item)
        {
            //EcsGUI.Changed = true;
            if (item.Obj == null)
            {
                currentProperty.managedReferenceValue = null;
            }
            else
            {
                currentProperty.managedReferenceValue = item.Obj.CreateInstance();
                currentProperty.isExpanded = false;
            }
            currentProperty.serializedObject.ApplyModifiedProperties();
            DragonGUI.DelayedChanged = true;
        }
        #endregion

        protected override float GetCustomHeight(SerializedProperty property, GUIContent label)
        {
            _isForceRepaint = false;
            if (IsRecursive(label)) { return EditorGUI.GetPropertyHeight(property, label); }

            _unrecursiveLabel.text = label.text;
            _unrecursiveLabel.tooltip = label.tooltip;
            label = _unrecursiveLabel;

            if (_isSerializeReference)
            {
                _hasSerializableData = property.HasSerializableData();
            }

            SerializedProperty componentProp = property;

            // SerializeReference field
            if (_isSerializeReference)
            {
                if (property.IsNullManagedReference() == false)
                {
                    try
                    {
                        var type = GetCachedManagedType(property);
                        if (DragonFieldCahce.RuntimeDict.TryGetValue(type, out var info) && info.HasWrappedFieldName)
                        {
                            componentProp = property.FindPropertyRelative(info.WrappedFieldName);
                        }
                    }
                    catch
                    {
                        componentProp = property;
                    }
                    if (componentProp == null)
                    {
                        return DamagedComponentHeight;
                    }
                }
            }
            else
            {
                var fieldType = PropertyType;
                if (DragonFieldCahce.RuntimeDict.TryGetValue(fieldType, out var info) && info.HasWrappedFieldName)
                {
                    componentProp = property.FindPropertyRelative(info.WrappedFieldName);
                }
            }
            if (componentProp == null)
            {
                componentProp = property;
            }

            // Serializable field
            {
                float result = EditorGUIUtility.singleLineHeight;
                if (_hasSerializableData)
                {
                    result = EditorGUI.GetPropertyHeight(componentProp, label);
                }
                if (CheckIsInilineInspector(componentProp) && componentProp.isExpanded)
                {
                    if (UseAdvancedInlineInspector)
                    {
                        if (_cachedInlineInspectorHeightInit)
                        {
                            result += Mathf.Abs(_cachedInlineInspectorHeight);
                        }
                    }
                    else
                    {
                        result += GetInlinePropertyHeight(componentProp.objectReferenceValue);
                    }
                }
                if (IsDrawMetaBlock)
                {
                    result += Padding * 4f;
                }
                return result;
            }
        }
        private float GetInlinePropertyHeight(UnityObject target)
        {
            var obj = new SerializedObject(target);
            var height = 2f;
            var spacing = Spacing;
            
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            iterator.NextVisible(true);
            height += spacing;
            height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
            while (iterator.NextVisible(false))
            {
                height += spacing;
                height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
            }
            if (height > 0)
            {
                height += spacing;
            }

            return height;
        }


        protected override void DrawCustom(Rect rect, SerializedProperty property, GUIContent label)
        {
            _isForceRepaint = false;
            if (IsRecursive(label)) { EditorGUI.PropertyField(rect, property, label, true); return; }

            _unrecursiveLabel.text = label.text;
            _unrecursiveLabel.tooltip = label.tooltip;
            label = _unrecursiveLabel;

            if (_isSerializeReference)
            {
                _hasSerializableData = property.HasSerializableData();
            }

            var e = Event.current;
            var et = e.type;
            var rootProperty = property;

            ITypeMeta meta = null;
            SerializedProperty componentProp = property;
            bool isDrawDropDown = IsDrawDropDown && _isSerializeReference;
            bool isNull = false;

            if (_isSerializeReference)
            {
                DragonFieldCahce info = null;
                isNull = property.IsNullManagedReference();
                if (!isNull)
                {
                    var type = GetCachedManagedType(property);
                    if (DragonFieldCahce.TryGetInfoFor(type, out info) && info.HasWrappedFieldName)
                    {
                        componentProp = property.FindPropertyRelative(info.WrappedFieldName);
                    }
                }
                if (componentProp == null)
                {
                    DrawDamagedComponent(rect, "Damaged component template.");
                    return;
                }

                if (info != null)
                {
                    meta = info.Type.GetMeta();
                }
                else if (isNull == false)
                {
                    var type = GetCachedManagedType(property);
                    meta = type.GetMeta();
                }

                if (isDrawDropDown && isNull == false && ReferenceDropDownAttribute.HideButtonIfNotNull)
                {
                    isDrawDropDown = false;
                }
            }
            else
            {
                var fieldType = PropertyType;
                if (DragonFieldCahce.RuntimeDict.TryGetValue(fieldType, out var info) && info.HasWrappedFieldName)
                {
                    componentProp = property.FindPropertyRelative(info.WrappedFieldName);
                }
            }
            if (componentProp == null)
            {
                componentProp = property;
            }
            bool isInlineInspectr = CheckIsInilineInspector(componentProp);

            if (isInlineInspectr)
            {
                var targetObject = componentProp.objectReferenceValue;
                if (targetObject)
                {
                    meta = targetObject.GetMeta();
                }
                else
                {
                    isNull = true;
                }
            }

            if (isNull)
            {
                meta = TypeMeta.NullTypeMeta;
            }
            if (meta == null)
            {
                meta = PropertyType.GetMeta();
            }


            float selectionButtonRightOffset = 0f;
            if (IsDrawMetaBlock)
            {
                ref var r = ref rect;
                var (skip, optionsWidth) = DragonGUI.DrawTypeMetaBlock(ref r, rootProperty, meta);
                selectionButtonRightOffset = optionsWidth;
                if (skip || e.type == EventType.Used)
                {
                    return;
                }
            }
            if (IsArrayElement)
            {
                label.text = meta.Name;
            }

            if (isDrawDropDown)
            {
                Rect srcRect = rect;
                srcRect.xMax -= selectionButtonRightOffset;
                DrawSelectionDropDown(srcRect, property, label, IsDrawMetaBlock);
            }

            var fieldRect = rect;

            if (componentProp.propertyType != SerializedPropertyType.Generic &&
                componentProp.propertyType != SerializedPropertyType.ManagedReference)
            {
                fieldRect.xMax -= selectionButtonRightOffset;
            }

            if (_hasSerializableData)
            {
                if (isInlineInspectr)
                {
                    EditorGUI.BeginProperty(fieldRect, label, componentProp);
                    var position = fieldRect;
                    var targetObject = componentProp.objectReferenceValue;


                    Rect foldoutRect = new Rect(position.x, position.y, DragonGUI.LabelWidth, OneLineHeight);
                    Rect objectFieldRect = new Rect(position.x + DragonGUI.LabelWidth, position.y,
                                                    position.width - DragonGUI.LabelWidth, OneLineHeight);

                    bool foldout = componentProp.isExpanded;
                    bool isDrawInline = foldout;
                    if (targetObject != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        foldout = EditorGUI.Foldout(foldoutRect, foldout, label, true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            componentProp.isExpanded = foldout;
                            componentProp.serializedObject.ApplyModifiedProperties();
                        }
                        EditorGUI.ObjectField(objectFieldRect, componentProp, GUIContent.none);
                        if (foldout == false)
                        {
                            isDrawInline = false;
                        }
                    }
                    else
                    {
                        //Rect propRect = fieldRect;
                        //propRect.xMax -= 30f;
                        //Rect buttonRect = position;
                        //buttonRect.xMin = propRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                        //if (GUI.Button(buttonRect, "+"))
                        //{
                        //	CreateScriptableObjectWindow.Show(property, fieldInfo.FieldType);
                        //}

                        EditorGUI.PropertyField(fieldRect, componentProp, label);
                        isDrawInline = false;
                    }

                    if (isDrawInline)
                    {
                        if (UseAdvancedInlineInspector)
                        {
                            var inspectorHeight = Mathf.Abs(_cachedInlineInspectorHeight);
                            Rect inspectorRect = new Rect(position.x, position.y + OneLineHeight + Spacing,
                                                          position.width, inspectorHeight);
                            inspectorRect.xMax += selectionButtonRightOffset;

                            using (DragonGUI.UpIndentLevel()) using (DragonGUI.SetEnable(!InlineInspectorAttribute.IsReadOnly))
                            {
                                EditorGUI.BeginChangeCheck();
                                GUILayout.BeginArea(inspectorRect);
                                GetCachedInlineInspectorEditor(componentProp.serializedObject.context, componentProp.objectReferenceValue).OnInspectorGUI();
                                GUILayout.EndArea();
                                if (EditorGUI.EndChangeCheck() && et == EventType.MouseUp)
                                {
                                    _cachedInlineInspectorHeightInit = false;
                                    _cachedInlineInspectorHeight = -_cachedInlineInspectorHeight;
                                }
                            }
                        }
                        else
                        {
                            Rect localRect = position;
                            localRect.xMax += selectionButtonRightOffset;
                            //DrawInlineBackground(position);
                            var obj = new SerializedObject(targetObject);
                            obj.UpdateIfRequiredOrScript();

                            var spacing = Spacing;
                            localRect.xMin += 14;
                            localRect.xMax -= 5;
                            localRect.yMin += 1;
                            localRect.yMax -= 1;

                            localRect.y += OneLineHeight;

                            using (DragonGUI.SetEnable(!InlineInspectorAttribute.IsReadOnly)) using (DragonGUI.CheckChanged())
                            {
                                SerializedProperty iterator = obj.GetIterator();
                                using (DragonGUI.Disable)
                                {
                                    iterator.NextVisible(true);
                                    localRect.y += spacing;
                                    localRect.height = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                                    EditorGUI.PropertyField(localRect, iterator, includeChildren: true);
                                    localRect.y += localRect.height;
                                }
                                while (iterator.NextVisible(false))
                                {
                                    localRect.y += spacing;
                                    localRect.height = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                                    EditorGUI.PropertyField(localRect, iterator, includeChildren: true);
                                    localRect.y += localRect.height;
                                }
                                if (DragonGUI.Changed)
                                {
                                    obj.ApplyModifiedProperties();
                                }
                            }
                        }
                    }
                    EditorGUI.EndProperty();

                    if (isDrawInline)
                    {
                        if (UseAdvancedInlineInspector)
                        {
                            GetCachedInlineInspectorHeight(componentProp.serializedObject.context, componentProp.objectReferenceValue);
                        }
                    }
                }
                else
                {
                    EditorGUI.PropertyField(fieldRect, componentProp, label, true);
                }
            }
            else
            {
                EditorGUI.LabelField(rect, label);
            }


            if (_isForceRepaint)
            {
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        #region Other
        private void DrawSelectionDropDown(Rect rect, SerializedProperty property, GUIContent label, bool isDrawMetaBlock)
        {
            if (rect.width < 0) { return; }

            Rect position;
            if (string.IsNullOrWhiteSpace(label.text))
            {
                position = rect;
            }
            else
            {
                position = rect.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            }

            bool isHideButtonIfNotNull = ReferenceDropDownAttribute.HideButtonIfNotNull;

            Type type = null;
            if (property.IsNullManagedReference() == false &&
                property.hasMultipleDifferentValues == false)
            {
                type = GetCachedManagedType(property);
            }

            string text = type == null ? "Select..." : type.GetMeta().Name;
            if (!isHideButtonIfNotNull || type == null)
            {
                if (GUI.Button(position, text, EditorStyles.layerMaskField))
                {
                    currentProperty = property;
                    _dropDown.OpenForField(position, property);
                }
            }
            else
            {
                GUI.Label(position, text);
            }
        }
        private void DrawDamagedComponent(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Warning);
        }
        #endregion
    }
}
#endif