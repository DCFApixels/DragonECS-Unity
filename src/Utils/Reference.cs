//#define DISABLE_SERIALIZE_REFERENCE_RECOVERY
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
    [InitializeOnLoad]
    internal static class RecoveryReferenceUtility
    {
        internal static bool _recompileAfterInitializationScope = false;
        private static Dictionary<string, Type> _metaIDTypePairs;

        static RecoveryReferenceUtility()
        {
            _recompileAfterInitializationScope = true;
            EditorApplication.update += BeforeCompilation;

        }
        private static void BeforeCompilation()
        {
            _recompileAfterInitializationScope = false;
            EditorApplication.update -= BeforeCompilation;
        }


        private static void InitRecoverCache()
        {
            if (_metaIDTypePairs != null) { return; }
            _metaIDTypePairs = new Dictionary<string, Type>();

            List<Type> types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.TryGetAttribute(out MetaIDAttribute atr))
                    {
                        _metaIDTypePairs.Add(atr.ID, type);
                    }
                }
            }
        }
        internal static T TryRecoverReference<T>(string metaID)
        {
            InitRecoverCache();
            if (_metaIDTypePairs.TryGetValue(metaID, out Type type))
            {
                return (T)Activator.CreateInstance(type);
            }
            return default;
        }
    }
#endif

    [Serializable]
    public struct Reference<T>
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
        : ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
        [SerializeReference]
        private T _value;
        [SerializeField]
        private string _json;
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _value; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _value = value; }
        }
#else
        [SerializeField]
        public T Value;
#endif
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Value == null; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(Reference<T> a) { return a.Value; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Reference<T>(T a) { return new Reference<T>() { Value = a }; }

#if UNITY_EDITOR && !DISABLE_SERIALIZE_REFERENCE_RECOVERY
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_value == null && RecoveryReferenceUtility._recompileAfterInitializationScope && string.IsNullOrEmpty(_json) == false)
            {
                int indexof = _json.IndexOf(',');
                _value = RecoveryReferenceUtility.TryRecoverReference<T>(_json.Substring(0, indexof));
                if (_value == null) { return; }
                JsonUtility.FromJsonOverwrite(_json.Substring(indexof + 1), _value);
            }
            _json = null;
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_value != null && EditorApplication.isPlaying == false)
            {
                _json = $"{_value.GetMeta().MetaID},{JsonUtility.ToJson(_value)}";
            }
        }
#endif
    }
}