#if UNITY_EDITOR
using DCFApixels.DragonECS.Core;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityComponent = UnityEngine.Component;
using UnityObject = UnityEngine.Object;


namespace DCFApixels.DragonECS.Unity.Editors.X
{
    internal class RuntimeComponentsDrawer
    {
        private const BindingFlags INSTANCE_FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeComponentReflectionCache()
        {
            RefEditorWrapper.ResetStaticState();
            foreach (var item in _runtimeComponentReflectionCaches)
            {
                item.Value.ResetWrappers();
            }
            //_runtimeComponentReflectionCaches.Clear();
        }
        private const int RuntimeComponentsMaxDepth = 2;
        private const int RuntimeComponentsDepthRoot = -1;

        private static RuntimeComponentsDrawer[] _drawers;
        private static int _runtimeComponentsDepth = RuntimeComponentsDepthRoot;
        static RuntimeComponentsDrawer()
        {
            _drawers = new RuntimeComponentsDrawer[RuntimeComponentsMaxDepth + 1];
            _drawers[0] = new RuntimeComponentsDrawer();
            _drawers[1] = new RuntimeComponentsDrawer();
            _drawers[2] = new RuntimeComponentsDrawer();
        }

        private List<IEcsPool> _componentPoolsBuffer = new List<IEcsPool>(64);


        #region Properties
        private static RuntimeDrawMode RuntimeDrawMode
        {
            get { return UserSettingsPrefs.instance.RuntimeDrawMode; }
            set { UserSettingsPrefs.instance.RuntimeDrawMode = value; }
        }
        private static bool IsShowHidden
        {
            get { return UserSettingsPrefs.instance.IsShowHidden; }
            set { UserSettingsPrefs.instance.IsShowHidden = value; }
        }
        private static bool IsShowRuntimeComponents
        {
            get { return UserSettingsPrefs.instance.IsShowRuntimeComponents; }
            set { UserSettingsPrefs.instance.IsShowRuntimeComponents = value; }
        }
        #endregion

        #region reflection cache
        internal class RuntimeComponentReflectionCache
        {
            public readonly Type Type;
            public readonly bool IsValueType;
            public readonly bool IsUnmanaged;
            public readonly DrawerType DrawerType;
            public readonly FieldInfoData[] Fields;

