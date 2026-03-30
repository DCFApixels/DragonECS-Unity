#if DISABLE_DEBUG
#undef DEBUG
#endif
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
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
    public abstract class ComponentTemplateBase<T> : ComponentTemplateBase, ICloneable
        where T : struct
    {
        protected static readonly TypeMeta Meta = EcsDebugUtility.GetTypeMeta<T>();

        private static bool _defaultValueTypeInit = false;
        private static T _defaultValueType;
        protected static T DefaultValueType
        {
            get
            {
                if (_defaultValueTypeInit == false)
                {
                    Type type = typeof(T);
                    if (type.IsValueType)
                    {
                        FieldInfo field;
                        field = type.GetField("Default", BindingFlags.Static | BindingFlags.Public);
                        if (field != null && field.FieldType == type)
                        {
                            _defaultValueType = (T)field.GetValue(null);
                        }
                        field = type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                        if (field != null && field.FieldType == type)
                        {
                            _defaultValueType = (T)field.GetValue(null);
                        }
                    }
                    _defaultValueTypeInit = true;
                }
                return _defaultValueType;
            }
        }

        [SerializeField]
        protected T component = DefaultValueType;
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
        protected virtual T CloneComponent() { return component; }
        object ICloneable.Clone()
        {
            ComponentTemplateBase<T> templateClone = (ComponentTemplateBase<T>)MemberwiseClone();
            templateClone.component = CloneComponent();
            return templateClone;
        }
        #endregion
    }

    public abstract class ComponentTemplate<T> : ComponentTemplateBase<T>
        where T : struct, IEcsComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsPool<T>.Apply(ref component, entityID, worldID);
        }
    }
    public abstract class TagComponentTemplate<T> : ComponentTemplateBase<T>
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
                    _meta = ComponentType.GetMeta();
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
                            _defaultValueDummy = field.GetValue(null);
                        }

                        if(_defaultValueDummy == null)
                        {
                            field = Type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                            if (field != null && field.FieldType == Type)
                            {
                                _defaultValueDummy = field.GetValue(null);
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
            if (typeof(IComponentTemplate).IsAssignableFrom(type))
            {
                var ct = (IComponentTemplate)Activator.CreateInstance(type);
                IsUnique = ct.IsUnique;
                ComponentType = ct.Type;
                if (ct is ITypeMeta metaOverride)
                {
                    _meta = metaOverride;
                }
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

