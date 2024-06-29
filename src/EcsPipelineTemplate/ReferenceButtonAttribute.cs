using DCFApixels.DragonECS;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal sealed class ReferenceButtonAttribute : PropertyAttribute { }
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
        private static Dictionary<Type, ReferenceDropDown> _predicatTypesMenus = new Dictionary<Type, ReferenceDropDown>();

        private class ReferenceDropDown : AdvancedDropdown
        {
            public readonly Type PredicateType;
            public ReferenceDropDown(Type predicateType) : base(new AdvancedDropdownState())
            {
                PredicateType = predicateType;

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
                    if (PredicateType.IsAssignableFrom(type))
                    {
                        ITypeMeta meta = type.ToMeta();
                        string name = meta.Name;
                        string description = meta.Description.Text;
                        MetaGroup group = meta.Group;
                        var splitedGroup = group.Splited;

                        Item parent = root;
                        if(splitedGroup.Count > 0)
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

                        var leafItem = new Item(type, name, increment++);
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
                    IEnumerator<string> splitedEnumOther = Group.Splited.GetEnumerator();
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

            //private readonly struct Key : IEquatable<Key>
            //{
            //    public readonly string FullName;
            //    public readonly int Length;
            //    public Key(string fullName, int length)
            //    {
            //        FullName = fullName;
            //        Length = length;
            //    }
            //    public bool Equals(Key other)
            //    {
            //        if (Length != other.Length)
            //        {
            //            return false;
            //        }
            //        for (int i = 0; i < Length; i++)
            //        {
            //            if (FullName[i] != other.FullName[i])
            //            {
            //                return false;
            //            }
            //        }
            //        return true;
            //    }
            //    public override bool Equals(object obj)
            //    {
            //        return obj is Key key && Equals(key);
            //    }
            //    public override int GetHashCode()
            //    {
            //        unchecked
            //        {
            //            int state = Length;
            //            state ^= state << 13;
            //            state ^= state >> 17;
            //            state ^= state << 5;
            //            return FullName.GetHashCode() ^ state;
            //        };
            //    }
            //}
            #endregion

        }

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

        private static ReferenceDropDown GetReferenceDropDown(Type predicatType)
        {
            Init();
            if (_predicatTypesMenus.TryGetValue(predicatType, out ReferenceDropDown menu) == false)
            {
                menu = new ReferenceDropDown(predicatType);
                menu.OnSelected += SelectComponent;
                _predicatTypesMenus.Add(predicatType, menu);
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
                GetReferenceDropDown(fieldInfo.FieldType).Show(buttonRect);
            }
        }
    }
}
#endif


[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.SYSTEMS_GROUP)]
[System.Serializable] public class TestSystem0 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.SYSTEMS_GROUP)]
[System.Serializable] public class TestSystem1 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.SYSTEMS_GROUP)]
[System.Serializable] public class TestSystem2 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP, EcsConsts.SYSTEMS_GROUP)]
[System.Serializable] public class TestSystem3 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP)]
[System.Serializable] public class TestSystem4 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP)]
[System.Serializable] public class TestSystem7 : IEcsProcess { }
[MetaGroup(EcsConsts.PACK_GROUP)]
[System.Serializable] public class TestSystem8 : IEcsProcess { }
[System.Serializable] public class _TestSystemX : IEcsProcess { }
[System.Serializable] public class TestSystem9 : IEcsProcess { }
[System.Serializable] public class TestSystem5 : IEcsProcess { }
[System.Serializable] public class TestSystem6 : IEcsProcess { }