            private RefEditorWrapper[] _wrappers = new RefEditorWrapper[RuntimeComponentsMaxDepth];
            public RefEditorWrapper GetWrapper(int depth)
            {
                return _wrappers[depth];
            }
            public RuntimeComponentReflectionCache(Type type)
            {
                Type = type;
                ResetWrappers();
                IsUnmanaged = UnsafeUtility.IsUnmanaged(type);
                IsValueType = type.IsValueType;

                bool isVoideType = type == typeof(void);
                bool isUnityObjectType = typeof(UnityObject).IsAssignableFrom(type);
                bool isLeaf = isUnityObjectType || type.IsPrimitive || type == typeof(string) || type.IsEnum;

                DrawerType = DrawerType.UNDEFINED;

                if(type.IsArray || isVoideType)
                {
                    DrawerType = DrawerType.Ignored;
                }

                if (DrawerType == DrawerType.UNDEFINED && isLeaf)
                {
                    if (type.IsEnum)
                    {
                        DrawerType = type.HasAttribute<FlagsAttribute>() ? DrawerType.EnumFlags : DrawerType.Enum;
                    }
                    else if (isUnityObjectType)
                    {
                        DrawerType = DrawerType.UnityObject;
                    }
                    else if (type == typeof(bool))
                    {
                        DrawerType = DrawerType.Bool;
                    }
                    else if (type == typeof(string))
                    {
                        DrawerType = DrawerType.String;
                    }
                    else if (type == typeof(float))
                    {
                        DrawerType = DrawerType.Float;
                    }
                    else if (type == typeof(double))
                    {
                        DrawerType = DrawerType.Double;
                    }
                    else if (type == typeof(byte))
                    {
                        DrawerType = DrawerType.Byte;
                    }
                    else if (type == typeof(sbyte))
                    {
                        DrawerType = DrawerType.SByte;
                    }
                    else if (type == typeof(short))
                    {
                        DrawerType = DrawerType.Short;
                    }
                    else if (type == typeof(ushort))
                    {
                        DrawerType = DrawerType.UShort;
                    }
                    else if (type == typeof(int))
                    {
                        DrawerType = DrawerType.Int;
                    }
                    else if (type == typeof(uint))
                    {
                        DrawerType = DrawerType.UInt;
                    }
                    else if (type == typeof(long))
                    {
                        DrawerType = DrawerType.Long;
                    }
                    else if (type == typeof(ulong))
                    {
                        DrawerType = DrawerType.ULong;
                    }
                }

                if (DrawerType == DrawerType.UNDEFINED)
                {
                    DrawerType = type.IsGenericType ? DrawerType.UnityNotSerializableComposite : DrawerType.UnitySerializableComposite;
                }

                if (isVoideType) { return; }

                if (DrawerType == DrawerType.UnityNotSerializableComposite)
                {
                    var fieldInfos = type.GetFields(INSTANCE_FIELD_FLAGS);
                    Fields = new FieldInfoData[fieldInfos.Length];
                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        var fieldInfo = fieldInfos[i];
                        Fields[i] = new FieldInfoData(fieldInfo);
                    }
                }
            }
            public void ResetWrappers()
            {
                _wrappers[0] = RefEditorWrapper.Take();
                _wrappers[1] = RefEditorWrapper.Take();
            }
            public readonly struct FieldInfoData
            {
                public readonly FieldInfo FieldInfo;
                public readonly Type FieldType;
                public readonly string UnityFormatName;
                public readonly bool IsUnityObjectField;
                public readonly bool IsPassToUnitySerialize;
                public readonly RuntimeComponentReflectionCache ValueTypeReflectionCache;

                public FieldInfoData(FieldInfo fieldInfo)
                {
                    FieldInfo = fieldInfo;
                    FieldType = fieldInfo.FieldType;
                    IsUnityObjectField = typeof(UnityObject).IsAssignableFrom(fieldInfo.FieldType);
                    UnityFormatName = UnityEditorUtility.TransformFieldName(fieldInfo.Name);
                    IsPassToUnitySerialize =
                        (fieldInfo.IsPublic || fieldInfo.HasAttribute<SerializeField>() || fieldInfo.HasAttribute<SerializeReference>()) &&
                        (fieldInfo.IsInitOnly || fieldInfo.HasAttribute<System.NonSerializedAttribute>()) == false;
                    ValueTypeReflectionCache = FieldType.IsValueType ? GetRuntimeComponentReflectionCache(FieldType) : null;
                }
                public FieldInfoData(FieldInfo fieldInfo, Type fieldType, string unityFormatName, bool isPassToSerialize = true)
                {
                    FieldInfo = fieldInfo;
                    FieldType = fieldType;
                    UnityFormatName = unityFormatName;
                    IsUnityObjectField = typeof(UnityObject).IsAssignableFrom(fieldType);
                    IsPassToUnitySerialize = isPassToSerialize;
                    ValueTypeReflectionCache = FieldType.IsValueType ? GetRuntimeComponentReflectionCache(FieldType) : null;
                }
                public RuntimeComponentReflectionCache GetReflectionCache(Type type)
                {
                    if (ValueTypeReflectionCache != null)
                    {
                        return ValueTypeReflectionCache;
                    }
                    return GetRuntimeComponentReflectionCache(type);
                }
            }
        }
        private static Dictionary<Type, RuntimeComponentReflectionCache> _runtimeComponentReflectionCaches = new Dictionary<Type, RuntimeComponentReflectionCache>();
        private static RuntimeComponentReflectionCache GetRuntimeComponentReflectionCache(Type type)
        {
            if (_runtimeComponentReflectionCaches.TryGetValue(type, out RuntimeComponentReflectionCache result) == false)
            {
                result = new RuntimeComponentReflectionCache(type);
                _runtimeComponentReflectionCaches.Add(type, result);
            }
            return result;
        }
        #endregion

