using System.Runtime.CompilerServices;
using UnityEngine;
using DCFApixels.DragonECS.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DCFApixels.DragonECS
{
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.COMPONENTS_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "This component is automatically added if an entity is connected to one of the EcsEntityConnect. It also contains a reference to the connected EcsEntityConnect.")]
    [MetaID("14AC6B239201C6A60337AF3384D237E7")]
    [MetaTags(MetaTags.ENGINE_MEMBER)]
    [System.Serializable]
    public readonly struct GameObjectConnect : IEcsComponent, IEcsComponentLifecycle<GameObjectConnect>
    {
        public readonly EcsEntityConnect Connect;
        public bool IsConnected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Connect != null; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GameObjectConnect(EcsEntityConnect connect)
        {
            Connect = connect;
        }

        void IEcsComponentLifecycle<GameObjectConnect>.Enable(ref GameObjectConnect component)
        {
            component = default;
        }
        void IEcsComponentLifecycle<GameObjectConnect>.Disable(ref GameObjectConnect component)
        {
            if (component.Connect != null)
            {
                component.Connect.Disconnect();
            }
            component = default;
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
    internal static class GameObjectIconConsts
    {
        public const int RAW_LABEL_ICON_LAST = (int)GameObjectIcon.Label_Purple;
    }

    public static class GameObjectRefExt
    {
        public static entlong NewEntityWithGameObject(this EcsWorld self, string name = "Entity", GameObjectIcon icon = GameObjectIcon.NONE)
        {
            entlong result = self.NewEntityLong();
            GameObject newGameObject = new GameObject(name);
            newGameObject.AddComponent<EcsEntityConnect>().ConnectWith(result, false);
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
}
