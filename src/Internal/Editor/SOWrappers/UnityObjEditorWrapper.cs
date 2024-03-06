#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [Serializable]
    internal class UnityObjEditorWrapper : WrapperBase<UnityObjEditorWrapper>
    {
        [SerializeField]
        public UnityEngine.Object data;

        public override object Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return data; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityObjEditorWrapper Take(UnityEngine.Object data)
        {
            var result = Take();
            result.data = data;
            result.SO.Update();
            return result;
        }
    }
}
#endif