        #region draw world component
        public static void DrawWorldComponents(EcsWorld world)
        {
            if (_runtimeComponentsDepth == RuntimeComponentsDepthRoot)
            {
                try
                {
                    _runtimeComponentsDepth = 0;
                    _drawers[0].DrawWorldComponents_Internal(world);
                }
                finally
                {
                    _runtimeComponentsDepth = RuntimeComponentsDepthRoot;
                }
            }
        }
        private void DrawWorldComponents_Internal(EcsWorld world)
        {
            bool isNull = world == null || world.IsDestroyed || world.ID == 0;
            if (isNull) { return; }
            using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
            {
                IsShowRuntimeComponents = EditorGUILayout.BeginFoldoutHeaderGroup(IsShowRuntimeComponents, "RUNTIME COMPONENTS", EditorStyles.foldout);
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (IsShowRuntimeComponents == false) { return; }

                var worldID = world.ID;
                var cmps = world.GetWorldComponents();
                int index = -1;
                int total = 9;
                foreach (var cmp in cmps)
                {
                    index++;
                    var meta = cmp.ComponentType.GetMeta();
                    if (meta.IsHidden == false || IsShowHidden)
                    {
                        Type componentType = cmp.ComponentType;

                        object data = cmp.GetRaw(worldID);

                        ExpandStack expandStack = ExpandStack.Take(componentType);

                        float padding = EditorGUIUtility.standardVerticalSpacing;
                        Rect optionButton = GUILayoutUtility.GetLastRect();
                        optionButton.yMin = optionButton.yMax;
                        optionButton.yMax += DragonGUI.HeadIconsRect.height;
                        optionButton.xMin = optionButton.xMax - 64;
                        optionButton.center += Vector2.up * padding * 2f;
                        //Canceling isExpanded
                        if (DragonGUI.ClickTest(optionButton))
                        {
                            ref bool isExpanded = ref expandStack.Down();
                            isExpanded = !isExpanded;
                        }

                        Color panelColor = DragonGUI.SelectPanelColor(meta, index, total);

                        using (DragonGUI.Layout.BeginVertical(panelColor.SetAlpha(EscEditorConsts.MetaBlockFillStyle_Alpha)))
                        {
                            //Edit script button
                            if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                            {
                                optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                DragonGUI.ScriptAssetButton(optionButton, script);
                            }
                            //Description icon
                            if (string.IsNullOrEmpty(meta.Description.Text) == false)
                            {
                                optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                DragonGUI.DescriptionIcon(optionButton, meta.Description.Text);
                            }

                            RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);
                            if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandStack, data, out object resultData, 0))
                            {
                                cmp.SetRaw(worldID, resultData);
                            }

                        }
                    }
                }
            }
        }
        #endregion

        #region draw entity component
        public static void DrawRuntimeComponents(int entityID, EcsWorld world, bool isWithFoldout, bool isRoot)
        {
            if (isRoot)
            {
                _runtimeComponentsDepth = RuntimeComponentsDepthRoot;
            }
            _runtimeComponentsDepth++;

            try
            {
                _drawers[_runtimeComponentsDepth].DrawRuntimeComponents(entityID, world, isWithFoldout);
            }
            finally
            {
                _runtimeComponentsDepth--;
                if (_runtimeComponentsDepth < RuntimeComponentsDepthRoot)
                {
                    _runtimeComponentsDepth = RuntimeComponentsDepthRoot;
                }
            }
        }
        private void DrawRuntimeComponents(int entityID, EcsWorld world, bool isWithFoldout)
        {
            using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle())) using (DragonGUI.SetIndentLevel(0))
            {
                if (_runtimeComponentsDepth >= RuntimeComponentsMaxDepth)
                {
                    GUILayout.Label("Max depth for inspecting components at runtime");
                    return;
                }
                if (isWithFoldout)
                {
                    IsShowRuntimeComponents = EditorGUILayout.BeginFoldoutHeaderGroup(IsShowRuntimeComponents, "RUNTIME COMPONENTS", EditorStyles.foldout);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                if (isWithFoldout == false || IsShowRuntimeComponents)
                {
                    if (DragonGUI.Layout.AddComponentButtons(out Rect dropDownRect))
                    {
                        RuntimeComponentsUtility.GetAddComponentGenericMenu(world).Open(dropDownRect, entityID);
                    }

                    using (DragonGUI.Layout.BeginVertical(Color.white.SetAlpha(0.066f)))
                    {
                        IsShowHidden = EditorGUILayout.Toggle("Show Hidden", IsShowHidden);
                        RuntimeDrawMode = (RuntimeDrawMode)EditorGUILayout.EnumPopup("Draw Mode", selected: RuntimeDrawMode);
                    }

                    world.GetComponentPoolsFor(entityID, _componentPoolsBuffer);
                    for (int i = 0; i < _componentPoolsBuffer.Count; i++)
                    {
                        var pool = _componentPoolsBuffer[i];
                        if (pool.ComponentType.IsValueType)
                        {
                            DrawRuntimeValueComponent(entityID, pool, 9, i);
                        }
                        else
                        {
                            DrawRuntimeClassComponent(entityID, pool, 9, i);
                        }
                    }
                }
            }
        }
        private struct DrawRuntimeCompoentnsCahce : IEcsWorldComponent<DrawRuntimeCompoentnsCahce>
        {
            public EcsWorld World;
            public Record[] Records;

            void IEcsWorldComponent<DrawRuntimeCompoentnsCahce>.Init(ref DrawRuntimeCompoentnsCahce component, EcsWorld world)
            {
                component.World = world;
                component.Records = new Record[world.Count];
            }
            void IEcsWorldComponent<DrawRuntimeCompoentnsCahce>.OnDestroy(ref DrawRuntimeCompoentnsCahce component, EcsWorld world) { }

            public ref Record GetFor(int componentTypeID)
            {
                if(componentTypeID >= Records.Length)
                {
                    var newSize = DragonArrayUtility.NextPow2(componentTypeID + 1);
                    Array.Resize(ref Records, newSize);
                }
                ref var result = ref Records[componentTypeID];
                if(result.IsInit == false && World.TryFindPoolInstance(componentTypeID, out var pool))
                {
                    var componentType = pool.ComponentType;
                    result.Meta = componentType.GetMeta();
                    result.ExpandStack = ExpandStack.Take(componentType);
                    result.FieldInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, result.Meta.Name);
                    if (ScriptsCache.TryGetScriptAsset(result.Meta, out result.ScriptReference) == false)
                    {
                        result.ScriptReference = null;
                    }

                    result.IsInit = true;
                }
                return ref result;
            }

            public struct Record
            {
                public bool IsInit;
                public TypeMeta Meta;
                public ExpandStack ExpandStack;
                public RuntimeComponentReflectionCache.FieldInfoData FieldInfoData;
                public MonoScript ScriptReference;
            }
        }

        private void DrawRuntimeValueComponent(int entityID, IEcsPool pool, int total, int index)
        {
            ref var cache = ref pool.World.Get<DrawRuntimeCompoentnsCahce>().GetFor(pool.ComponentTypeID);
            
            var meta = cache.Meta;
            if (meta.IsHidden == false || IsShowHidden)
            {
                float padding = EditorGUIUtility.standardVerticalSpacing;
                Rect optionButton = GUILayoutUtility.GetLastRect();
                optionButton.yMin = optionButton.yMax;
                optionButton.yMax += DragonGUI.HeadIconsRect.height;
                optionButton.xMin = optionButton.xMax - 64;
                optionButton.center += Vector2.up * padding * 2f;

                Color fillColor = Color.clear;
                Color backColor = Color.clear;
                if (Event.current.type == EventType.Repaint)
                {
                    switch (UserSettingsPrefs.instance.MetaBlockRectStyle)
                    {
                        default:
                        case MetaBlockRectStyle.Clean:
                            {
                                backColor = Color.white.SetAlpha(EscEditorConsts.MetaBlockCleanStyle_Alpha);
                            }
                            break;
                        case MetaBlockRectStyle.Edge:
                            {
                                backColor = Color.white.SetAlpha(EscEditorConsts.MetaBlockCleanStyle_Alpha);
                                fillColor = DragonGUI.SelectPanelColor(meta, index, total)
                                    .SetAlpha(EscEditorConsts.MetaBlockEdgeStyle_Alpha);
                            }
                            break;
                        case MetaBlockRectStyle.Fill:
                            {
                                backColor = DragonGUI.SelectPanelColor(meta, index, total)
                                    .Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE)
                                    .SetAlpha(EscEditorConsts.MetaBlockFillStyle_Alpha);
                            }
                            break;
                    }
                }

                using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetWhiteStyle(), backColor))
                {
                    using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetWhiteEdge4Style(), fillColor))
                    {
                        //Close button
                        optionButton.xMin = optionButton.xMax - DragonGUI.HeadIconsRect.width;
                        if (DragonGUI.CloseButton(optionButton))
                        {
                            pool.Del(entityID);
                            return;
                        }
                        //Edit script button
                        if (cache.ScriptReference != null)
                        {
                            optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            DragonGUI.ScriptAssetButton(optionButton, cache.ScriptReference);
                        }
                        //Description icon
                        if (string.IsNullOrEmpty(meta.Description.Text) == false)
                        {
                            optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            DragonGUI.DescriptionIcon(optionButton, meta.Description.Text);
                        }


                        ref bool peakExpand = ref cache.ExpandStack.Peek();
                        var label = UnityEditorUtility.GetLabel(meta.Name);
                        if (peakExpand)
                        {
                            object data = pool.GetRaw(entityID);
                            if (DrawRuntimeData(ref cache.FieldInfoData, label, cache.ExpandStack, data, out object resultData, 0))
                            {
                                pool.SetRaw(entityID, resultData);
                            }
                        }
                        else
                        {
                            var foldoutStyle = EditorStyles.foldout;
                            Rect rect = GUILayoutUtility.GetRect(label, foldoutStyle);
                            peakExpand = EditorGUI.BeginFoldoutHeaderGroup(rect, false, label, foldoutStyle, null, null);
                            EditorGUILayout.EndFoldoutHeaderGroup();
                        }
                    }
                }
            }
        }
        private void DrawRuntimeClassComponent(int entityID, IEcsPool pool, int total, int index)
        {
            object data = pool.GetRaw(entityID);
            Type componentType = data.GetType();
            var meta = componentType.GetMeta();
            if (meta.IsHidden == false || IsShowHidden)
            {
                ExpandStack expandStack = ExpandStack.Take(componentType);

                float padding = EditorGUIUtility.standardVerticalSpacing;
                Rect optionButton = GUILayoutUtility.GetLastRect();
                optionButton.yMin = optionButton.yMax;
                optionButton.yMax += DragonGUI.HeadIconsRect.height;
                optionButton.xMin = optionButton.xMax - 64;
                optionButton.center += Vector2.up * padding * 2f;

                Color fillColor = Color.clear;
                Color backColor = Color.clear;
                if (Event.current.type == EventType.Repaint)
                {
                    switch (UserSettingsPrefs.instance.MetaBlockRectStyle)
                    {
                        default:
                        case MetaBlockRectStyle.Clean:
                            {
                                backColor = Color.white.SetAlpha(EscEditorConsts.MetaBlockCleanStyle_Alpha);
                            }
                            break;
                        case MetaBlockRectStyle.Edge:
                            {
                                backColor = Color.white.SetAlpha(EscEditorConsts.MetaBlockCleanStyle_Alpha);
                                fillColor = DragonGUI.SelectPanelColor(meta, index, total)
                                    .SetAlpha(EscEditorConsts.MetaBlockEdgeStyle_Alpha);
                            }
                            break;
                        case MetaBlockRectStyle.Fill:
                            {
                                backColor = DragonGUI.SelectPanelColor(meta, index, total)
                                    .Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE)
                                    .SetAlpha(EscEditorConsts.MetaBlockFillStyle_Alpha);
                            }
                            break;
                    }
                }


                using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetWhiteStyle(), backColor))
                {
                    using (DragonGUI.Layout.BeginVertical(UnityEditorUtility.GetWhiteEdge4Style(), fillColor))
                    {
                        //Close button
                        optionButton.xMin = optionButton.xMax - DragonGUI.HeadIconsRect.width;
                        if (DragonGUI.CloseButton(optionButton))
                        {
                            pool.Del(entityID);
                            return;
                        }
                        //Edit script button
                        if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                        {
                            optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            DragonGUI.ScriptAssetButton(optionButton, script);
                        }
                        //Description icon
                        if (string.IsNullOrEmpty(meta.Description.Text) == false)
                        {
                            optionButton = DragonGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            DragonGUI.DescriptionIcon(optionButton, meta.Description.Text);
                        }

                        RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);

                        if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandStack, data, out object resultData, 0))
                        {
                            pool.SetRaw(entityID, resultData);
                        }
                    }
                }
            }
        }
        #endregion



        #region draw data
        private bool DrawRuntimeData(ref RuntimeComponentReflectionCache.FieldInfoData fieldInfoData, GUIContent label, ExpandStack expandStack, object data, out object outData, int depth)
        {
            const int DEPTH_MAX = 24;

            using (DragonGUI.CheckChanged())
            {
                outData = data;
                object newData = data;
                Type type = data == null ? typeof(void) : data.GetType();
                bool isUnityObjectField = fieldInfoData.IsUnityObjectField;
                if (isUnityObjectField == false && data == null)
                {
                    EditorGUILayout.TextField(label, "Null");
                    return false;
                }

                var reflectionCache = fieldInfoData.GetReflectionCache(type);
                if (depth >= DEPTH_MAX || reflectionCache == null)
                {
                    EditorGUILayout.TextField(label, "error");
                    return false;
                }

                ref bool isExpanded = ref expandStack.Down();
                bool childElementChanged = false;
                var eventType = Event.current.type;

                var label2 = "-";
                var drawerType = reflectionCache.DrawerType;

                if (isUnityObjectField)
                {
                    drawerType = DrawerType.UnityObject;
                }
                switch (drawerType)
                {
                    case DrawerType.UNDEFINED:
                        {
                            EditorGUILayout.LabelField(label, label2);
                        }
                        break;
                    case DrawerType.Ignored:
                        {
                            EditorGUILayout.LabelField(label, label2);
                        }
                        break;
                    case DrawerType.UnitySerializableComposite:
                        {
                            using (DragonGUI.CheckChanged())
                            {
                                RefEditorWrapper wrapper = reflectionCache.GetWrapper(_runtimeComponentsDepth);
                                wrapper.data = data;
                                wrapper.SO.Update();
                                wrapper.IsExpanded = isExpanded;
                                EditorGUILayout.PropertyField(wrapper.Property, label, true);

                                if (DragonGUI.Changed)
                                {
                                    wrapper.SO.ApplyModifiedProperties();
                                    newData = wrapper.Data;
                                    childElementChanged = true;
                                }
                                isExpanded = wrapper.IsExpanded;
                            }
                        }
                        break;
                    case DrawerType.UnityNotSerializableComposite:
                        {
                            GUILayout.Space(DragonGUI.Spacing);
                            var foldoutStyle = EditorStyles.foldout;
                            Rect rect = GUILayoutUtility.GetRect(label, foldoutStyle);
                            //rect.xMin += EcsGUI.Indent;
                            isExpanded = EditorGUI.BeginFoldoutHeaderGroup(rect, isExpanded, label, foldoutStyle, null, null);
                            EditorGUILayout.EndFoldoutHeaderGroup();

                            if (isExpanded)
                            {
                                using (DragonGUI.UpIndentLevel())
                                {
                                    for (int j = 0, jMax = reflectionCache.Fields.Length; j < jMax; j++)
                                    {
                                        var field = reflectionCache.Fields[j];
                                        if (DrawRuntimeData(ref field, UnityEditorUtility.GetLabel(field.UnityFormatName), expandStack, field.FieldInfo.GetValue(data), out object fieldData, depth + 1))
                                        {
                                            field.FieldInfo.SetValue(data, fieldData);
                                            newData = data;
                                            childElementChanged = true;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case DrawerType.UnityObject:
                        {
                            using (DragonGUI.CheckChanged())
                            {
                                var uobj = UnsafeUtility.As<object, UnityObject>(ref data);
                                bool isComponent = typeof(UnityComponent).IsAssignableFrom(fieldInfoData.FieldType);
                                var newuobj = EditorGUILayout.ObjectField(label, uobj, fieldInfoData.FieldType, true);

                                if (uobj != newuobj)
                                {
                                    if (isComponent && newuobj is GameObject go)
                                    {
                                        newuobj = go.GetComponent(fieldInfoData.FieldType);
                                    }
                                    newData = newuobj;
                                    childElementChanged = true;
                                }
                            }
                        }
                        break;
                    case DrawerType.Enum:
                        {
                            if (eventType != EventType.Layout)
                            {
                                var enumData = UnsafeUtility.As<object, Enum>(ref data);
                                newData = EditorGUILayout.EnumPopup(label, enumData);
                            }
                            else
                            {
                                EditorGUILayout.EnumPopup(label, default);
                            }
                        }
                        break;
                    case DrawerType.EnumFlags:
                        {
                            if (eventType != EventType.Layout)
                            {
                                var enumData = UnsafeUtility.As<object, Enum>(ref data);
                                newData = EditorGUILayout.EnumFlagsField(label, enumData);
                            }
                            else
                            {
                                EditorGUILayout.EnumFlagsField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Bool:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = EditorGUILayout.Toggle(label, (bool)data);
                            }
                            else
                            {
                                EditorGUILayout.Toggle(label, default);
                            }
                        }
                        break;
                    case DrawerType.String:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = EditorGUILayout.TextField(label, (string)data);
                            }
                            else
                            {
                                EditorGUILayout.TextField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Float:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = EditorGUILayout.FloatField(label, (float)data);
                            }
                            else
                            {
                                EditorGUILayout.FloatField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Double:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = EditorGUILayout.DoubleField(label, (double)data);
                            }
                            else
                            {
                                EditorGUILayout.DoubleField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Byte:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (byte)EditorGUILayout.IntField(label, (byte)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.SByte:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (sbyte)EditorGUILayout.IntField(label, (sbyte)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Short:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (short)EditorGUILayout.IntField(label, (short)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.UShort:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (ushort)EditorGUILayout.IntField(label, (ushort)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Int:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (int)EditorGUILayout.IntField(label, (int)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.UInt:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (uint)EditorGUILayout.IntField(label, (int)(uint)data);
                            }
                            else
                            {
                                EditorGUILayout.IntField(label, default);
                            }
                        }
                        break;
                    case DrawerType.Long:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = EditorGUILayout.LongField(label, (long)data);
                            }
                            else
                            {
                                EditorGUILayout.LongField(label, default);
                            }
                        }
                        break;
                    case DrawerType.ULong:
                        {
                            if (eventType != EventType.Layout)
                            {
                                newData = (ulong)EditorGUILayout.LongField(label, (long)(ulong)data);
                            }
                            else
                            {
                                EditorGUILayout.LongField(label, default);
                            }
                        }
                        break;
                    default:
                        {
                            EditorGUILayout.LabelField(label, label2);
                        }
                        break;
                }

                expandStack.Up();
                if (childElementChanged || DragonGUI.Changed)
                {
                    outData = newData;
                    return true;
                }
            }

            return false;
        }

        public enum DrawerType
        {
            UNDEFINED = 0,
            Ignored,
            // Composite
            UnitySerializableComposite,
            UnityNotSerializableComposite,
            // Leaft types
            UnityObject,
            Enum,
            EnumFlags,
            Bool,
            String,
            Float,
            Double,
            Byte,
            SByte,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
        }
        #endregion
    }
}
#endif