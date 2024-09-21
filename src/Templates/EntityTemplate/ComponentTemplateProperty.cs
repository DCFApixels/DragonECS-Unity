using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [Serializable]
    public struct ComponentTemplateProperty : IEquatable<ComponentTemplateProperty>
    {
        [SerializeReference]
        private IComponentTemplate _template;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComponentTemplateProperty(IComponentTemplate template)
        {
            _template = template;
        }
        public IComponentTemplate Template
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _template = value; }
        }
        public Type Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template.Type; }
        }
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _template == null; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Apply(short worldID, int entityID) { _template.Apply(worldID, entityID); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetRaw() { return _template.GetRaw(); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnGizmos(Transform transform, IComponentTemplate.GizmosMode mode) { _template.OnGizmos(transform, mode); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnValidate(UnityEngine.Object obj) { _template.OnValidate(obj); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRaw(object raw) { _template.SetRaw(raw); }
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

    public sealed class ComponentTemplateReferenceAttribute : PropertyAttribute { }
}