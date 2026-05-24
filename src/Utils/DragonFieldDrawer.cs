#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;


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

		private bool _isInit = false;
        private bool _hasSerializableData = true;
        private bool _isForceRepaint = false;

        // this is a damn hack to prevent the drawer from being called recursively when multiple attributes are attached to it
        private static GUIContent _unrecursiveLabel;
        private bool _isSerializeReference;

        private bool _cachedInlineInspectorHeightInit = false;
		private float _cachedInlineInspectorHeight = 0f;
        private Editor _cachedInlineInspectorEditor;
        private Editor GetCachedInlineInspectorEditor(UnityEngine.Object target)
        {
			if (_cachedInlineInspectorEditor != null && _cachedInlineInspectorEditor.target != target)
            {
				UnityEngine.Object.DestroyImmediate(_cachedInlineInspectorEditor);
				_cachedInlineInspectorHeightInit = false;
			}
            if (_cachedInlineInspectorEditor == null && target != null)
            {
                _cachedInlineInspectorEditor = Editor.CreateEditor(target); ;
            }
			return _cachedInlineInspectorEditor;
        }
        private float GetCachedInlineInspectorHeight(UnityEngine.Object target)
        {
			float result = Mathf.Abs(_cachedInlineInspectorHeight);
			if (_cachedInlineInspectorHeightInit == false || _cachedInlineInspectorHeight <= 0f)
            {
                var editor = GetCachedInlineInspectorEditor(target);
                if (editor != null)
                {
					EditorGUILayout.BeginVertical();
					editor.OnInspectorGUI();
					EditorGUILayout.EndVertical();

					result = Mathf.Abs(_cachedInlineInspectorHeight);

					if (Event.current.type == EventType.Repaint)
					{
						Rect lastRect = GUILayoutUtility.GetLastRect();
						if (lastRect.height > 0)
						{
							_cachedInlineInspectorHeight = lastRect.height;
							result = _cachedInlineInspectorHeight;
							_isForceRepaint = true;
						}
					}
				}
                _cachedInlineInspectorHeightInit = true;

			}
            return result;
		}

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
                    _cachedManagedType = sp.managedReferenceValue.GetType();
                }
            }
            return _cachedManagedType;
        }

        #region Properties
        private float Padding => Spacing;
        protected override bool IsInit => _isInit;
        private bool IsDrawDropDown => ReferenceDropDownAttribute != null;
        private bool IsDrawMetaBlock => DragonMetaBlockAttribute != null;
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

            _isInit = true;
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

        private bool IsRecursive(GUIContent label)
        {
            return ReferenceEquals(label, _unrecursiveLabel);

		}
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
                if (property.IsNullManagedReference())
                {
                    float result = EditorGUIUtility.singleLineHeight;
                    if (IsDrawMetaBlock)
                    {
                        result += Padding * 2f;
                    }
                    return result;
                }

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
                if(CheckIsInilineInspector(componentProp) && _cachedInlineInspectorHeightInit && componentProp.isExpanded)
                {
                    result += Mathf.Abs(_cachedInlineInspectorHeight);
                }
                if (IsDrawMetaBlock)
                {
                    result += Padding * 4f;
                }
                return result;
            }
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

            //var e = Event.current;
            var rootProperty = property;

            ITypeMeta meta = null;
            SerializedProperty componentProp = property;
            bool isDrawProperty = true;
            bool isDrawDropDown = IsDrawDropDown && _isSerializeReference;

            Rect srcRect = rect;
			if (_isSerializeReference)
            {
                DragonFieldCahce info = null;
                bool mrNull = property.IsNullManagedReference();
                if (mrNull)
                {
                    isDrawProperty = false;
                }
                else
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

                if (meta == null)
                {
                    if (info != null)
                    {
                        meta = info.Type.GetMeta();
                    }
                    else if (mrNull == false)
                    {
                        var type = GetCachedManagedType(property);
                        meta = type.GetMeta();
                    }
                }

                if (isDrawDropDown && mrNull == false && ReferenceDropDownAttribute.HideButtonIfNotNull)
                {
                    isDrawDropDown = false;
                }
            }
            else
            {
                if (CheckIsInilineInspector(componentProp))
                {
					var unityObj = componentProp.objectReferenceValue;
					if (unityObj)
					{
						meta = unityObj.GetMeta();
					}
                    else
                    {
						meta = PropertyType.GetMeta();
					}
				}
                else
                {
					var fieldType = PropertyType;
					if (DragonFieldCahce.RuntimeDict.TryGetValue(fieldType, out var info) && info.HasWrappedFieldName)
					{
						componentProp = property.FindPropertyRelative(info.WrappedFieldName);
					}
					meta = fieldType.GetMeta();
				}
            }
            if (componentProp == null)
            {
                componentProp = property;
            }

            float selectionButtonRightOffset = 0f;

			if (isDrawProperty)
            {
                if (IsDrawMetaBlock)
                {
                    
                    ref var r = ref rect;
                    var (skip, optionsWidth) = DragonGUI.DrawTypeMetaBlock(ref r, rootProperty, meta);
                    selectionButtonRightOffset = optionsWidth;
                    if (skip)
                    {
                        return;
                    }
                    if(Event.current.type == EventType.Used)
                    {
                        return;
                    }
                }
            }

            if (isDrawProperty)
            {
                if (IsArrayElement)
                {
                    label.text = meta.Name;
                }
            }

			if (isDrawDropDown)
            {
                srcRect.xMax -= selectionButtonRightOffset;
                DrawSelectionDropDown(srcRect, property, label);
            }

			if (isDrawProperty)
            {
                var fieldRect = rect;

				if (ReferenceEquals(property, componentProp) &&
                    componentProp.propertyType != SerializedPropertyType.Generic &&
                    componentProp.propertyType != SerializedPropertyType.ManagedReference)
                {
                    fieldRect.xMax -= selectionButtonRightOffset;
                    isDrawDropDown = false;
                }

                if (_hasSerializableData)
                {
                    if(CheckIsInilineInspector(componentProp))
                    {
                        EditorGUI.BeginProperty(fieldRect, label, componentProp);
						var position = fieldRect;
						var targetObject = componentProp.objectReferenceValue;

						Rect foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
						Rect objectFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
														position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

						bool foldout = componentProp.isExpanded;
                        bool isDraw = foldout;
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
                                isDraw = false;
							}
						}
						else
						{
							Rect propRect = position;
							//propRect.xMax -= 30f;
							//Rect buttonRect = position;
							//buttonRect.xMin = propRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                            //
							//if (GUI.Button(buttonRect, "+"))
							//{
							//	CreateScriptableObjectWindow.Show(property, fieldInfo.FieldType);
							//}

							EditorGUI.PropertyField(propRect, componentProp, label);
                            isDraw = false;
						}

                        if (isDraw)
                        {
							var inspectorHeight = Mathf.Abs(_cachedInlineInspectorHeight);

							int il = EditorGUI.indentLevel;
							EditorGUI.indentLevel++;

							float indent = (EditorGUI.indentLevel + 1) * 15f;
							Rect inspectorRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
														  position.width, inspectorHeight);
							inspectorRect.xMax += selectionButtonRightOffset;
							var et = Event.current.type;
							var defaultEnabled = GUI.enabled;
							if (InlineInspectorAttribute.IsReadOnly)
							{
								GUI.enabled = false;
							}

							EditorGUI.BeginChangeCheck();
							GUILayout.BeginArea(inspectorRect);
							GetCachedInlineInspectorEditor(componentProp.objectReferenceValue).OnInspectorGUI();
							GUILayout.EndArea();
							if (EditorGUI.EndChangeCheck() && et == EventType.MouseUp)
							{
								_cachedInlineInspectorHeightInit = false;
							}

							GUI.enabled = defaultEnabled;
							EditorGUI.indentLevel = il;
						}
						EditorGUI.EndProperty();

						if (isDraw)
                        {
							GetCachedInlineInspectorHeight(componentProp.objectReferenceValue);
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
            }
            else
            {
                EditorGUI.LabelField(rect, label);
            }

            if (isDrawDropDown)
            {
                DrawFakeSelectionDropDown(srcRect, property, label);
            }

            if (_isForceRepaint)
			{
				EditorWindow.focusedWindow?.Repaint();
			}
        }



        private void DrawFakeSelectionDropDown(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (rect.width < 0) { return; }

            Rect position;
            if (string.IsNullOrEmpty(label.text))
            {
                position = rect;
            }
            else
            {
                position = rect.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            }

            position.height = OneLineHeight;
            position.y += Spacing * 2;

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
                }
            }
        }
        private void DrawSelectionDropDown(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (rect.width < 0) { return; }

            Rect position;
            if (string.IsNullOrEmpty(label.text))
            {
                position = rect;
            }
            else
            {
                position = rect.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            }

            position.height = OneLineHeight;
            position.y += Spacing * 2;

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
    }
}
#endif