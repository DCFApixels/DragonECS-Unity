#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsRootUnity))]
    internal class EcsRootUnityEditor : ExtendedEditor<EcsRootUnity>
    {
        protected override void DrawCustom()
        {
            DrawStatus();
            DrawTemplates();
            DrawControlButtons();
        }


        private enum Status
        {
            Undefined,
            NotInitialized,
            Disable,
            Run,
        }
        private Status GetStatus(EcsRootUnity target)
        {
            Status status;
            if (target.IsInit)
            {
                status = Target.enabled ? Status.Run : Status.Disable;
            }
            else
            {
                status = Status.NotInitialized;
            }
            return status;
        }
        private void DrawStatus()
        {
            Status status = GetStatus(Target);

            if (IsMultipleTargets)
            {
                foreach (var target in Targets)
                {
                    if (status != GetStatus(target))
                    {
                        status = Status.Undefined;
                        break;
                    }
                }
            }

            Color color = default;
            string text = default;

            switch (status)
            {
                case Status.Undefined:
                    color = Color.gray;
                    text = "-";
                    break;
                case Status.NotInitialized:
                    color = Color.red;
                    text = "Not Initialized";
                    break;
                case Status.Disable:
                    color = Color.yellow;
                    text = "Disable";
                    break;
                case Status.Run:
                    color = Color.green;
                    text = "Run";
                    break;
            }

            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.DrawRect(rect, color.SetAlpha(0.2f));
            GUI.Label(rect, text);
        }

        private void DrawTemplates()
        {
            using (EcsGUI.CheckChanged())
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
            }
        }


        private void DrawControlButtons()
        {
            float height = EcsGUI.EntityBarHeight;
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, height);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
            rect = rect.AddPadding(2f, 0f);
            var (_, buttonRect) = rect.HorizontalSliceRight(height);
            if (EcsGUI.ValidateButton(buttonRect))
            {
                foreach (var target in Targets)
                {
                    target.Validate_Editor();
                }
            }
            buttonRect = RectUtility.Move(buttonRect, -height, 0);
            if (EcsGUI.AutosetButton(buttonRect))
            {
                foreach (var target in Targets)
                {
                    target.Autoset_Editor();
                }
            }
        }
    }
}
#endif