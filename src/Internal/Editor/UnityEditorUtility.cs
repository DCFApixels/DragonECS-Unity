using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static partial class UnityEditorUtility
    {
        #region TransformFieldName
        public static string TransformToUpperName(string name)
        {
            if (name.Length <= 0)
            {
                return name;
            }
            StringBuilder b = new StringBuilder();
            bool nextWorld = true;
            bool prewIsUpper = false;


            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetter(c) == false)
                {
                    nextWorld = true;
                    prewIsUpper = false;
                    continue;
                }

                bool isUpper = char.IsUpper(c);
                if (isUpper)
                {
                    if (nextWorld == false && prewIsUpper == false)
                    {
                        b.Append('_');
                    }
                }
                b.Append(char.ToUpper(c));
                nextWorld = false;
                prewIsUpper = isUpper;
            }

            return b.ToString();
        }

        public static string TransformFieldName(string name)
        {
            if (name.Length <= 0)
            {
                return name;
            }
            StringBuilder b = new StringBuilder();
            bool nextWorld = true;
            bool prewIsUpper = false;


            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetter(c) == false)
                {
                    nextWorld = true;
                    prewIsUpper = false;
                    continue;
                }

                bool isUpper = char.IsUpper(c);
                if (isUpper)
                {
                    if (nextWorld == false && prewIsUpper == false)
                    {
                        b.Append(' ');
                        nextWorld = true;
                    }
                }

                if (nextWorld)
                {
                    b.Append(char.ToUpper(c));
                }
                else
                {
                    b.Append(c);
                }
                nextWorld = false;
                prewIsUpper = isUpper;
            }

            return b.ToString();
        }
        #endregion

    }
}


#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    [InitializeOnLoad]
    internal static partial class UnityEditorUtility
    {
        static UnityEditorUtility()
        {
            colorBoxeStyles = new SparseArray<GUIStyle>();
        }
        private static SparseArray<GUIStyle> colorBoxeStyles = new SparseArray<GUIStyle>();
        private static GUIContent _singletonIconContent = null;
        private static GUIContent _singletonContent = null;


        #region Label
        public static GUIContent GetLabelTemp()
        {
            if (_singletonContent == null)
            {
                _singletonContent = new GUIContent();
            }
            return _singletonContent;
        }
        public static GUIContent GetLabel(string name, string tooltip = null)
        {
            if (_singletonContent == null)
            {
                _singletonContent = new GUIContent();
            }
            _singletonContent.text = name;
            _singletonContent.image = null;
            _singletonContent.tooltip = tooltip;
            return _singletonContent;
        }
        public static GUIContent GetLabel(Texture image, string tooltip = null)
        {
            if (_singletonIconContent == null)
            {
                _singletonIconContent = new GUIContent();
            }
            _singletonIconContent.text = string.Empty;
            _singletonIconContent.image = image;
            _singletonIconContent.tooltip = tooltip;
            return _singletonIconContent;
        }
        #endregion

        #region GetStyle
        public static GUIStyle GetStyle(Color color, float alphaMultiplier)
        {
            color.a *= alphaMultiplier;
            return GetStyle(color);
        }
        public static GUIStyle GetStyle(Color32 color32)
        {
            int colorCode = new Color32Union(color32).colorCode;
            if (colorBoxeStyles.TryGetValue(colorCode, out GUIStyle style))
            {
                if (style == null || style.normal.background == null)
                {
                    style = CreateStyle(color32, colorCode);
                    colorBoxeStyles[colorCode] = style;
                }
                return style;
            }

            style = CreateStyle(color32, colorCode);
            colorBoxeStyles.Add(colorCode, style);
            return style;
        }
        private static GUIStyle CreateStyle(Color32 color32, int colorCode)
        {
            GUIStyle result = new GUIStyle(GUI.skin.box);
            Color componentColor = color32;
            Texture2D texture2D = CreateTexture(2, 2, componentColor);
            result.hover.background = texture2D;
            result.hover.scaledBackgrounds = Array.Empty<Texture2D>();
            result.focused.background = texture2D;
            result.focused.scaledBackgrounds = Array.Empty<Texture2D>();
            result.active.background = texture2D;
            result.active.scaledBackgrounds = Array.Empty<Texture2D>();
            result.normal.background = texture2D;
            result.normal.scaledBackgrounds = Array.Empty<Texture2D>();
            return result;
        }
        private static Texture2D CreateTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; ++i)
                pixels[i] = color;

            var result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
        #endregion

        #region Utils
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 4)]
        private readonly ref struct Color32Union
        {
            [FieldOffset(0)]
            public readonly int colorCode;
            [FieldOffset(0)]
            public readonly byte r;
            [FieldOffset(1)]
            public readonly byte g;
            [FieldOffset(2)]
            public readonly byte b;
            [FieldOffset(3)]
            public readonly byte a;
            public Color32Union(byte r, byte g, byte b, byte a) : this()
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
            public Color32Union(Color32 color) : this()
            {
                r = color.r;
                g = color.g;
                b = color.b;
                a = color.a;
            }
        }
        #endregion
    }

    internal static class RuntimeComponentsUtility
    {
        public struct WorldData
        {
            public GenericMenu addComponentGenericMenu;
            public int poolsCount;
            public WorldData(GenericMenu addComponentGenericMenu, int poolsCount)
            {
                this.addComponentGenericMenu = addComponentGenericMenu;
                this.poolsCount = poolsCount;
            }
        }
        //world id
        private static Dictionary<EcsWorld, WorldData> _worldDatas = new Dictionary<EcsWorld, WorldData>();

        public static GenericMenu GetAddComponentGenericMenu(EcsWorld world)
        {
            if (_worldDatas.TryGetValue(world, out WorldData data))
            {
                if (data.poolsCount != world.PoolsCount)
                {
                    data = CreateWorldData(world);
                    _worldDatas[world] = data;
                }
            }
            else
            {
                data = CreateWorldData(world);
                _worldDatas[world] = data;
                world.AddListener(new Listener(world));
            }

            return data.addComponentGenericMenu;
        }

        private static WorldData CreateWorldData(EcsWorld world)
        {
            GenericMenu genericMenu = new GenericMenu();

            var pools = world.AllPools;
            for (int i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];
                if (pool.IsNullOrDummy())
                {
                    continue;
                }
                var meta = pool.ComponentType.ToMeta();
                string name = meta.Group.Name + meta.Name;
                genericMenu.AddItem(new GUIContent(name, meta.Description.Text), false, OnAddComponent, pool);
            }
            return new WorldData(genericMenu, world.PoolsCount);
        }

        public static int CurrentEntityID = 0;

        private static void OnAddComponent(object userData)
        {
            IEcsPool pool = (IEcsPool)userData;
            if (pool.World.IsUsed(CurrentEntityID) == false)
            {
                return;
            }
            if (pool.Has(CurrentEntityID) == false)
            {
                pool.AddRaw(CurrentEntityID, Activator.CreateInstance(pool.ComponentType));
            }
            else
            {
                Debug.LogWarning($"Entity({CurrentEntityID}) already has component {EcsDebugUtility.GetGenericTypeName(pool.ComponentType)}.");
            }
        }

        private class Listener : IEcsWorldEventListener
        {
            private EcsWorld _world;
            public Listener(EcsWorld world)
            {
                _world = world;
            }
            public void OnReleaseDelEntityBuffer(ReadOnlySpan<int> buffer) { }
            public void OnWorldDestroy()
            {
                _worldDatas.Remove(_world);
            }
            public void OnWorldResize(int newSize) { }
        }
    }
}
#endif