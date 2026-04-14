#if DISABLE_DEBUG
#undef DEBUG
#endif
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [Serializable]
    public struct ComponentTemplateProperty : IEquatable<ComponentTemplateProperty>
    {
        [SerializeReference]
        [ReferenceDropDown]
        [TypeMetaBlock]
        private ITemplateNode _template;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentTemplateProperty(ITemplateNode template)
        {
            _template = template;
        }
        public ITemplateNode Template
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _template = value; }
        }
        public Type Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template is IComponentTemplate tml ? tml.ComponentType : _template.GetType(); }
        }
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template == null; }
        }
        private IComponentTemplate Tmpl
        {
            get { return _template as IComponentTemplate; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Apply(short worldID, int entityID) { _template.Apply(worldID, entityID); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnGizmos(Transform transform, IComponentTemplate.GizmosMode mode) { Tmpl?.OnGizmos(transform, mode); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnValidate(UnityEngine.Object obj) { Tmpl?.OnValidate(obj); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetRaw()
        {
            if(_template is IComponentTemplate tmpl)
            {
                return tmpl.GetRaw();
            }
            return _template;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw(object raw) 
        {
            if (_template is IComponentTemplate tmpl)
            {
                tmpl.SetRaw(raw);
            }
            _template = (IComponentTemplate)raw;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ComponentTemplateProperty other) { return _template == other._template; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return _template.GetHashCode(); }
        public override bool Equals(object obj) { return obj is ComponentTemplateProperty other && Equals(other); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ComponentTemplateProperty a, ComponentTemplateProperty b) { return a._template == b._template; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ComponentTemplateProperty a, ComponentTemplateProperty b) { return a._template != b._template; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ComponentTemplateProperty a, Null? b) { return a.IsNull; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Null? a, ComponentTemplateProperty b) { return b.IsNull; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ComponentTemplateProperty a, Null? b) { return !a.IsNull; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Null? a, ComponentTemplateProperty b) { return !b.IsNull; }
        public readonly struct Null { }
    }
}