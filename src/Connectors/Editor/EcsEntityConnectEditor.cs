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
            EcsEntityConnect[] targets = new EcsEntityConnect[this.targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = (EcsEntityConnect)this.targets[i];
            }
            DrawEntityInfo(targets);

            DrawTemplates();

            DrawControlButtons(targets);
            DrawComponents(targets);
        }

        private void DrawEntityInfo(EcsEntityConnect[] targets)
        {
            bool isConnected = Target.Entity.TryUnpackForUnityEditor(out int id, out short gen, out short worldID, out EcsWorld world);
            EcsGUI.EntityStatus status = IsMultipleTargets ? EcsGUI.EntityStatus.Undefined : isConnected ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive;
            EcsGUI.Layout.EntityBarForAlive(status, id, gen, worldID);
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

        private void DrawControlButtons(EcsEntityConnect[] targets)
        {
            float height = EcsGUI.EntityBarHeight;
            Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, height);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
            rect = rect.AddPadding(2f, 0f);
            var (_, buttonRect) = rect.HorizontalSliceRight(height);
            if (EcsGUI.AutosetCascadeButton(buttonRect))
            {
                foreach (var target in targets)
                {
                    target.AutosetCascade_Editor();
                }
            }
            buttonRect = RectUtility.Move(buttonRect, -height, 0);
            if (EcsGUI.AutosetButton(buttonRect))
            {
                foreach (var target in targets)
                {
                    target.Autoset_Editor();
                }
            }
            using (EcsGUI.SetEnable(Application.isPlaying))
            {
                buttonRect = buttonRect.Move(-height, 0);
                if (EcsGUI.DelEntityButton(buttonRect))
                {
                    foreach (var target in targets)
                    {
                        target.DeleteEntity_Editor();
                    }
                }
                buttonRect = buttonRect.Move(-height, 0);
                if (EcsGUI.UnlinkButton(buttonRect))
                {
                    foreach (var target in targets)
                    {
                        target.UnlinkEntity_Editor();
                    }
                }
            }
        }

        private void DrawComponents(EcsEntityConnect[] targets)
        {
            if (IsMultipleTargets)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].IsConnected == true)
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
                    EcsGUI.Layout.DrawRuntimeComponents(entityID, world);
                }
            }
        }
    }
}
#endif