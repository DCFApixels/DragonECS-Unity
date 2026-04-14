#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class DragonMemberWrapperAttribute : Attribute
    {
        public string WrappedFieldName;
        public DragonMemberWrapperAttribute(string wrappedFieldName)
        {
            WrappedFieldName = wrappedFieldName;
        }
    }
}

namespace DCFApixels.DragonECS
{

    public interface IComponentTemplate : ITemplateNode
    {
        #region Properties
        Type ComponentType { get; }
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
    [MetaProxy(typeof(ComponentTemplateMetaProxy))]
    [DragonMemberWrapper("component")]
    public abstract class ComponentTemplateBase : IComponentTemplate
    {
        #region Properties
        public abstract Type ComponentType { get; }
        public virtual bool IsUnique { get { return true; } }
        #endregion

        #region Methods
        public abstract object GetRaw();
        public abstract void SetRaw(object raw);
        public virtual void OnGizmos(Transform transform, IComponentTemplate.GizmosMode mode) { }
        public virtual void OnValidate(UnityEngine.Object obj) { }

        public abstract void Apply(short worldID, int entityID);
        #endregion

        #region MetaProxy
        protected class ComponentTemplateMetaProxy : MetaProxyBase
        {
            protected TypeMeta Meta;
            public override string Name { get { return Meta?.Name; } }
            public override MetaColor? Color { get { return Meta != null && Meta.IsCustomColor ? Meta.Color : null; } }
            public override MetaGroup Group { get { return Meta?.Group; } }
            public override MetaDescription Description { get { return Meta?.Description; } }
            public override IEnumerable<string> Tags { get { return Meta?.Tags; } }
            public ComponentTemplateMetaProxy(Type type) : base(type)
            {
                Meta = null;
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.Name == "component")
                    {
                        Meta = field.FieldType.GetMeta();
                        return;
                    }
                }

            }
        }
        #endregion
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public abstract class ComponentTemplateBase<T> : ComponentTemplateBase, ICloneable
    {
        protected static readonly TypeMeta Meta = EcsDebugUtility.GetTypeMeta<T>();

        private static bool _defaultValueInit = false;
        private static bool _hasDefaultValue = false;
        private static T _defaultValue;
        private static ICloneable _defaultValueCloneable;
        private static CloneMethod _defaultValueCloneMethod;
        private enum CloneMethod
        {
            Set,
            Clone_Reflection,
            ICloneable,
        }
        protected static void InitStatic()
        {
            if (_defaultValueInit == false)
            {
                _hasDefaultValue = false;
                Type type = typeof(T);
                FieldInfo field;
                field = type.GetField("Default", BindingFlags.Static | BindingFlags.Public);
                if (field != null && field.FieldType == type)
                {
                    _defaultValue = (T)field.GetValue(null);
                    _hasDefaultValue = true;
                }
                if(_hasDefaultValue == false)
                {
                    field = type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                    if (field != null && field.FieldType == type)
                    {
                        _defaultValue = (T)field.GetValue(null);
                        _hasDefaultValue = true;
                    }
                }
                if (_defaultValue == null)
                {
                    _hasDefaultValue = false;
                }

                if (_hasDefaultValue)
                {
                    _defaultValueCloneable = _defaultValue as ICloneable;
                    _defaultValueCloneMethod = CloneMethod.Set;

                    if (type.IsValueType == false)
                    {
                        _defaultValueCloneMethod = CloneMethod.Clone_Reflection;
                    }
                    if (_defaultValueCloneable != null)
                    {
                        _defaultValueCloneMethod = CloneMethod.ICloneable;
                    }
                }

                _defaultValueInit = true;
            }
        }
        protected virtual T DefaultComponent
        {
            get
            {
                if (_hasDefaultValue)
                {
                    return CloneComponent(_defaultValue);
                }
                return default;
            }
        }

        [SerializeField]
        protected T component;
        [SerializeField]
        [HideInInspector]
        private byte _offset; // Avoids the error "Cannot get managed reference index with out bounds offset"

        public ComponentTemplateBase()
        {
            InitStatic();
            component = DefaultComponent;
        }

        public sealed override Type ComponentType { get { return typeof(T); } }

        #region Methods
        public sealed override object GetRaw() { return component; }
        public sealed override void SetRaw(object raw) { component = (T)raw; }
        protected virtual T CloneComponent(T component)
        {
            switch (_defaultValueCloneMethod)
            {
                case CloneMethod.Set:
                    return component;
                case CloneMethod.Clone_Reflection:
                    return (T)component.Clone_Reflection();
                case CloneMethod.ICloneable:
                    return (T)_defaultValueCloneable.Clone();
            }
            return default;
        }
        object ICloneable.Clone()
        {
            ComponentTemplateBase<T> templateClone = (ComponentTemplateBase<T>)MemberwiseClone();
            templateClone.component = CloneComponent(component);
            return templateClone;
        }
        #endregion
    }
    [System.Serializable]
    public class ComponentTemplate<T> : ComponentTemplateBase<T>
        where T : struct, IEcsComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsPool<T>.Apply(ref component, entityID, worldID);
        }
    }
    [System.Serializable]
    public class TagComponentTemplate<T> : ComponentTemplateBase<T>
        where T : struct, IEcsTagComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsTagPool<T>.Apply(ref component, entityID, worldID);
        }
    }
}

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class ComponentTemplateTypeCache
    {
        private static ComponentTemplateTypeCache[] _all;
        internal static ReadOnlySpan<ComponentTemplateTypeCache> All
        {
            get { return _all; }
        }

        static ComponentTemplateTypeCache()
        {
            List<ComponentTemplateTypeCache> list = new List<ComponentTemplateTypeCache>(256);
            foreach (var type in UnityEditorUtility._serializableTypes)
            {
                //Debug.Log(type.Name);
                if (typeof(ITemplateNode).IsAssignableFrom(type) && (typeof(IComponentTemplate).IsAssignableFrom(type) || typeof(IEcsComponentMember).IsAssignableFrom(type)))
                {
                    ComponentTemplateTypeCache element = new ComponentTemplateTypeCache(type);
                    list.Add(element);
                }
            }
            _all = list.ToArray();
        }


        public readonly Type Type;
        public readonly Type ComponentType;
        public readonly bool IsUnique;
        private ITypeMeta _meta;
        public ITypeMeta Meta
        {
            get
            {
                if (_meta == null)
                {
                    {
                        _meta = ComponentType.GetMeta();
                    }
                }
                return _meta;
            }
        }
        private bool _defaultValueTypeInit = false;
        private object _defaultValueDummy;
        public object DefaultValue
        {
            get
            {
                if (_defaultValueTypeInit == false)
                {
                    if (Type.IsValueType)
                    {
                        FieldInfo field;
                        field = Type.GetField("Default", BindingFlags.Static | BindingFlags.Public);
                        if (field != null && field.FieldType == Type)
                        {
                            _defaultValueDummy = field.GetValue(null).Clone_Reflection();
                        }

                        if(_defaultValueDummy == null)
                        {
                            field = Type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                            if (field != null && field.FieldType == Type)
                            {
                                _defaultValueDummy = field.GetValue(null).Clone_Reflection();
                            }
                        }
                    }
                    _defaultValueTypeInit = true;
                }
                return _defaultValueDummy;
            }
        }
        public ComponentTemplateTypeCache(Type type)
        {
            Type = type;

            IsUnique = false;
            if (type.GetInterfaces().Contains(typeof(ITypeMeta)))
            {
                var metaOverride = (ITypeMeta)Activator.CreateInstance(type);
                _meta = metaOverride;
            }

            if (type.GetInterfaces().Contains(typeof(IComponentTemplate)))
            {
                var ct = (IComponentTemplate)Activator.CreateInstance(type);
                IsUnique = ct.IsUnique;
                ComponentType = ct.ComponentType;
            }
            else
            {
                ComponentType = Type;
            }
        }
        public object CreateInstance()
        {
            if(DefaultValue != null)
            {
                return DefaultValue.Clone_Reflection();
            }
            return Activator.CreateInstance(Type);
        }
    }
}
#endif

