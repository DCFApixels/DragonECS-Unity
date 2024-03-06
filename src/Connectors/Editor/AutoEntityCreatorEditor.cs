#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(AutoEntityCreator))]
    [CanEditMultipleObjects]
    public class AutoEntityCreatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Autoset"))
            {
                foreach (var tr in targets)
                {
                    AutoEntityCreator creator = (AutoEntityCreator)tr;
                    creator.Autoset_Editor();
                    EditorUtility.SetDirty(creator);
                }
            }
        }
    }
}
#endif