using System;
using System.Reflection;

namespace DCFApixels.DragonECS.Unity.Internal
{
	public static class InternalInstantiationUtility
	{
		public static Func<object, object> GetCloneMethod(Type type)
		{
			var cloneMethod = GetCloneMethodInfo(type);
			if(cloneMethod != null)
			{
				return (object v) => { return cloneMethod.Invoke(v, Array.Empty<object>()); };
			}
			return (object v) => { return v; };
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

		private static MethodInfo GetCloneMethodInfo(Type type)
		{
			// Проверяем, реализует ли тип ICloneable
			if (!typeof(ICloneable).IsAssignableFrom(type))
				return null;

			// Получаем метод интерфейса
			var interfaceMethod = typeof(ICloneable).GetMethod("Clone");

			// Получаем карту интерфейса для данного типа
			var map = type.GetInterfaceMap(typeof(ICloneable));

			// Ищем соответствие
			for (int i = 0; i < map.InterfaceMethods.Length; i++)
			{
				if (map.InterfaceMethods[i] == interfaceMethod)
				{
					return map.TargetMethods[i]; // реальный метод в типе
				}
			}


			var memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
			return memberwiseClone;
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
						var memberwiseCloneMethdo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
						_cloneDefaultValueMethod = (T v) => { return (T)memberwiseCloneMethdo.Invoke(v, Array.Empty<object>()); };
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