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
            EditorGUI.BeginChangeCheck();
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }


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