#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
            bool isConnected = Target.Entity.TryUnpack(out int id, out short gen, out EcsWorld world);
            EcsGUI.EntityStatus status = IsMultipleTargets ? EcsGUI.EntityStatus.Undefined : isConnected ? EcsGUI.EntityStatus.Alive : EcsGUI.EntityStatus.NotAlive;

            float width = EditorGUIUtility.currentViewWidth;
            float height = EditorGUIUtility.singleLineHeight;
            Rect rect = GUILayoutUtility.GetRect(width, height + 3f);
            var (left, delEntityButtonRect) = RectUtility.HorizontalSliceRight(rect, height + 3);
            var (entityRect, unlinkButtonRect) = RectUtility.HorizontalSliceRight(left, height + 3);

            using (new EditorGUI.DisabledScope(status != EcsGUI.EntityStatus.Alive))
            {
                if (EcsGUI.UnlinkButton(unlinkButtonRect))
                {
                    Target.ConnectWith(entlong.NULL);
                }
                if (EcsGUI.DelEntityButton(delEntityButtonRect))
                {
                    world.DelEntity(id);
                    Target.ConnectWith(entlong.NULL);
                }
            }

            EcsGUI.DrawEntity(entityRect, status, id, gen, world.id);
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