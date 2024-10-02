#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class SystemsDropDown : MetaObjectsDropDown<Type>
    {
        public SystemsDropDown()
        {
            Type[] predicateTypes = new Type[] { typeof(IEcsModule), typeof(IEcsProcess) };
            IEnumerable<(Type, ITypeMeta)> itemMetaPairs = UnityEditorUtility._serializableTypes.Where(o =>
            {
                foreach (Type predicateTypes in predicateTypes)
                {
                    if (predicateTypes.IsAssignableFrom(o))
                    {
                        return true;
                    }
                }
                return false;
            }).Select(o => (o, (ITypeMeta)o.ToMeta()));
            Setup(itemMetaPairs);
        }

        private SerializedProperty _arrayProperty;
        private SerializedProperty _fieldProperty;

        public void OpenForArray(Rect position, SerializedProperty arrayProperty)
        {
            _arrayProperty = arrayProperty;
            _fieldProperty = null;
            Show(position);
        }
        public void OpenForField(Rect position, SerializedProperty fieldProperty)
        {
            _arrayProperty = null;
            _fieldProperty = fieldProperty;
            Show(position);
        }

        protected override void ItemSelected(Item item)
        {
            base.ItemSelected(item);

            Type type = item.Obj;

            if (_arrayProperty != null && type != null)
            {
                int index = _arrayProperty.arraySize;
                _arrayProperty.arraySize += 1;
                _fieldProperty = _arrayProperty.GetArrayElementAtIndex(index);
                _fieldProperty.Next(true);//Смещение чтобы перейти к полю Traget внутри рекорда
            }

            if (_fieldProperty != null)
            {
                if (type == null)
                {
                    _fieldProperty.managedReferenceValue = null;
                }
                else
                {
                    _fieldProperty.managedReferenceValue = Activator.CreateInstance(type);
                    _fieldProperty.isExpanded = true;
                }

                _fieldProperty.serializedObject.ApplyModifiedProperties();
                EcsGUI.DelayedChanged = true;
            }
        }
    }
    internal class ComponentDropDown : MetaObjectsDropDown<IComponentTemplate>
    {
        public ComponentDropDown()
        {
            IEnumerable<(IComponentTemplate, ITypeMeta)> itemMetaPairs = ComponentTemplateTypeCache.Dummies.ToArray().Select(dummy =>
            {
                ITypeMeta meta;
                if (dummy is IComponentTemplateWithMetaOverride withMetaOverride)
                {
                    meta = withMetaOverride;
                }
                else
                {
                    meta = dummy.Type.GetMeta();
                }
                return (dummy, meta);
            });
            Setup(itemMetaPairs);
        }

        private bool _isCheckUnique;
        private SerializedProperty _arrayProperty;
        private SerializedProperty _fieldProperty;

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
            base.ItemSelected(item);

            if (item.Obj == null)
            {
                _fieldProperty.managedReferenceValue = null;
                _fieldProperty.serializedObject.ApplyModifiedProperties();
                return;
            }

            Type componentType = item.Obj.GetType();
            IComponentTemplate cmptmp = item.Obj;

            if (_arrayProperty != null && cmptmp != null)
            {
                int index = _arrayProperty.arraySize;
                if (_isCheckUnique)
                {
                    if (cmptmp.IsUnique)
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
                _fieldProperty.managedReferenceValue = cmptmp.Clone();
                _fieldProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
    internal class RuntimeComponentDropDown : MetaObjectsDropDown<IEcsPool>
    {
        public RuntimeComponentDropDown(IEnumerable<IEcsPool> pools)
        {
            IEnumerable<(IEcsPool, ITypeMeta)> itemMetaPairs = pools.Select(pool =>
            {
                return (pool, (ITypeMeta)pool.ComponentType.GetMeta());
            });
            Setup(itemMetaPairs);
        }

        private int _entityID;

        public void Open(Rect position, int entityID)
        {
            _entityID = entityID;
            Show(position);
        }

        protected override void ItemSelected(Item item)
        {
            IEcsPool pool = item.Obj;
            if (pool.World.IsUsed(_entityID) == false)
            {
                return;
            }
            if (pool.Has(_entityID) == false)
            {
                pool.AddRaw(_entityID, Activator.CreateInstance(pool.ComponentType));
            }
            else
            {
                Debug.LogWarning($"Entity({_entityID}) already has component {EcsDebugUtility.GetGenericTypeName(pool.ComponentType)}.");
            }
        }
    }
    internal class MetaObjectsDropDown<T> : AdvancedDropdown
    {
        private string _name;
        private bool _isContainsNull;
        private IEnumerable<(T, ITypeMeta)> _itemMetaPairs;
        public MetaObjectsDropDown() : base(new AdvancedDropdownState())
        {
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * 30);

        }
        protected void Setup(IEnumerable<(T, ITypeMeta)> itemMetaPairs, string name = "Select Type...", bool isContainsNull = true)
        {
            _name = name;
            _isContainsNull = isContainsNull;
            _itemMetaPairs = itemMetaPairs;
        }
        protected override AdvancedDropdownItem BuildRoot()
        {
            int increment = 0;
            var root = new Item(default, _name, increment++);

            if (_isContainsNull)
            {
                root.AddChild(new Item(default, "<NULL>", increment++));
            }

            Dictionary<Key, Item> dict = new Dictionary<Key, Item>();

            foreach (var pair in _itemMetaPairs)
            {
                ITypeMeta meta = pair.Item2;

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
                            item = new Item(default, subgroup, increment++);
                            parent.AddChild(item);
                            dict.Add(key, item);
                        }
                        parent = item;
                        i++;
                    }
                }

                var leafItem = new Item(pair.Item1, meta.Name, increment++);
                parent.AddChild(leafItem);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            var tType = (Item)item;
            ItemSelected(tType);
            OnSelected(tType);
        }
        protected virtual void ItemSelected(Item item) { }


        public event Action<Item> OnSelected = delegate { };

        public class Item : AdvancedDropdownItem
        {
            public readonly T Obj;
            public Item(T obj, string name, int id) : base(name)
            {
                Obj = obj;
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
}
#endif