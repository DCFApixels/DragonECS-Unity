using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [DebugColor(DebugColor.Cyan)]
    public struct UnityGameObject
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
        public static ent NewEntityWithGameObject(this IEcsWorld self, string name = "Entity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            ent result = self.NewEntity();
            GameObject newGameObject = new GameObject(name);
            newGameObject.AddComponent<EcsEntity>()._entity = result;
            result.Write<UnityGameObject>() = new UnityGameObject(newGameObject);

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

    public class EcsEntity : MonoBehaviour
    {
        internal ent _entity;
        public ent entity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity;
        }
        public bool IsAlive
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entity.IsAlive();
        }
    }

#if UNITY_EDITOR

    namespace Editors
    {
        using UnityEditor;
        [CustomEditor(typeof(EcsEntity))]
        public class EcsEntityEditor : Editor
        {
            private EcsEntity Target => (EcsEntity)target;
            public override void OnInspectorGUI()
            {
                EditorGUILayout.IntField(Target._entity.id);
                GUILayout.Label(Target._entity.ToString());
            }
        }
    }
#endif
}
