using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public interface ITemplateComponent
    {
        public void Add(EcsWorld w, int e);
    }
    public interface ITemplateComponentName : ITemplateComponent
    {
        public string Name { get; }
    }
    public interface ITemplateComponentGizmos
    {
        public void OnGizmos(Transform transform, Mode mode);
        public enum Mode
        {
            Always,
            Selected
        }
    }
    public interface ITemplateComponentOnValidate
    {
        public void OnValidate(GameObject gameObject);
    }

    [Serializable]
    public abstract class TemplateComponentInitializerBase
    {
        public virtual string Name => string.Empty;
        public virtual Color Color => Color.black;
        public virtual string Description => string.Empty;
        public abstract Type Type { get; }

        internal abstract object ComponentRef { get; }

        #region Get meta
        internal static Color GetColor(Type type)
        {
            //var atr = type.GetCustomAttribute<DebugColorAttribute>();
            //if (atr == null) return Color.black;
            //return atr.GetUnityColor();
            return EcsDebugUtility.GetColor(type).ToUnityColor();
        }
        internal static string GetName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                    friendlyName = friendlyName.Remove(iBacktick);

                friendlyName += "/" + friendlyName;
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }
            return friendlyName;
        }

        internal static string GetDescription(Type type)
        {
            var atr = type.GetCustomAttribute<DebugDescriptionAttribute>();
            if (atr == null) return string.Empty;
            return atr.description;
        }
        #endregion
    }
    [Serializable]
    public abstract class TemplateComponentInitializer<T> : TemplateComponentInitializerBase, ITemplateComponentName, ITemplateComponentGizmos, ITemplateComponentOnValidate
    {
        private static string _autoname = GetName(typeof(T));
        private static Color _autoColor = GetColor(typeof(T));
        private static string _autoDescription = GetDescription(typeof(T));

        [SerializeField]
        protected T component;

        #region Properties
        public override string Name => _autoname;
        public override Color Color => _autoColor;
        public override string Description => _autoDescription;
        public sealed override Type Type => typeof(T);

        internal T Component => component;
        internal override object ComponentRef => component;
        #endregion

        public abstract void Add(EcsWorld w, int e);
        public virtual void OnGizmos(Transform transform, ITemplateComponentGizmos.Mode mode) { }
        public virtual void OnValidate(GameObject gameObject) { }
    }

    internal static class ITemplateBrowsableExt
    {
        private static MethodInfo memberwiseCloneMethdo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static ITemplateComponent Clone(this ITemplateComponent obj)
        {
            return (ITemplateComponent)memberwiseCloneMethdo.Invoke(obj, null);
        }
    }

#if UNITY_EDITOR
    namespace Editors
    {
        internal static class TemplateBrowsableTypeCache
        {
            private static Type[] _types;
            private static ITemplateComponent[] _dummies;
            internal static ReadOnlySpan<Type> Types => _types;
            internal static ReadOnlySpan<ITemplateComponent> Dummies => _dummies;

            static TemplateBrowsableTypeCache()
            {
                List<Type> types = new List<Type>();
                Type interfaceType = typeof(ITemplateComponent);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var targetTypes = assembly.GetTypes().Where(type => !type.IsGenericType && (type.IsValueType || type.IsClass) && type.GetCustomAttribute<SerializableAttribute>() != null);

                    types.AddRange(targetTypes.Where(type => interfaceType.IsAssignableFrom(type)));

                    foreach (var t in targetTypes)
                    {
                        if (t.IsSubclassOf(typeof(TemplateComponentInitializer<>)))
                        {
                            if (t.HasAttribute<SerializableAttribute>())
                                types.Add(t);
                        }
                    }
                }
                _types = types.ToArray();
                _dummies = new ITemplateComponent[_types.Length];

                for (int i = 0; i < _types.Length; i++)
                    _dummies[i] = (ITemplateComponent)Activator.CreateInstance(_types[i]);
            }
        }
    }
#endif
}
