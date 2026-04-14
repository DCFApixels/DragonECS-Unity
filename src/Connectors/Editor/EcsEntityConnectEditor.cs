#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsEntityConnect), true)]
    [CanEditMultipleObjects]
    internal class EcsEntityConnectEditor : ExtendedEditor<EcsEntityConnect>
    {
        protected override void DrawCustom()
        {
            DrawEntityInfo();

            DrawTemplates();

            DrawControlButtons();
            DrawComponents();
        }

        private void DrawEntityInfo()
        {
            //bool isConnected = Target.Entity.TryUnpackForUnityEditor(out int id, out short gen, out short worldID, out EcsWorld world);
            //EcsGUI.EntityStatus status = IsMultipleTargets ? EcsGUI.EntityStatus.Undefined : isConnected ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive;
            //EcsGUI.Layout.EntityField(status, id, gen, worldID);
            DragonGUI.Layout.EntityField(Target.Entity);
        }

        private void DrawTemplates()
        {
            using (DragonGUI.CheckChanged())
            {
                var iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }

                if (DragonGUI.Changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private void DrawControlButtons()
        {
            float height = DragonGUI.EntityBarHeight;
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, height);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
            rect = rect.AddPadding(2f, 0f);
            var (_, buttonRect) = rect.HorizontalSliceRight(height);
            if (DragonGUI.AutosetCascadeButton(buttonRect))
            {
                foreach (var target in Targets)
                {
                    target.AutosetCascade_Editor();
                }
            }
            buttonRect = RectUtility.Move(buttonRect, -height, 0);
            if (DragonGUI.AutosetButton(buttonRect))
            {
                foreach (var target in Targets)
                {
                    target.Autoset_Editor();
                }
            }
            using (DragonGUI.SetEnable(Application.isPlaying))
            {
                buttonRect = buttonRect.Move(-height, 0);
                if (DragonGUI.DelEntityButton(buttonRect))
                {
                    foreach (var target in Targets)
                    {
                        target.DeleteEntity_Editor();
                    }
                }
                buttonRect = buttonRect.Move(-height, 0);
                if (DragonGUI.UnlinkButton(buttonRect))
                {
                    foreach (var target in Targets)
                    {
                        target.UnlinkEntity_Editor();
                    }
                }
            }
        }

        private void DrawComponents()
        {
            if (IsMultipleTargets)
            {
                for (int i = 0; i < Targets.Length; i++)
                {
                    if (Targets[i].IsConnected == true)
                    {
                        EditorGUILayout.HelpBox("Multiple component editing is not available.", MessageType.Warning);
                        return;
                    }
                }
            }
            if (Target.Entity.TryUnpackForUnityEditor(out int entityID, out short gen, out short worldID, out EcsWorld world))
            {
                if (world.IsNullOrDetroyed() == false)
                {
                    DragonGUI.Layout.DrawRuntimeComponents(entityID, world, true, true);
                }
            }
        }
    }
}
#endif