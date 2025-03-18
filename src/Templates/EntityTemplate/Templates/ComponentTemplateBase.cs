#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using static DCFApixels.DragonECS.IComponentTemplate;

namespace DCFApixels.DragonECS
{
    public interface IComponentTemplate : ITemplateNode
    {
        #region Properties
        Type Type { get; }
        bool IsUnique { get; }
        #endregion

        #region Methods
        object GetRaw();
        void SetRaw(object raw);
        void OnGizmos(Transform transform, GizmosMode mode);
        void OnValidate(UnityEngine.Object obj);
        #endregion

        public enum GizmosMode
        {
            Always,
            Selected
        }
    }

    [Serializable]
    public abstract class ComponentTemplateBase : IComponentTemplate, ITypeMeta
    {
        #region Properties
        public abstract Type Type { get; }
        public virtual ITypeMeta BaseMeta { get { return null; } }
        public virtual string Name { get { return string.Empty; } }
        public virtual MetaColor Color { get { return new MetaColor(MetaColor.Black); } }
        public virtual MetaGroup Group { get { return MetaGroup.Empty; } }
        public virtual MetaDescription Description { get { return MetaDescription.Empty; } }
        public virtual IReadOnlyList<string> Tags { get { return Array.Empty<string>(); } }
        public virtual bool IsUnique { get { return true; } }
        #endregion

        #region Methods
        public abstract object GetRaw();
        public abstract void SetRaw(object raw);
        public virtual void OnGizmos(Transform transform, GizmosMode mode) { }
        public virtual void OnValidate(UnityEngine.Object obj) { }

        public abstract void Apply(short worldID, int entityID);
        #endregion
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public abstract class ComponentTemplateBase<T> : ComponentTemplateBase
    {
        protected static readonly TypeMeta Meta = EcsDebugUtility.GetTypeMeta<T>();
        protected static readonly bool _isHasIEcsComponentLifecycle;
        protected static readonly IEcsComponentLifecycle<T> _iEcsComponentLifecycle;
        static ComponentTemplateBase()
        {
            _isHasIEcsComponentLifecycle = EcsComponentLifecycleHandler<T>.isHasHandler;
            _iEcsComponentLifecycle = EcsComponentLifecycleHandler<T>.instance;
        }
        private static T InitComponent()
        {
            T result = default;
            if (_isHasIEcsComponentLifecycle)
            {
                _iEcsComponentLifecycle.Enable(ref result);
            }
            return result;
        }

        [SerializeField]
        protected T component = InitComponent();
        [SerializeField]
        [HideInInspector]
        private byte _offset; // Avoids the error "Cannot get managed reference index with out bounds offset"

        #region Properties
        public sealed override ITypeMeta BaseMeta { get { return Meta; } }
        public sealed override Type Type { get { return typeof(T); } }
        public override string Name { get { return Meta.Name; } }
        public override MetaColor Color { get { return Meta.Color; } }
        public override MetaGroup Group { get { return Meta.Group; } }
        public override MetaDescription Description { get { return Meta.Description; } }
        public override IReadOnlyList<string> Tags { get { return Meta.Tags; } }
        #endregion

        #region Methods
        public sealed override object GetRaw() { return component; }
        public sealed override void SetRaw(object raw) { component = (T)raw; }
        #endregion
    }

    public abstract class ComponentTemplate<T> : ComponentTemplateBase<T>
        where T : struct, IEcsComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsWorld.GetPoolInstance<EcsPool<T>>(worldID).TryAddOrGet(entityID) = component;
        }
    }
    public abstract class TagComponentTemplate<T> : ComponentTemplateBase<T>
        where T : struct, IEcsTagComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsWorld.GetPoolInstance<EcsTagPool<T>>(worldID).Set(entityID, true);
        }
    }
}

namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class ComponentTemplateExtensions
    {
        private static MethodInfo memberwiseCloneMethdo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static IComponentTemplate Clone(this IComponentTemplate obj)
        {
            return (IComponentTemplate)memberwiseCloneMethdo.Invoke(obj, null);
        }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class ComponentTemplateTypeCache
    {
        private static Type[] _types;
        private static IComponentTemplate[] _dummies;
        internal static ReadOnlySpan<Type> Types
        {
            get { return _types; }
        }
        internal static ReadOnlySpan<IComponentTemplate> Dummies
        {
            get { return _dummies; }
        }

        static ComponentTemplateTypeCache()
        {
            Type interfaceType = typeof(IComponentTemplate);

            _types = UnityEditorUtility._serializableTypes.Where(type => interfaceType.IsAssignableFrom(type)).ToArray();
            foreach (var type in _types)
            {
                EcsDebugUtility.GetTypeMeta(type);
            }
            _dummies = new IComponentTemplate[_types.Length];

            for (int i = 0; i < _types.Length; i++)
            {
                _dummies[i] = (IComponentTemplate)Activator.CreateInstance(_types[i]);
            }
        }
    }
}
#endif

