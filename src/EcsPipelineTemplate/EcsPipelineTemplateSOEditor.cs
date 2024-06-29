#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsPipelineTemplateSO))]
    internal class EcsPipelineTemplateSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                Validate();
            }
            if (GUILayout.Button("Validate"))
            {
                Validate();
            }
        }

        private void Validate()
        {
            foreach (var target in targets)
            {
                ((EcsPipelineTemplateSO)target).Validate();
            }
        }
    }
}
#endif