using System;
using System.Reflection;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
	public static class InternalInstantiationUtility
	{
		public static Func<object, object> GetCloneMethod(Type type)
		{
			var cloneMethod = GetClonableMethodInfo(type);
			if (cloneMethod != null)
			{
				return (object v) => { return cloneMethod.Invoke(v, Array.Empty<object>()); };
			}

			if (type.IsValueType)
			{
				return (object v) => { return v; };
			}
			else
			{
				return (object v) =>
				{
					var json = JsonUtility.ToJson(v);
					return JsonUtility.FromJson(json, type);
				};
			}

		}
		public static bool TryFindDefaultOrEmptyField(Type type, out FieldInfo field, out bool nameIsEmpty)
		{
			field = type.GetField("Default", BindingFlags.Static | BindingFlags.Public);
			if (field != null && field.FieldType == type)
			{
				nameIsEmpty = false;
				return true;
			}
			field = type.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
			if (field != null && field.FieldType == type)
			{
				nameIsEmpty = true;
				return true;
			}
			nameIsEmpty = false;
			field = null;
			return false;
		}

		private static MethodInfo GetClonableMethodInfo(Type type)
		{
			if (!typeof(ICloneable).IsAssignableFrom(type))
			{
				return null;
			}

			var interfaceMethod = typeof(ICloneable).GetMethod("Clone");
			var map = type.GetInterfaceMap(typeof(ICloneable));
			for (int i = 0; i < map.InterfaceMethods.Length; i++)
			{
				if (map.InterfaceMethods[i] == interfaceMethod)
				{
					return map.TargetMethods[i];
				}
			}

			return null;
		}
	}
	public static class InternalInstantiationUtility<T>
	{
		private readonly static T _defaultValue;
		private readonly static Func<T, T> _cloneDefaultValueMethod;
		private readonly static CreateDefaultInstanceMethod _createDefaultInstanceMethod;
		private readonly static bool _isValueType;

		private static Func<T, T> _cloneMethodCache;
		public enum CreateDefaultInstanceMethod
		{
			None,
			SetDefaultValue,
			CloneDefaultValue,
			DefaultConstructor,
		}
		public static T DefaultValue
		{
			get { return _defaultValue; }
		}

		static InternalInstantiationUtility()
		{
			var type = typeof(T);
			_isValueType = type.IsValueType;

			if (type.IsClass && type.IsAbstract == false && type.GetConstructor(Type.EmptyTypes) != null)
			{
				_createDefaultInstanceMethod = CreateDefaultInstanceMethod.DefaultConstructor;
			}

			if (InternalInstantiationUtility.TryFindDefaultOrEmptyField(type, out var defaultField, out var nameIsEmpty))
			{
				//_hasDefaultValue = true;

				_defaultValue = (T)defaultField.GetValue(null);
				_createDefaultInstanceMethod = CreateDefaultInstanceMethod.SetDefaultValue;

				if (_defaultValue is ICloneable cloneable)
				{
					_cloneDefaultValueMethod = (T v) => { return (T)((ICloneable)v).Clone(); };
					_createDefaultInstanceMethod = CreateDefaultInstanceMethod.CloneDefaultValue;
				}
				else
				{
					if(_isValueType == false && nameIsEmpty == false)
					{
						_cloneDefaultValueMethod = (T v) => 
						{
							var json = JsonUtility.ToJson(v);
							return JsonUtility.FromJson<T>(json);
						};
						_createDefaultInstanceMethod = CreateDefaultInstanceMethod.CloneDefaultValue;
					}
				}
			}

			if (_createDefaultInstanceMethod == CreateDefaultInstanceMethod.None && _isValueType)
			{
				_createDefaultInstanceMethod = CreateDefaultInstanceMethod.DefaultConstructor;
			}

			if(_cloneDefaultValueMethod != null)
			{
				_cloneMethodCache = _cloneDefaultValueMethod;
			}
		}

		public static Func<T, T> GetCloneMethod()
		{
			if (_cloneMethodCache == null)
			{
				var type = typeof(T);
				var cloneMethod = InternalInstantiationUtility.GetCloneMethod(type);
				_cloneMethodCache = (T v) => { return (T)cloneMethod(v); };
			}
			return _cloneMethodCache;
		}
		public static T CreateDefaultInstance()
		{
			switch (_createDefaultInstanceMethod)
			{
				default:
				case CreateDefaultInstanceMethod.None:
					{
						return default;
					}
				case CreateDefaultInstanceMethod.SetDefaultValue:
					{
						return _defaultValue;
					}
				case CreateDefaultInstanceMethod.CloneDefaultValue:
					{
						return _cloneDefaultValueMethod(_defaultValue);
					}
				case CreateDefaultInstanceMethod.DefaultConstructor:
					{
						if (_isValueType)
						{
							return default;
						}
						return Activator.CreateInstance<T>();
					}
			}
		}

		public static T CloneInstance(T v)
		{
			return GetCloneMethod()(v);
		}
	}
}