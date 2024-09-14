#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(AutoEntityCreator))]
    [CanEditMultipleObjects]
    internal class AutoEntityCreatorEditor : ExtendedEditor<AutoEntityCreator>
    {
        protected override void DrawCustom()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
            if (EcsGUI.Changed)
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
            rect = rect.AddPadding(2f, 0f);
            var (left, autosetCascadeRect) = rect.HorizontalSliceRight(height);
            var (_, autosetRect) = rect.HorizontalSliceRight(height);

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