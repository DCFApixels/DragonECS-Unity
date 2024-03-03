using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class EcsEntityConnect : MonoBehaviour
    {
        private sealed class Aspect : EcsAspect
        {
            public readonly EcsPool<UnityGameObject> unityGameObjects;
            public Aspect(Builder b)
            {
                unityGameObjects = b.Include<UnityGameObject>();
            }
        }

        private entlong _entity;
        private EcsWorld _world;

        [SerializeField]
        private ScriptableEntityTemplate[] _scriptableTemplates;
        [SerializeField]
        private MonoEntityTemplate[] _monoTemplates;

        internal void SetTemplates_Editor(MonoEntityTemplate[] tempaltes)
        {
            _monoTemplates = tempaltes;
        }

        #region Properties
        public entlong Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity;
        }
        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _world;
        }
        public bool IsConected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity.IsAlive;
        }
        #endregion

        public void ConnectWith(entlong entity, bool applyTemplates = false)
        {
            if (_entity.TryGetID(out int oldE) && _world != null)
            {
                var s = _world.GetAspect<Aspect>();
                s.unityGameObjects.Del(oldE);
            }
            _world = null;

            if (entity.TryGetID(out int newE))
            {
                _entity = entity;
                _world = _entity.World;
                var s = _world.GetAspect<Aspect>();
                if (!s.unityGameObjects.Has(newE)) s.unityGameObjects.Add(newE) = new UnityGameObject(gameObject);

                if (applyTemplates)
                    ApplyTemplates();
            }
            else
            {
                _entity = entlong.NULL;
            }
        }
        public void ApplyTemplates()
        {
            ApplyTemplatesFor(_entity.ID);
        }
        public void ApplyTemplatesFor(int entityID)
        {
            foreach (var t in _scriptableTemplates)
            {
                t.Apply(_world.id, entityID);
            }
            foreach (var t in _monoTemplates)
            {
                t.Apply(_world.id, entityID);
            }
        }
    }
}


#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using DCFApixels.DragonECS.Unity.Internal;
    using UnityEditor;
    using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

    [CustomEditor(typeof(EcsEntityConnect))]
    [CanEditMultipleObjects]
    public class EcsEntityEditor : Editor
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
            DrawTop();

            DrawEntityInfo(targets);

            DrawTemplates();

            DrawButtons();
            DrawComponents(targets);
        }
        private void DrawTop()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private void DrawEntityInfo(EcsEntityConnect[] targets)
        {
            float width = EditorGUIUtility.currentViewWidth;
            float height = EditorGUIUtility.singleLineHeight;
            Rect entityRect = GUILayoutUtility.GetRect(width, height + 3f);

            var (entityInfoRect, statusRect) = RectUtility.VerticalSliceBottom(entityRect, 3f);

            var (idRect, genWorldRect) = RectUtility.HorizontalSliceLerp(entityInfoRect, 0.5f);
            var (genRect, worldRect) = RectUtility.HorizontalSliceLerp(genWorldRect, 0.5f);



            bool isConnected = Target.Entity.TryUnpack(out int id, out short gen, out short world);
           

            if (IsMultipleTargets == false && isConnected)
            {
                Color statusColor = EcsGUI.GreenColor;
                statusColor.a = 0.32f;
                EditorGUI.DrawRect(statusRect, statusColor);
                EditorGUI.IntField(idRect, id);
                EditorGUI.IntField(genRect, gen);
                EditorGUI.IntField(worldRect, world);
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    Color statusColor = IsMultipleTargets ? EcsGUI.GrayColor : EcsGUI.RedColor;
                    statusColor.a = 0.32f;
                    EditorGUI.DrawRect(statusRect, statusColor);
                    EditorGUI.TextField(idRect, "Entity ID");
                    EditorGUI.TextField(genRect, "Gen");
                    EditorGUI.TextField(worldRect, "World ID");
                }
            }
        }

        private void DrawTemplates()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                EditorGUILayout.PropertyField(iterator, true);
                enterChildren = false;
            }
        }

        private void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Autoset"))
            {
                Target.SetTemplates_Editor(Target.GetComponents<MonoEntityTemplate>());
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Autoset Cascade"))
            {
                foreach (var item in Target.GetComponentsInChildren<EcsEntityConnect>())
                {
                    item.SetTemplates_Editor(item.GetComponents<MonoEntityTemplate>());
                    EditorUtility.SetDirty(item);
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
                EcsGUI.Layout.DrawComponents(entityID, world);
            }
        }
    }
}
#endif