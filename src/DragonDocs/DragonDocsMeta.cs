﻿using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Docs
{
    [Serializable]
    [DataContract]
    public class DragonDocsMeta : IComparable<DragonDocsMeta>
    {
        [NonSerialized] private Type _sourceType;
        [NonSerialized] private bool _isInitSourceType = false;

        [DataMember, SerializeField] internal string _assemblyQualifiedName = string.Empty;
        //[DataMember, SerializeField] internal EcsMemberType _memberType = EcsMemberType.Undefined;

        [DataMember, SerializeField] internal string _name = string.Empty;
        [DataMember, SerializeField] internal bool _isCustomName = false;
        [DataMember, SerializeField] internal string _typeName = string.Empty;
        [DataMember, SerializeField] internal Color32 _color = new Color32(0, 0, 0, 255);
        [DataMember, SerializeField] internal bool _isCustomColor = false;
        [DataMember, SerializeField] internal string _autor = string.Empty;
        [DataMember, SerializeField] internal string _description = string.Empty;

        [DataMember, SerializeField] internal string _group = string.Empty;
        [DataMember, SerializeField] internal string[] _tags = Array.Empty<string>();

        [DataMember, SerializeField] internal string _metaID = string.Empty;

        [DataMember, SerializeField] internal bool _isHidden = false;


        public string AssemblyQualifiedName
        {
            get { return _assemblyQualifiedName; }
        }
        //public EcsMemberType EcsMemberType
        //{
        //    get { return _memberType; }
        //}
        public string Name
        {
            get { return _name; }
        }
        public bool IsCustomName
        {
            get { return _isCustomName; }
        }
        public string TypeName
        {
            get { return _typeName; }
        }
        public Color32 Color
        {
            get { return _color; }
        }
        public bool IsCustomColor
        {
            get { return _isCustomColor; }
        }
        public string Autor
        {
            get { return _autor; }
        }
        public string Description
        {
            get { return _description; }
        }
        public string Group
        {
            get { return _group; }
        }
        public ReadOnlySpan<string> Tags
        {
            get { return _tags; }
        }
        public string MetaID
        {
            get { return _metaID; }
        }
        public bool IsHidden
        {
            get { return _isHidden; }
        }

        public DragonDocsMeta(TypeMeta meta)
        {
            _sourceType = meta.Type;
            _assemblyQualifiedName = meta.Type.AssemblyQualifiedName;
            //_memberType = meta.EcsMemberType;

            _name = meta.Name;
            _isCustomName = meta.IsCustomName;
            _typeName = meta.TypeName;
            _color = meta.Color.ToUnityColor32();
            _isCustomColor = meta.IsCustomColor;
            _autor = meta.Description.Author;


            if (meta.Description.IsHasAutor)
            {
                _description = $"{meta.Description.Text}\r\n  - {meta.Description.Author}";
            }
            else
            {
                _description = meta.Description.Text;
            }

            _group = meta.Group.Name;
            _tags = new string[meta.Tags.Count];
            for (int i = 0, n = meta.Tags.Count; i < n; i++)
            {
                _tags[i] = meta.Tags[i];
            }

            _metaID = meta.MetaID;

            _isHidden = meta.IsHidden;
        }

        public bool TryGetSourceType(out Type type)
        {
            type = GetSourceType();
            return type != null;
        }
        private Type GetSourceType()
        {
            if (_isInitSourceType) { return _sourceType; }
            _isInitSourceType = true;
            _sourceType = Type.GetType(_assemblyQualifiedName);
            return _sourceType;
        }

        int IComparable<DragonDocsMeta>.CompareTo(DragonDocsMeta other)
        {
            int c = string.Compare(_group, other._group);
            return c == 0 ? string.Compare(_name, other._name) : c;
        }
    }
}