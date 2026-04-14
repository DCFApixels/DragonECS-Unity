#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(ComponentTemplateProperty), true)]
    internal class ComponentTemplatePropertyDrawer : ExtendedPropertyDrawer
    {
        //private EcsDragonFieldDrawer _drawer = new EcsDragonFieldDrawer(new PredicateTypesKey(typeof(ITemplateNode)));
        //protected override float GetCustomHeight(SerializedProperty property, GUIContent label)
        //{
        //    property.Next(true);
        //    _drawer.StaticInit();
        //    _drawer.Init(property);
        //    return _drawer.GetPropertyHeight(property, label);
        //}
        //protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    var root = property.Copy();
        //    property.Next(true);
        //    _drawer.StaticInit();
        //    _drawer.Init(property);
        //    _drawer.Draw(position, root, property, label);
        //}

        protected override float GetCustomHeight(SerializedProperty property, GUIContent label)
        {
            property.Next(true);
            return EditorGUI.GetPropertyHeight(property, label);
        }
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            var root = property.Copy();
            property.Next(true);
            EditorGUI.PropertyField(position, property, label);
        }
    }
    //[CustomPropertyDrawer(typeof(ComponentTemplateFieldAttribute), true)]
    //internal class ComponentTemplateFieldDrawer : ExtendedPropertyDrawer<ComponentTemplateFieldAttribute>
    //{
    //    private const float DamagedComponentHeight = 18f * 2f;
    //    private ComponentTemplatesDropDown _componentDropDown;
    //    private PredicateTypesKey? _predicateOverride;
    //
    //    #region Properties
    //  private float Padding => Spacing;
    //  protected override bool IsInit => _componentDropDown != null;
    //  #endregion
    //
    //    public ComponentTemplateFieldDrawer() { }
    //    public ComponentTemplateFieldDrawer(PredicateTypesKey key)
    //    {
    //        _predicateOverride = key;
    //    }
    //
    //    #region Init
    //    protected override void OnInit(SerializedProperty property)
    //    {
    //        PredicateTypesKey key;
    //        if(_predicateOverride == null)
    //        {
    //            Type[] withOutTypes = Type.EmptyTypes;
    //            if (fieldInfo != null)
    //            {
    //                withOutTypes = fieldInfo.TryGetAttribute(out ReferenceButtonWithOutAttribute a) ? a.PredicateTypes : Array.Empty<Type>();
    //            }
    //            if (Attribute != null)
    //            {
    //                var types = Attribute.PredicateTypes;
    //                if(types == null || types.Length == 0)
    //                {
    //                    types = new Type[] { typeof(ITemplateNode) };
    //                }
    //                key = new PredicateTypesKey(typeof(ITemplateNode), types, withOutTypes);
    //            }
    //            else
    //            {
    //                key = new PredicateTypesKey(typeof(object), withOutTypes);
    //            }
    //            _predicateOverride = key;
    //        }
    //        _componentDropDown = ComponentTemplatesDropDown.Get(_predicateOverride.Value);
    //        _componentDropDown.OnSelected += SelectComponent;
    //    }
    //
    //    [ThreadStatic]
    //    private static SerializedProperty currentProperty;
    //    private static void SelectComponent(ComponentTemplatesDropDown.Item item)
    //    {
    //        //EcsGUI.Changed = true;
    //        object inst = item.Obj.CreateInstance();
    //        currentProperty.managedReferenceValue = inst;
    //        currentProperty.isExpanded = false;
    //        currentProperty.serializedObject.ApplyModifiedProperties();
    //    }
    //    #endregion
    //
    //    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //    {
    //        bool isSerializeReference = property.propertyType == SerializedPropertyType.ManagedReference;
    //        //#region No SerializeReference
    //        //if (property.propertyType != SerializedPropertyType.ManagedReference)
    //        //{
    //        //    return EditorGUI.GetPropertyHeight(property, label);
    //        //}
    //        //#endregion
    //
    //        if (isSerializeReference)
    //        {
    //            var instance = property.managedReferenceValue;
    //            IComponentTemplate template = instance as IComponentTemplate;
    //            if (instance == null)
    //            {
    //                return EditorGUIUtility.singleLineHeight + Padding * 2f;
    //            }
    //
    //            try
    //            {
    //                if (instance is ComponentTemplateBase customTemplate)
    //                {
    //                    property = property.FindPropertyRelative("component");
    //                }
    //            }
    //            catch
    //            {
    //                property = null;
    //            }
    //            if (property == null)
    //            {
    //                return DamagedComponentHeight;
    //            }
    //        }
    //        
    //        int propCount = EcsGUI.GetChildPropertiesCount(property);
    //
    //        return (propCount <= 0 ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(property, label)) + Padding * 4f;
    //    }
    //
    //    protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        Draw(position, property, property, label);
    //    }
    //    public void Draw(Rect rect, SerializedProperty rootProperty, SerializedProperty property, GUIContent label)
    //    {
    //        bool isSerializeReference = property.propertyType == SerializedPropertyType.ManagedReference;
    //        //#region No SerializeReference
    //        //if (isSerializeReference == false)
    //        //{
    //        //    EditorGUI.PropertyField(position, property, label, true);
    //        //    return;
    //        //}
    //        //#endregion
    //
    //        ITypeMeta meta = null;
    //        SerializedProperty componentProp = property;
    //        if (isSerializeReference)
    //        {
    //            var template = property.managedReferenceValue;
    //            if (template == null)
    //            {
    //                DrawSelectionPopup(rect, property, label);
    //                return;
    //            }
    //
    //            IComponentTemplate componentTemplate = template as IComponentTemplate;
    //            if (componentProp.managedReferenceValue is ComponentTemplateBase customTemplate)
    //            {
    //                componentProp = property.FindPropertyRelative("component");
    //            }
    //            if (componentProp == null)
    //            {
    //                DrawDamagedComponent(rect, "Damaged component template.");
    //                return;
    //            }
    //
    //            meta = template as ITypeMeta;
    //            if (meta == null)
    //            {
    //                if (componentTemplate != null)
    //                {
    //                    meta = componentTemplate.ComponentType.GetMeta();
    //                }
    //                else
    //                {
    //                    meta = template.GetMeta();
    //                }
    //            }
    //        }
    //        else
    //        {
    //            meta = fieldInfo.FieldType.GetMeta();
    //        }
    //
    //
    //        if (EcsGUI.DrawTypeMetaBlock(ref rect, rootProperty, meta).skip)
    //        {
    //            return;
    //        }
    //
    //        label.text = meta.Name;
    //        if (componentProp.propertyType == SerializedPropertyType.Generic)
    //        {
    //            EditorGUI.PropertyField(rect, componentProp, label, true);
    //        }
    //        else
    //        {
    //            EditorGUI.PropertyField(rect.AddPadding(0, 20f, 0, 0), componentProp, label, true);
    //        }
    //    }
    //
    //    private void DrawSelectionPopup(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        EditorGUI.LabelField(position, label);
    //        Rect buttonRect = RectUtility.AddPadding(position, EditorGUIUtility.labelWidth, 0f, 0f, 0f);
    //        if (GUI.Button(buttonRect, "Select"))
    //        {
    //            currentProperty = property;
    //            _componentDropDown.Show(buttonRect);
    //        }
    //    }
    //    private void DrawDamagedComponent(Rect position, string message)
    //    {
    //        EditorGUI.HelpBox(position, message, MessageType.Warning);
    //    }
    //}
}
#endif