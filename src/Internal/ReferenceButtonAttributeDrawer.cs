using System;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal interface IReferenceButtonAttribute
    {
        Type[] PredicateTypes { get; }
        bool IsHideButtonIfNotNull { get; }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Internal
{
    using DCFApixels.DragonECS.Unity.Editors;
    using UnityEditor;
    internal partial class UnityReflectionCache
    {
        public bool IsReferenceButtonCacheInit_Editor;
        public bool InitReferenceButtonCache_Editor(SerializedProperty sp)
        {
            if (IsReferenceButtonCacheInit_Editor) { return false; }

            HasSerializableData_Editor = sp.HasSerializableData();

            IsReferenceButtonCacheInit_Editor = true;
            return true;
        }
        public bool HasSerializableData_Editor;
    }
}

namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ComponentTemplateReferenceAttribute), true)]
    [CustomPropertyDrawer(typeof(ReferenceButtonAttribute), true)]
    internal sealed class ReferenceButtonAttributeDrawer : ExtendedPropertyDrawer<IReferenceButtonAttribute>
    {
        private Type[] _withOutTypes;
        protected override void OnInit()
        {
            Type fieldType = fieldInfo.FieldType;
            _withOutTypes = fieldType.TryGetAttribute(out ReferenceButtonWithOutAttribute a) ? a.PredicateTypes : Array.Empty<Type>();
            //if (fieldType.IsGenericType)
            //{
            //    if (fieldType.IsGenericTypeDefinition == false)
            //    {
            //        fieldType = fieldType.GetGenericTypeDefinition();
            //    }
            //}
        }

        private UnityReflectionCache _reflectionCache;
        private UnityReflectionCache Cahce(SerializedProperty sp)
        {
            if (UnityReflectionCache.InitLocal(sp.managedReferenceValue.GetType(), ref _reflectionCache))
            {
                _reflectionCache.InitReferenceButtonCache_Editor(sp);
            }
            return _reflectionCache;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init();
            if (property.managedReferenceValue != null &&
                Cahce(property).HasSerializableData_Editor)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return OneLineHeight;
            }
        }

        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsArrayElement)
            {
                label = UnityEditorUtility.GetLabelTemp();
            }
            Rect selButtnoRect = position;
            selButtnoRect.height = OneLineHeight;
            DrawSelectionPopupButton(selButtnoRect, property);

            if (property.managedReferenceValue != null &&
                Cahce(property).HasSerializableData_Editor)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.LabelField(position, label);
                EditorGUI.EndProperty();
            }
        }

        private void DrawSelectionPopupButton(Rect position, SerializedProperty property)
        {
            Rect buttonRect = IsArrayElement ? position : position.AddPadding(EditorGUIUtility.labelWidth, 0f, 0f, 0f); ;
            EcsGUI.DrawSelectReferenceButton(buttonRect, property, Attribute.PredicateTypes.Length == 0 ? new Type[1] { fieldInfo.FieldType } : Attribute.PredicateTypes, _withOutTypes, Attribute.IsHideButtonIfNotNull);
        }
    }
}
#endif