#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsEntityConnect))]
    [CanEditMultipleObjects]
    public class EcsEntityConnectEditor : Editor
    {
        private bool _isInit = false;
        private EcsEntityConnect Target => (EcsEntityConnect)target;
        private bool IsMultipleTargets => targets.Length > 1;

        private void Init()
        {
            if (_isInit)
            {
                return;
            }
            _isInit = true;
        }

        public override void OnInspectorGUI()
        {
            Init();
            EcsEntityConnect[] targets = new EcsEntityConnect[this.targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                targets[i] = (EcsEntityConnect)this.targets[i];
            }
            DrawEntityInfo(targets);

            DrawTemplates();

            DrawButtons();
            DrawComponents(targets);
        }

        private void DrawEntityInfo(EcsEntityConnect[] targets)
        {
            //TODO Отрефакторить
            float width = EditorGUIUtility.currentViewWidth;
            float height = EditorGUIUtility.singleLineHeight;
            Rect entityRect = GUILayoutUtility.GetRect(width, height + 3f);
            var (entityInfoRect, statusRect) = RectUtility.VerticalSliceBottom(entityRect, 3f);

            Color w = Color.gray;
            w.a = 0.6f;
            Color b = Color.black;
            b.a = 0.55f;
            EditorGUI.DrawRect(entityInfoRect, w);

            var (idRect, genWorldRect) = RectUtility.HorizontalSliceLerp(entityInfoRect, 0.5f);
            var (genRect, worldRect) = RectUtility.HorizontalSliceLerp(genWorldRect, 0.5f);

            idRect = RectUtility.AddPadding(idRect, 2, 1, 0, 0);
            genRect = RectUtility.AddPadding(genRect, 1, 1, 0, 0);
            worldRect = RectUtility.AddPadding(worldRect, 1, 2, 0, 0);
            EditorGUI.DrawRect(idRect, b);
            EditorGUI.DrawRect(genRect, b);
            EditorGUI.DrawRect(worldRect, b);

            bool isConnected = Target.Entity.TryUnpack(out int id, out short gen, out short world);

            GUIStyle style = new GUIStyle(EditorStyles.numberField);
            style.alignment = TextAnchor.MiddleCenter;
            style.font = EditorStyles.boldFont;
            if (IsMultipleTargets == false && isConnected)
            {
                Color statusColor = EcsGUI.GreenColor;
                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                EditorGUI.IntField(idRect, id, style);
                EditorGUI.IntField(genRect, gen, style);
                EditorGUI.IntField(worldRect, world, style);
            }
            else
            {
                Color statusColor = IsMultipleTargets ? new Color32(200, 200, 200, 255) : EcsGUI.RedColor;
                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(idRect, "Entity ID", style);
                    GUI.Label(genRect, "Generation", style);
                    GUI.Label(worldRect, "World ID", style);
                }
            }
        }

        private void DrawTemplates()
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
        }

        private void Autoset(EcsEntityConnect target)
        {
            var result = target.MonoTemplates.Where(o => o != null).Union(GetTemplatesFor(target.transform));

            target.SetTemplates_Editor(result.ToArray());
            EditorUtility.SetDirty(target);
        }
        private IEnumerable<MonoEntityTemplate> GetTemplatesFor(Transform parent)
        {
            IEnumerable<MonoEntityTemplate> result = parent.GetComponents<MonoEntityTemplate>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.TryGetComponent<EcsEntityConnect>(out _))
                {
                    return Enumerable.Empty<MonoEntityTemplate>();
                }
                result = result.Concat(GetTemplatesFor(child));
            }
            return result;
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Autoset"))
            {
                Autoset(Target);
            }
            if (GUILayout.Button("Autoset Cascade"))
            {
                foreach (var item in Target.GetComponentsInChildren<EcsEntityConnect>())
                {
                    Autoset(item);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawComponents(EcsEntityConnect[] targets)
        {
            if (IsMultipleTargets)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].IsConected == true)
                    {
                        EditorGUILayout.HelpBox("Multiple component editing is not available.", MessageType.Warning);
                        return;
                    }
                }
            }
            if (Target.Entity.TryUnpack(out int entityID, out EcsWorld world))
            {
                EcsGUI.Layout.DrawRuntimeComponents(entityID, world);
            }
        }
    }
}
#endif