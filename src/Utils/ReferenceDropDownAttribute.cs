#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        #region CheckSkip
        [ThreadStatic]
        private static int _skips = 0;
        private bool CheckSkip()
        {
            if (_skips > 0)
            {
                _skips--;
                return true;
            }
            int count = 0;
            if (ReferenceDropDownAttribute != null) { count++; }
            if (TypeMetaBlockAttribute != null) { count++; }

            _skips = count - 1;
            return false;
        }
        #endregion

        #region Properties
        private float Padding => Spacing;
        protected override bool IsInit => _isInit;
        private bool IsDrawDropDown => ReferenceDropDownAttribute != null;
        private bool IsDrawMetaBlock => TypeMetaBlockAttribute != null;
        #endregion

        #region Init
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
            if (CheckSkip()) { return EditorGUI.GetPropertyHeight(property, label); }
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
            if (CheckSkip()) { EditorGUI.PropertyField(rect, property, label, true); return; }
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
                meta = fieldInfo.FieldType.GetMeta();
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










    internal class DragonFieldDropDown : MetaObjectsDropDown<DragonFieldCahce>
    {
        private DragonFieldDropDown() { }

        private bool _isCheckUnique;
        private SerializedProperty _arrayProperty;
        private SerializedProperty _fieldProperty;

        public static Dictionary<PredicateTypesKey, DragonFieldDropDown> _dropDownsCache = new Dictionary<PredicateTypesKey, DragonFieldDropDown>(32);
        public static DragonFieldDropDown Get(PredicateTypesKey key)
        {
            if (_dropDownsCache.TryGetValue(key, out var result) == false)
            {
                result = new DragonFieldDropDown();
                IEnumerable<(DragonFieldCahce template, ITypeMeta meta)> itemMetaPairs = DragonFieldCahce.All.ToArray()
                    .Where(o =>
                    {
                        return key.Check(o.Type);

                    })
                    .Select(o =>
                    {
                        return (o, (ITypeMeta)o.Meta);
                    });

                //TODO оптимизировать или вырезать
                itemMetaPairs = itemMetaPairs.OrderBy(o => o.meta.Group.Name);
                result.Setup(itemMetaPairs);
                _dropDownsCache[key] = result;
            }
            return result;
        }

        public void OpenForArray(Rect position, SerializedProperty arrayProperty, bool isCheckUnique)
        {
            _isCheckUnique = isCheckUnique;
            _arrayProperty = arrayProperty;
            _fieldProperty = null;
            Show(position);
        }
        public void OpenForField(Rect position, SerializedProperty fieldProperty)
        {
            _isCheckUnique = false;
            _arrayProperty = null;
            _fieldProperty = fieldProperty;
            Show(position);
        }

        protected override void ItemSelected(Item item)
        {
            if (item.Obj == null)
            {
                _fieldProperty.managedReferenceValue = null;
                _fieldProperty.serializedObject.ApplyModifiedProperties();
                return;
            }

            Type componentType = item.Obj.GetType();
            var data = item.Obj;

            if (_arrayProperty != null && data != null)
            {
                int index = _arrayProperty.arraySize;
                if (_isCheckUnique)
                {
                    if (data.IsUnique)
                    {
                        for (int i = 0, iMax = _arrayProperty.arraySize; i < iMax; i++)
                        {
                            if (_arrayProperty.GetArrayElementAtIndex(i).managedReferenceValue.GetType() == componentType)
                            {
                                return;
                            }
                        }
                    }
                }
                _arrayProperty.arraySize += 1;
                _fieldProperty = _arrayProperty.GetArrayElementAtIndex(index);
            }

            if (_fieldProperty != null)
            {
                _fieldProperty.managedReferenceValue = data.CreateInstance();
                _fieldProperty.serializedObject.ApplyModifiedProperties();
            }

            //Event.current.Use();
        }
    }












    internal class DragonFieldCahce
    {
        private static DragonFieldCahce[] _all;
        internal static IReadOnlyList<DragonFieldCahce> All
        {
            get { return _all; }
        }
        static DragonFieldCahce() { StaticInit(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void StaticInit()
        {
            List<DragonFieldCahce> list = new List<DragonFieldCahce>(UnityEditorUtility._serializableTypes.Length);
            foreach (var type in UnityEditorUtility._serializableTypes)
            {
                DragonFieldCahce element = new DragonFieldCahce(type);
                list.Add(element);
            }
            _all = list.ToArray();
        }


        public readonly Type Type;
        public readonly Type ComponentType;
        public readonly bool IsUnique;
        private TypeMeta _meta;
        public TypeMeta Meta
        {
            get
            {
                if (_meta == null)
                {
                    {
                        _meta = Type.GetMeta();
                    }
                }
                return _meta;
            }
        }
        private bool _defaultValueTypeInit = false;
        private object _defaultValueDummy;
        public object DefaultValue
        {
            get
            {
                if (_defaultValueTypeInit == false)
                {
                    if (Type.IsValueType)
                    {
                        FieldInfo field;
                        field = Type.GetField("Default", BindingFlags.Static | BindingFlags.Public);
                        if (field != null && field.FieldType == Type)
                        {
                            _defaultValueDummy = field.GetValue(null).Clone_Reflection();
                        }

                        if (_defaultValueDummy == null)
                        {
                            field = Type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                            if (field != null && field.FieldType == Type)
                            {
                                _defaultValueDummy = field.GetValue(null).Clone_Reflection();
                            }
                        }
                    }
                    _defaultValueTypeInit = true;
                }
                return _defaultValueDummy;
            }
        }
        public DragonFieldCahce(Type type)
        {
            Type = type;
            IsUnique = false;

            if (type.GetInterfaces().Contains(typeof(IComponentTemplate)))
            {
                var ct = (IComponentTemplate)Activator.CreateInstance(type);
                IsUnique = ct.IsUnique;
                ComponentType = ct.ComponentType;
            }
            else
            {
                ComponentType = Type;
            }
        }
        public object CreateInstance()
        {
            if (DefaultValue != null)
            {
                return DefaultValue.Clone_Reflection();
            }
            return Activator.CreateInstance(Type);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
#endif