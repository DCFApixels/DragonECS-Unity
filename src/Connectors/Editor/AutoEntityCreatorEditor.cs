#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(AutoEntityCreator))]
    [CanEditMultipleObjects]
    public class AutoEntityCreatorEditor : Editor
    {
        private AutoEntityCreator Target => (AutoEntityCreator)target;

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
            DrawControlButtons();
        }


        private void DrawControlButtons()
        {
            float height = EcsGUI.EntityBarHeight;
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, height);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
            rect = RectUtility.AddPadding(rect, 2f, 0f);
            var (left, autosetCascadeRect) = RectUtility.HorizontalSliceRight(rect, height);
            var (_, autosetRect) = RectUtility.HorizontalSliceRight(left, height);

            if (EcsGUI.AutosetCascadeButton(autosetCascadeRect))
            {
                foreach (AutoEntityCreator target in targets)
                {
                    target.AutosetCascade_Editor();
                }
            }

            if (EcsGUI.AutosetButton(autosetRect))
            {
                foreach (AutoEntityCreator target in targets)
                {
                    target.Autoset_Editor();
                }
            }
        }
    }
}
#endif