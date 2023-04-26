using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using UnityEditor.ShortcutManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [DebugColor(DebugColor.Cyan)]
    public struct UnityGameObject : IEcsComponent
    {
        public GameObject gameObject;
        public Transform transform;

        public string Name
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => gameObject.name;
        }

        public UnityGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            transform = gameObject.transform;
        }
    }

    public enum GameObjectIcon : byte
    {
        NONE,
        Label_Gray,
        Label_Blue,
        Label_Teal,
        Label_Green,
        Label_Yellow,
        Label_Orange,
        Label_Red,
        Label_Purple,
        Circle_Gray,
        Circle_Blue,
        Circle_Teal,
        Circle_Green,
        Circle_Yellow,
        Circle_Orange,
        Circle_Red,
        Circle_Purple,
        Diamond_Gray,
        Diamond_Blue,
        Diamond_Teal,
        Diamond_Green,
        Diamond_Yellow,
        Diamond_Orange,
        Diamond_Red,
        Diamond_Purple
    }
    public static class GameObjectIconConsts
    {
        public const int RAW_LABEL_ICON_LAST = (int)GameObjectIcon.Label_Purple;
    }

    public static class GameObjectRefExt
    {
        public static EcsEntity NewEntityWithGameObject(this EcsWorld self, string name = "EcsEntity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            EcsEntity result = self.NewEntity();
            GameObject newGameObject = new GameObject(name);
            newGameObject.AddComponent<EcsEntityConnect>().ConectWith(result);
          //  self.GetPool<UnityGameObject>().Add(result.id) = 
#if UNITY_EDITOR
            if (icon != GameObjectIcon.NONE)
            {
                string contentName;
                int number = (int)icon - 1;
                if (number < GameObjectIconConsts.RAW_LABEL_ICON_LAST)
                {
                    contentName = $"sv_label_{number}";
                }
                else
                {
                    number -= GameObjectIconConsts.RAW_LABEL_ICON_LAST;
                    contentName = $"sv_icon_dot{number}_pix16_gizmo";
                }
                GUIContent iconContent = EditorGUIUtility.IconContent(contentName);
                EditorGUIUtility.SetIconForObject(newGameObject, (Texture2D)iconContent.image);
            }
#endif

            return result;
        }
    }


    public class EcsEntityConnect : MonoBehaviour
    {
        private sealed class Query : EcsQuery
        {
            public readonly EcsPool<UnityGameObject> unityGameObjects;
            public Query(Builder b)
            {
                unityGameObjects = b.Include<UnityGameObject>();
            }
        }

        private EcsEntity _entity;
        private EcsWorld _world;

        #region Properties
        public EcsEntity Entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity;
        }
        public EcsWorld World
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _world;
        }
        public bool IsAlive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity.IsAlive;
        }
        #endregion

        public void ConectWith(EcsEntity entity)
        {
            int e = _entity.id;
            if (_world != null && _entity.IsNotNull)
            {
                var q = _world.Select<Query>();
                q.unityGameObjects.Del(e);
            }
            _world = null;

            _entity = entity;

            if (_entity.IsNotNull)
            {
                _world = _entity.GetWorld();
                var q = _world.Select<Query>();
                if (!q.unityGameObjects.Has(e)) q.unityGameObjects.Add(e) = new UnityGameObject(gameObject);
            }
        }
    }

#if UNITY_EDITOR

    namespace Editors
    {
        using UnityEditor;
        [CustomEditor(typeof(EcsEntityConnect))]
        public class EcsEntityEditor : Editor
        {
            private EcsEntityConnect Target => (EcsEntityConnect)target;
            public override void OnInspectorGUI()
            {
                EditorGUILayout.IntField(Target.Entity.id);
                GUILayout.Label(Target.Entity.ToString());
            }
        }
    }
#endif
}
