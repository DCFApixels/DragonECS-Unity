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
    using UnityEditor;

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

            DrawConnectStatus(targets);
            DrawEntityInfo();

            DrawTemplates();

            DrawButtons();
            DrawComponents(targets);
        }
        private void DrawTop()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
        private void DrawConnectStatus(EcsEntityConnect[] targets)
        {
            bool isConnected = Target.IsConected;
            for (int i = 0; i < targets.Length; i++)
            {
                if (isConnected != targets[i].IsConected)
                {
                    isConnected = !Target.IsConected;
                    break;
                }
            }
            if (isConnected == Target.IsConected)
            {
                EcsGUI.Layout.DrawConnectStatus(Target.IsConected);
            }
            else
            {
                EcsGUI.Layout.DrawUndeterminedConnectStatus();
            }
        }

        private void DrawEntityInfo()
        {
            GUILayout.Label(string.Empty);
            Rect entityRect = GUILayoutUtility.GetLastRect();
            Rect idRect = entityRect;
            idRect.xMax -= idRect.width / 2f;
            Rect genRect = entityRect;
            genRect.xMin = idRect.xMax;
            genRect.xMax -= genRect.width / 2f;
            Rect worldRect = genRect;
            worldRect.x += worldRect.width;

            if (IsMultipleTargets == false && Target.Entity.TryUnpack(out int id, out short gen, out short world))
            {
                EditorGUI.IntField(idRect, id);
                EditorGUI.IntField(genRect, gen);
                EditorGUI.IntField(worldRect, world);
            }
            else
            {
                GUI.enabled = false;
                EditorGUI.TextField(idRect, "Entity ID");
                EditorGUI.TextField(genRect, "Gen");
                EditorGUI.TextField(worldRect, "World ID");
                GUI.enabled = true;
            }
        }

        private void DrawTemplates()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
                enterChildren = false;
            }
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Autoset Templates"))
            {
                Target.SetTemplates_Editor(Target.GetComponents<MonoEntityTemplate>());
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Autoset Templates Cascade"))
            {
                foreach (var item in Target.GetComponentsInChildren<EcsEntityConnect>())
                {
                    item.SetTemplates_Editor(item.GetComponents<MonoEntityTemplate>());
                    EditorUtility.SetDirty(item);
                }
            }
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
                        break;
                    }
                }
            }
            if (Target.IsConected)
            {
                EcsGUI.Layout.DrawComponents(Target.Entity);
            }
        }
    }
}
#endif