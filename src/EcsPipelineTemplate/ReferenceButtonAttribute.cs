using DCFApixels.DragonECS;
using System;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal sealed class ReferenceButtonAttribute : PropertyAttribute
    {
        public readonly Type[] predicateTypes;
        public ReferenceButtonAttribute() : this(Array.Empty<Type>()) { }
        public ReferenceButtonAttribute(params Type[] predicateTypes)
        {
            this.predicateTypes = predicateTypes;
            Array.Sort(predicateTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));
        }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityObject = UnityEngine.Object;

    [CustomPropertyDrawer(typeof(ReferenceButtonAttribute))]
    internal sealed class ReferenceButtonAttributeDrawer : PropertyDrawer
    {
        private static bool _isInit;

        private static Type[] _serializableTypes;
        private static Dictionary<PredicateTypesKey, ReferenceDropDown> _predicatTypesMenus = new Dictionary<PredicateTypesKey, ReferenceDropDown>();
        private ReferenceButtonAttribute TargetAttribute => (ReferenceButtonAttribute)attribute;

        #region PredicateTypesKey
        private readonly struct PredicateTypesKey : IEquatable<PredicateTypesKey>
        {
            public readonly Type[] types;
            public PredicateTypesKey(Type[] types)
            {
                this.types = types;
            }
            public bool Equals(PredicateTypesKey other)
            {
                if (types.Length != other.types.Length) { return false; }
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] != other.types[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            public override bool Equals(object obj)
            {
                return obj is PredicateTypesKey key && Equals(key);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(types);
            }
            public static implicit operator PredicateTypesKey(Type[] types) { return new PredicateTypesKey(types); }
            public static implicit operator Type[](PredicateTypesKey key) { return key.types; }
        }
        #endregion

        #region ReferenceDropDown
        private class ReferenceDropDown : AdvancedDropdown
        {
            public readonly Type[] PredicateTypes;
            public ReferenceDropDown(Type[] predicateTypes) : base(new AdvancedDropdownState())
            {
                PredicateTypes = predicateTypes;
                minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * 30);
            }
            protected override AdvancedDropdownItem BuildRoot()
            {
                int increment = 0;
                var root = new Item(null, "Select Type", increment++);
                root.AddChild(new Item(null, "<NULL>", increment++));

                Dictionary<Key, Item> dict = new Dictionary<Key, Item>();

                foreach (var type in _serializableTypes)
                {
                    bool isAssignable = false;
                    foreach (Type predicateTypes in PredicateTypes)
                    {
                        if (predicateTypes.IsAssignableFrom(type))
                        {
                            isAssignable = true;
                            break;
                        }
                    }
                    if (isAssignable)
                    {
                        ITypeMeta meta = type.ToMeta();
                        string description = meta.Description.Text;
                        MetaGroup group = meta.Group;
                        var splitedGroup = group.Splited;

                        Item parent = root;
                        if (splitedGroup.Count > 0)
                        {
                            int i = 1;
                            foreach (var subgroup in splitedGroup)
                            {
                                Key key = new Key(group, i);
                                if (dict.TryGetValue(key, out Item item) == false)
                                {
                                    item = new Item(null, subgroup, increment++);
                                    parent.AddChild(item);
                                    dict.Add(key, item);
                                }
                                parent = item;
                                i++;
                            }
                        }

                        var leafItem = new Item(type, meta.Name, increment++);
                        parent.AddChild(leafItem);
                    }
                }
                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                OnSelected((Item)item);
            }

            public event Action<Item> OnSelected = delegate { };

            public class Item : AdvancedDropdownItem
            {
                public readonly Type Type;
                public Item(Type type, string name, int id) : base(name)
                {
                    Type = type;
                    this.id = id;
                }
            }

            #region Key
            private readonly struct Key : IEquatable<Key>
            {
                public readonly MetaGroup Group;
                public readonly int Length;
                public Key(MetaGroup group, int length)
                {
                    Group = group;
                    Length = length;
                }
                public bool Equals(Key other)
                {
                    if (Length != other.Length)
                    {
                        return false;
                    }
                    IEnumerator<string> splitedEnum = Group.Splited.GetEnumerator();
                    IEnumerator<string> splitedEnumOther = other.Group.Splited.GetEnumerator();
                    for (int i = 0; i < Length; i++)
                    {
                        splitedEnum.MoveNext();
                        splitedEnumOther.MoveNext();
                        if (splitedEnum.Current != splitedEnumOther.Current)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                public override bool Equals(object obj)
                {
                    return obj is Key key && Equals(key);
                }
                public override int GetHashCode()
                {
                    unchecked
                    {
                        int state = Length;
                        state ^= state << 13;
                        state ^= state >> 17;
                        state ^= state << 5;
                        var x = Group.Splited.GetEnumerator();
                        x.MoveNext();
                        return x.Current.GetHashCode() ^ state;
                    };
                }
            }
            #endregion
        }
        #endregion

        #region Init
        private static void Init()
        {
            if (_isInit) { return; }

            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var targetTypes = assembly.GetTypes().Where(type =>
                    (type.IsGenericType || type.IsAbstract || type.IsInterface) == false &&
                    type.IsSubclassOf(typeof(UnityObject)) == false &&
                    type.GetCustomAttribute<SerializableAttribute>() != null);

                types.AddRange(targetTypes);
            }
            _serializableTypes = types.ToArray();
            _isInit = true;
        }

        private static ReferenceDropDown GetReferenceDropDown(Type[] predicatTypes)
        {
            Init();
            if (_predicatTypesMenus.TryGetValue(predicatTypes, out ReferenceDropDown menu) == false)
            {
                menu = new ReferenceDropDown(predicatTypes);
                menu.OnSelected += SelectComponent;
                _predicatTypesMenus.Add(predicatTypes, menu);
            }

            return menu;
        }

        [ThreadStatic]
        private static SerializedProperty currentProperty;
        private static void SelectComponent(ReferenceDropDown.Item item)
        {
            Type type = item.Type;
            if (type == null)
            {
                currentProperty.managedReferenceValue = null;
            }
            else
            {
                currentProperty.managedReferenceValue = Activator.CreateInstance(type);
                currentProperty.isExpanded = true;
            }

            currentProperty.serializedObject.ApplyModifiedProperties();

            EcsGUI.Changed = true;
        }
        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.managedReferenceValue != null)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect selButtnoRect = position;
            selButtnoRect.height = EditorGUIUtility.singleLineHeight;
            DrawSelectionPopup(selButtnoRect, property, label);

            if (property.managedReferenceValue != null)
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

        private void DrawSelectionPopup(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect buttonRect = RectUtility.AddPadding(position, EditorGUIUtility.labelWidth, 0f, 0f, 0f);
            object obj = property.hasMultipleDifferentValues ? null : property.managedReferenceValue;
            if (GUI.Button(buttonRect, obj == null ? "Select..." : obj.GetMeta().Name, EditorStyles.layerMaskField))
            {
                currentProperty = property;
                if (TargetAttribute.predicateTypes.Length == 0)
                {
                    GetReferenceDropDown(new Type[1] { fieldInfo.FieldType }).Show(buttonRect);
                }
                else
                {
                    GetReferenceDropDown(TargetAttribute.predicateTypes).Show(buttonRect);
                }
            }
        }
    }
}
#endif


[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.SYSTEMS_GROUP)]
[System.Serializable] public class TestSystem0 : IEcsProcess { }
[System.Serializable] public class TestSystem1 : IEcsProcess { }
[System.Serializable] public class TestSystem2 : IEcsProcess { }
[System.Serializable] public class TestSystem3 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP)]
[System.Serializable] public class TestSystem4 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP)]
[System.Serializable] public class TestSystem7 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.AUTHOR)]
[System.Serializable] public class TestSystem8 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.AUTHOR)]
[System.Serializable] public class _TestSystemX : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.AUTHOR)]
[System.Serializable] public class TestSystem9 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.AUTHOR)]
[System.Serializable] public class TestSystem5 : IEcsProcess { }
[System.Serializable] public class TestSystem6 : IEcsProcess { }