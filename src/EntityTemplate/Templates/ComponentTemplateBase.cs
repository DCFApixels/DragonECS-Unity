using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static DCFApixels.DragonECS.IComponentTemplate;

namespace DCFApixels.DragonECS
{
    public interface IComponentTemplate : ITemplateNode
    {
        #region Properties
        Type Type { get; }
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
    public interface IComponentTemplateWithMetaOverride : IComponentTemplate, ITypeMeta { }

    [Serializable]
    public abstract class ComponentTemplateBase : IComponentTemplateWithMetaOverride
    {
        #region Properties
        public abstract Type Type { get; }
        public virtual bool IsCustomName { get { return false; } }
        public virtual string Name { get { return string.Empty; } }
        public virtual bool IsCustomColor { get { return false; } }
        public virtual MetaColor Color { get { return new MetaColor(MetaColor.Black); } }
        public virtual MetaGroup Group { get { return MetaGroup.Empty; } }
        public virtual MetaDescription Description { get { return MetaDescription.Empty; } }
        public virtual IReadOnlyCollection<string> Tags { get { return Array.Empty<string>(); } }
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
    public abstract class ComponentTemplateBase<T> : ComponentTemplateBase
    {
        protected static TypeMeta Meta = EcsDebugUtility.GetTypeMeta<T>();
        [SerializeField]
        protected T component;

        #region Properties
        public override Type Type { get { return typeof(T); } }
        public override bool IsCustomName { get { return Meta.IsCustomName; } }
        public override string Name { get { return Meta.Name; } }
        public override bool IsCustomColor { get { return Meta.IsCustomColor; } }
        public override MetaColor Color { get { return Meta.Color; } }
        public override MetaGroup Group { get { return Meta.Group; } }
        public override MetaDescription Description { get { return Meta.Description; } }
        public override IReadOnlyCollection<string> Tags { get { return Meta.Tags; } }
        #endregion

        #region Methods
        public override object GetRaw()
        {
            return component;
        }
        public override void SetRaw(object raw)
        {
            component = (T)raw;
        }
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
            List<Type> types = new List<Type>();
            Type interfaceType = typeof(IComponentTemplate);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var targetTypes = assembly.GetTypes().Where(type => !type.IsGenericType && !(type.IsAbstract || type.IsInterface) /*&& type.GetCustomAttribute<SerializableAttribute>() != null*/);

                types.AddRange(targetTypes.Where(type => interfaceType.IsAssignableFrom(type)));

                foreach (var t in targetTypes)
                {
                    if (t.IsSubclassOf(typeof(ComponentTemplateBase<>)))
                    {
                        types.Add(t);
                    }
                }
            }
            _types = types.ToArray();
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

