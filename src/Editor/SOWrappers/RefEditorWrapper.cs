 #if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [Serializable]
    internal class RefEditorWrapper : WrapperBase<RefEditorWrapper>
    {
        [SerializeReference]
        public object data;

        public override object Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return data; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RefEditorWrapper Take(object data)
        {
            var result = Take();
            result.data = data;
            result.SO.Update();
            return result;
        }
    }
}

[Serializable]
public class EmptyDummy
{
    public static readonly EmptyDummy Instance = new EmptyDummy();
    private EmptyDummy() { }
}
#endif
