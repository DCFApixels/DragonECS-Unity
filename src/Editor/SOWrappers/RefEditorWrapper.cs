#if UNITY_EDITOR
using System;
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
            get { return data; }
        }
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
