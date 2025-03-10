using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityObject = UnityEngine.Object;

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
    using UnityEditor;
    using Assembly = System.Reflection.Assembly;

    [InitializeOnLoad]
    internal static partial class UnityEditorUtility
    {
        static UnityEditorUtility()
        {
            _integrationAssembly = typeof(UnityEditorUtility).Assembly;

            colorBoxeStyles = new SparseArray<GUIStyle>();

            List<Type> serializableTypes = new List<Type>();
            List<EntityEditorBlockDrawer> entityEditorBlockDrawers = new List<EntityEditorBlockDrawer>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //var targetTypes = assembly.GetTypes().Where(type =>
                //    (type.IsGenericType || type.IsAbstract || type.IsInterface) == false &&
                //    type.IsSubclassOf(typeof(UnityObject)) == false &&
                //    type.GetCustomAttribute<SerializableAttribute>() != null);

                foreach (var type in assembly.GetTypes())
                {
                    if ((type.IsGenericType || type.IsAbstract || type.IsInterface) == false && 
                        typeof(EntityEditorBlockDrawer).IsAssignableFrom(type))
                    {
                        var drawer = (EntityEditorBlockDrawer)Activator.CreateInstance(type);
                        entityEditorBlockDrawers.Add(drawer);
                    }
                }

                var targetTypes = assembly.GetTypes().Where(type =>
                    (type.IsGenericType || type.IsAbstract || type.IsInterface) == false &&
                    type.IsSubclassOf(typeof(UnityObject)) == false &&
                    type.GetConstructor(Type.EmptyTypes) != null);

                serializableTypes.AddRange(targetTypes);
            }
            _serializableTypes = serializableTypes.ToArray();
            _entityEditorBlockDrawers = entityEditorBlockDrawers.ToArray();
            _serializableTypeWithMetaIDMetas = serializableTypes
                .Where(TypeMeta.IsHasMetaID)
                .Select(type => type.ToMeta())
                .ToArray();

            foreach (var item in _serializableTypeWithMetaIDMetas)
            {
                _metaIDTypePairs[item.MetaID] = item.Type;
            }
            //Array.Sort(_serializableTypes, (a, b) => string.Compare(a.AssemblyQualifiedName, b.AssemblyQualifiedName, StringComparison.Ordinal));

            //_noHiddenSerializableTypes = _serializableTypes.Where(o => {
            //    var atr = o.GetCustomAttribute<MetaTagsAttribute>();
            //    return atr != null && atr.Tags.Contains(MetaTags.HIDDEN);
            //}).ToArray();
        }

        internal static readonly Assembly _integrationAssembly;
        internal static readonly Type[] _serializableTypes;
        internal static readonly EntityEditorBlockDrawer[] _entityEditorBlockDrawers;
        internal static readonly TypeMeta[] _serializableTypeWithMetaIDMetas;
        private static readonly Dictionary<string, Type> _metaIDTypePairs = new Dictionary<string, Type>();

        public static bool TryGetTypeForMetaID(string metaID, out Type type)
        {
            return _metaIDTypePairs.TryGetValue(metaID, out type);
        }

        //private static Type[] _noHiddenSerializableTypes;

        private static SparseArray<GUIStyle> colorBoxeStyles = new SparseArray<GUIStyle>();
        private static GUIContent _singletonIconContent = null;
        private static GUIContent _singletonContent = null;
        private static GUIStyle _inputFieldCenterAnhor = null;

        private static Dictionary<Type, MonoScript> scriptsAssets = new Dictionary<Type, MonoScript>(256);


        internal static void ResetValues(this SerializedProperty property, bool isExpand = false)
        {
            ResetValues_Internal(property.Copy(), isExpand, property.depth);
        }
        private static void ResetValues_Internal(SerializedProperty property, bool isExpand, int depth)
        {
            property.isExpanded = isExpand;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    try
                    //TODO хз как с этим работать, но это говно постоянно кидает 
                    //InvalidOperationException: The operation is not possible when moved past all properties (Next returned false)
                    //и не дает инструментов и шансов этого избежать
                    {
                        bool x = true;
                        while (property.Next(x) && property.depth > depth)
                        {
                            ResetValues_Internal(property, isExpand, property.depth);
                            x = false;
                        }
                    }
                    catch (Exception) { }
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = default;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = default;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = default;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = default;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = default;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = default;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = default;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = default;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = default;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.ClearArray();
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = default;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = new AnimationCurve();
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = default;
                    break;
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    property.gradientValue = new Gradient();;
           
#else
                    Debug.LogWarning($"Unsupported SerializedPropertyType: {property.propertyType}");
#endif
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = Quaternion.identity;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.objectReferenceValue = default;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    for (int i = 0, iMax = property.fixedBufferSize; i < iMax; i++)
                    {
                        property.GetFixedBufferElementAtIndex(i).intValue = default;
                    }
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = default;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = default;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = default;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = default;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = default;
                    break;
                case SerializedPropertyType.Hash128:
                    property.hash128Value = default;
                    break;
                default:
                    Debug.LogWarning($"Unsupported SerializedPropertyType: {property.propertyType}");
                    break;
            }
        }
        internal static bool TryGetScriptAsset(Type type, out MonoScript script)
        {
            if (scriptsAssets.TryGetValue(type, out script) == false)
            {
                script = null;
                string name = type.Name;
                int indexOf = name.LastIndexOf('`');
                if (indexOf >= 0)
                {
                    name = name.Substring(0, indexOf);
                }
                var guids = AssetDatabase.FindAssets($"{name} t:MonoScript");
                for (var i = 0; i < guids.Length; i++)
                {
                    MonoScript textAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (textAsset != null && textAsset.name == name)
                    {
                        script = textAsset;
                        break;
                    }
                }
                scriptsAssets.Add(type, script);
            }
            return script != null;
        }

        #region Label
        public static GUIStyle GetInputFieldCenterAnhor()
        {
            if (_inputFieldCenterAnhor == null)
            {
                GUIStyle style = new GUIStyle(EditorStyles.numberField);
                style.alignment = TextAnchor.MiddleCenter;
                style.font = EditorStyles.boldFont;
                _inputFieldCenterAnhor = style;
            }
            return _inputFieldCenterAnhor;
        }
        public static GUIContent GetLabelTemp()
        {
            if (_singletonContent == null)
            {
                _singletonContent = new GUIContent();
            }
            _singletonContent.text = string.Empty;
            _singletonContent.tooltip = string.Empty;
            _singletonContent.image = null;
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
            public RuntimeComponentDropDown addComponentGenericMenu;
            public int poolsCount;
            public WorldData(RuntimeComponentDropDown addComponentGenericMenu, int poolsCount)
            {
                this.addComponentGenericMenu = addComponentGenericMenu;
                this.poolsCount = poolsCount;
            }
        }
        //world id
        private static Dictionary<EcsWorld, WorldData> _worldDatas = new Dictionary<EcsWorld, WorldData>();

        public static RuntimeComponentDropDown GetAddComponentGenericMenu(EcsWorld world)
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
            IEnumerable<IEcsPool> pools = world.AllPools.ToArray().Where(o => o.IsNullOrDummy() == false);
            RuntimeComponentDropDown genericMenu = new RuntimeComponentDropDown(pools);
            return new WorldData(genericMenu, world.PoolsCount);
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