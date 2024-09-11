#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
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
        protected void Setup(IEnumerable<(T, ITypeMeta)> itemMetaPairs, string name = "Select Type", bool isContainsNull = true)
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
            OnSelected((Item)item);
        }

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