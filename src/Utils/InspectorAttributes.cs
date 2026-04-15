#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity
{
    public sealed class ReferenceDropDownAttribute : PropertyAttribute
    {
        public readonly Type[] AllowTypes;
        public readonly bool IsHideButtonIfNotNull;
        public ReferenceDropDownAttribute(bool isHideButtonIfNotNull = false) : this(isHideButtonIfNotNull, Array.Empty<Type>()) { }
        public ReferenceDropDownAttribute(params Type[] predicateTypes) : this(false, predicateTypes) { }
        public ReferenceDropDownAttribute(bool isHideButtonIfNotNull, params Type[] predicateTypes)
        {
            IsHideButtonIfNotNull = isHideButtonIfNotNull;
            AllowTypes = predicateTypes;
            Array.Sort(predicateTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
    public sealed class ReferenceDropDownWithoutAttribute : Attribute
    {
        public readonly Type[] PredicateTypes;
        [Obsolete("With empty parameters, this attribute makes no sense.", true)]
        public ReferenceDropDownWithoutAttribute() : this(Array.Empty<Type>()) { }
        public ReferenceDropDownWithoutAttribute(params Type[] predicateTypes)
        {
            PredicateTypes = predicateTypes;
            Array.Sort(predicateTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
    public sealed class DragonMetaBlockAttribute : PropertyAttribute { }
}


#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using UnityEditor;
    using UnityEngine.Serialization;

    [CustomPropertyDrawer(typeof(ReferenceDropDownAttribute), true)]
    [CustomPropertyDrawer(typeof(DragonMetaBlockAttribute), true)]
    internal class DragonFieldDrawer : ExtendedPropertyDrawer
    {
        private const float DamagedComponentHeight = 18f * 2f;
        private DragonFieldDropDown _dropDown;
        private PredicateTypesKey? _predicateOverride;

        private ReferenceDropDownAttribute ReferenceDropDownAttribute;
        private ReferenceDropDownWithoutAttribute ReferenceDropDownWithoutAttribute;
        private DragonMetaBlockAttribute TypeMetaBlockAttribute;

        private bool _isInit = false;
        private bool _hasSerializableData = true;

        // this is a damn hack to prevent the drawer from being called recursively when multiple attributes are attached to it
        private static GUIContent _unrecursiveLabel;
        private bool _isSerializeReference;



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
        private bool IsDrawMetaBlock => TypeMetaBlockAttribute != null;
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
                        case ReferenceDropDownWithoutAttribute atr: ReferenceDropDownWithoutAttribute = atr; break;
                        case DragonMetaBlockAttribute atr: TypeMetaBlockAttribute = atr; break;
                    }
                }
            }
            if (_predicateOverride == null && PropertyType != null)
            {
                var targetType = PropertyType;
                if (ReferenceDropDownAttribute != null)
                {
                    Type[] withOutTypes = ReferenceDropDownWithoutAttribute != null ? ReferenceDropDownWithoutAttribute.PredicateTypes : Type.EmptyTypes;

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
            if (ReferenceEquals(label, _unrecursiveLabel)) { return EditorGUI.GetPropertyHeight(property, label); }
            _unrecursiveLabel.text = label.text;
            _unrecursiveLabel.tooltip = label.tooltip;
            label = _unrecursiveLabel;

            if (_isSerializeReference)
            {
                _hasSerializableData = property.HasSerializableData();
            }

            SerializedProperty componentProp = property;
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
                    //var instance = property.managedReferenceValue;
                    //if (DragonFieldCahce.RuntimeDict.TryGetValue(instance.GetType(), out var info) && info.HasWrappedFieldName)
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

            {
                //EcsDebug.PrintPass(_hasSerializableData);
                float result = EditorGUIUtility.singleLineHeight;
                if (_hasSerializableData)
                {
                    result = EditorGUI.GetPropertyHeight(componentProp, label);
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
            if (ReferenceEquals(label, _unrecursiveLabel)) { EditorGUI.PropertyField(rect, property, label, true); return; }
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
                    //var instance = property.managedReferenceValue;
                    //if (DragonFieldCahce.TryGetInfoFor(instance.GetType(), out info) && info.HasWrappedFieldName)
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
                        meta = info.ComponentType.GetMeta();
                    }
                    else
                    {
                        if (mrNull == false)
                        {
                            var type = GetCachedManagedType(property);
                            meta = type.GetMeta();
                        }
                    }
                }

                if (isDrawDropDown && mrNull == false && ReferenceDropDownAttribute.IsHideButtonIfNotNull)
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
                meta = fieldType.GetMeta();
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
                    //string oldText = label.text;
                    //label.text = string.Empty;
                    EditorGUI.PropertyField(fieldRect, componentProp, label, true);
                    //label.text = oldText;
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

            bool isHideButtonIfNotNull = ReferenceDropDownAttribute.IsHideButtonIfNotNull;

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

            bool isHideButtonIfNotNull = ReferenceDropDownAttribute.IsHideButtonIfNotNull;

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