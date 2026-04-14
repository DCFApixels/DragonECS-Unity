#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS
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
    public sealed class TypeMetaBlockAttribute : PropertyAttribute { }
}


#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReferenceDropDownAttribute), true)]
    [CustomPropertyDrawer(typeof(TypeMetaBlockAttribute), true)]
    internal class EcsDragonFieldDrawer : ExtendedPropertyDrawer
    {
        private const float DamagedComponentHeight = 18f * 2f;
        private DragonFieldDropDown _dropDown;
        private PredicateTypesKey? _predicateOverride;

        private ReferenceDropDownAttribute ReferenceDropDownAttribute;
        private ReferenceDropDownWithoutAttribute ReferenceDropDownWithoutAttribute;
        private TypeMetaBlockAttribute TypeMetaBlockAttribute;

        private bool _isInit = false;
        private bool _hasSerializableData;

        // this is a damn hack to prevent the drawer from being called recursively when multiple attributes are attached to it
        private static GUIContent _unrecursiveLable;

        #region Properties
        private float Padding => Spacing;
        protected override bool IsInit => _isInit;
        private bool IsDrawDropDown => ReferenceDropDownAttribute != null;
        private bool IsDrawMetaBlock => TypeMetaBlockAttribute != null;
        #endregion

        #region Init
        protected override void OnStaticInit()
        {
            if(_unrecursiveLable == null)
            {
                _unrecursiveLable = new GUIContent();
            }
        }
        protected override void OnInit(SerializedProperty property)
        {
            PredicateTypesKey key;
            _hasSerializableData = true;

            if (fieldInfo != null)
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    _hasSerializableData = property.HasSerializableData();
                }
                foreach (var atrRaw in Attributes)
                {
                    switch (atrRaw)
                    {
                        case ReferenceDropDownAttribute atr: ReferenceDropDownAttribute = atr; break;
                        case ReferenceDropDownWithoutAttribute atr: ReferenceDropDownWithoutAttribute = atr; break;
                        case TypeMetaBlockAttribute atr: TypeMetaBlockAttribute = atr; break;
                    }
                }
            }
            if (_predicateOverride == null && fieldInfo != null)
            {

                var targetType = fieldInfo.FieldType;
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
            EcsGUI.DelayedChanged = true;
        }
        #endregion

        protected override float GetCustomHeight(SerializedProperty property, GUIContent label)
        {
            if (ReferenceEquals(_unrecursiveLable, label)) { return EditorGUI.GetPropertyHeight(property, label); }
            _unrecursiveLable.text = label.text;
            _unrecursiveLable.tooltip = label.tooltip;
            label = _unrecursiveLable;
            //if (CheckSkip()) { return EditorGUI.GetPropertyHeight(property, label); }
            bool isSerializeReference = property.propertyType == SerializedPropertyType.ManagedReference;

            SerializedProperty componentProp = property;
            if (isSerializeReference)
            {
                var instance = property.managedReferenceValue;
                if (instance == null)
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
                    if (instance is ComponentTemplateBase customTemplate)
                    {
                        componentProp = property.FindPropertyRelative("component");
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
                var fieldType = fieldInfo.FieldType;
                if (typeof(ComponentTemplateBase).IsAssignableFrom(fieldType))
                {
                    componentProp = property.FindPropertyRelative("component");
                    if (componentProp == null)
                    {
                        componentProp = property;
                    }
                }
            }

            {
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
            if (ReferenceEquals(_unrecursiveLable, label)) { EditorGUI.PropertyField(rect, property, label, true); return; }
            _unrecursiveLable.text = label.text;
            _unrecursiveLable.tooltip = label.tooltip;
            label = _unrecursiveLable;
            //if (CheckSkip()) { EditorGUI.PropertyField(rect, property, label, true); return; }
            bool isSerializeReference = property.propertyType == SerializedPropertyType.ManagedReference;

            var e = Event.current;
            var rootProperty = property;

            ITypeMeta meta = null;
            SerializedProperty componentProp = property;
            bool isDrawProperty = true;
            bool isDrawDropDown = IsDrawDropDown && isSerializeReference;

            if (isSerializeReference)
            {
                var template = property.managedReferenceValue;

                if (template is ComponentTemplateBase)
                {
                    componentProp = property.FindPropertyRelative("component");
                }
                if (componentProp == null)
                {
                    DrawDamagedComponent(rect, "Damaged component template.");
                    return;
                }
                if (template == null)
                {
                    isDrawProperty = false;
                }

                //meta = template as ITypeMeta;
                if (meta == null)
                {
                    if (template is IComponentTemplate componentTemplate)
                    {
                        meta = componentTemplate.ComponentType.GetMeta();
                    }
                    else
                    {
                        meta = template.GetMeta();
                    }
                }

                if (isDrawDropDown && template != null && ReferenceDropDownAttribute.IsHideButtonIfNotNull)
                {
                    isDrawDropDown = false;
                }
            }
            else
            {
                var fieldType = fieldInfo.FieldType;
                if (typeof(ComponentTemplateBase).IsAssignableFrom(fieldType))
                {
                    componentProp = property.FindPropertyRelative("component");
                    if (componentProp == null)
                    {
                        componentProp = property;
                    }
                }
                meta = fieldType.GetMeta();
            }



            float selectionButtonRightOffset = 0f;

            if (isDrawProperty)
            {
                if (IsDrawMetaBlock)
                {
                    ref var r = ref rect;
                    var (skip, optionsWidth) = EcsGUI.DrawTypeMetaBlock(ref r, rootProperty, meta);
                    selectionButtonRightOffset = optionsWidth;
                    if (skip)
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

                var fieldRect = rect;

                if (property != componentProp &&
                    componentProp.propertyType != SerializedPropertyType.Generic &&
                    componentProp.propertyType != SerializedPropertyType.ManagedReference)
                {
                    fieldRect.xMax -= selectionButtonRightOffset;
                    isDrawDropDown = false;
                }

                var et = e.type;

                if (_hasSerializableData)
                {
                    EditorGUI.PropertyField(fieldRect, componentProp, label, true);
                }
                else
                {
                    EditorGUI.LabelField(rect, label);
                }

                var labelRect = rect;
                labelRect.width = EditorGUIUtility.labelWidth;
                labelRect.xMin -= 20f;
                if (e.type == EventType.Used && EcsGUI.HitTest(labelRect, e) == false)
                {
                    e.type = et;
                }

            }
            else
            {
                EditorGUI.LabelField(rect, label);
            }




            if (isDrawDropDown)
            {
                rect.xMax -= selectionButtonRightOffset;
                DrawSelectionDropDown(rect, property, label);
            }

        }





        private void DrawSelectionDropDown(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (rect.width < 0) { return; }

            var position = IsArrayElement ? rect : rect.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            position.height = OneLineHeight;

            bool isHideButtonIfNotNull = ReferenceDropDownAttribute.IsHideButtonIfNotNull;
            object obj = property.hasMultipleDifferentValues ? null : property.managedReferenceValue;

            string text = obj == null ? "Select..." : obj.GetMeta().Name;
            if (!isHideButtonIfNotNull || obj == null)
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