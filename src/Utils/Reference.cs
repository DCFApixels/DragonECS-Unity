using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [System.Serializable]
    public struct Reference<T>
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR
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
        public T Value;
#endif

#if UNITY_EDITOR
        private static System.Collections.Generic.Dictionary<string, System.Type> _metaIDTypePairs;
        private static void InitRecoverCache()
        {
            if (_metaIDTypePairs == null) { return; }
            _metaIDTypePairs = new System.Collections.Generic.Dictionary<string, System.Type>();
        }
        private static T TryRecoverReference(string metaID)
        {
            InitRecoverCache();
            if (_metaIDTypePairs.TryGetValue(metaID, out System.Type type))
            {
                return (T)System.Activator.CreateInstance(type);
            }
            return default;
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_value == null)
            {
                int indexof = _json.IndexOf(',');
                _value = TryRecoverReference(_json.Substring(0, indexof));
                if (_value == null) { return; }
                JsonUtility.FromJsonOverwrite(_json.Substring(indexof + 1), _value);
                _json = null;
            }
        }
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _json = $"{_value.GetMeta().MetaID},{JsonUtility.ToJson(_value)}";
        }
#endif
    }
}
