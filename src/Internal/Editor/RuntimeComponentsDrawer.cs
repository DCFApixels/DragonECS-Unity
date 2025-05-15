#if UNITY_EDITOR
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
        private static ComponentColorMode AutoColorMode
        {
            get { return UserSettingsPrefs.instance.ComponentColorMode; }
            set { UserSettingsPrefs.instance.ComponentColorMode = value; }
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
                _drawers[0].DrawWorldComponents_Internal(world);
            }
        }
        private void DrawWorldComponents_Internal(EcsWorld world)
        {
            bool isNull = world == null || world.IsDestroyed || world.ID == 0;
            if (isNull) { return; }
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
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
                    var meta = cmp.ComponentType.ToMeta();
                    if (meta.IsHidden == false || IsShowHidden)
                    {
                        Type componentType = cmp.ComponentType;

                        object data = cmp.GetRaw(worldID);

                        ExpandMatrix expandMatrix = ExpandMatrix.Take(componentType);

                        float padding = EditorGUIUtility.standardVerticalSpacing;
                        Rect optionButton = GUILayoutUtility.GetLastRect();
                        optionButton.yMin = optionButton.yMax;
                        optionButton.yMax += EcsGUI.HeadIconsRect.height;
                        optionButton.xMin = optionButton.xMax - 64;
                        optionButton.center += Vector2.up * padding * 2f;
                        //Canceling isExpanded
                        if (EcsGUI.ClickTest(optionButton))
                        {
                            ref bool isExpanded = ref expandMatrix.Down();
                            isExpanded = !isExpanded;
                        }

                        Color panelColor = EcsGUI.SelectPanelColor(meta, index, total);
                        using (EcsGUI.Layout.BeginVertical(panelColor.SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA)))
                        {
                            EditorGUI.BeginChangeCheck();

                            //Edit script button
                            if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                            {
                                optionButton = EcsGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                EcsGUI.ScriptAssetButton(optionButton, script);
                            }
                            //Description icon
                            if (string.IsNullOrEmpty(meta.Description.Text) == false)
                            {
                                optionButton = EcsGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                EcsGUI.DescriptionIcon(optionButton, meta.Description.Text);
                            }

                            RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);
                            if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData, 0))
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
            using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle())) using (EcsGUI.SetIndentLevel(0))
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
                    if (EcsGUI.Layout.AddComponentButtons(out Rect dropDownRect))
                    {
                        RuntimeComponentsUtility.GetAddComponentGenericMenu(world).Open(dropDownRect, entityID);
                    }

                    using (EcsGUI.SetBackgroundColor(GUI.color.SetAlpha(0.16f)))
                    {
                        GUILayout.Box("", UnityEditorUtility.GetWhiteStyle(), GUILayout.ExpandWidth(true));
                    }
                    IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);


                    world.GetComponentPoolsFor(entityID, _componentPoolsBuffer);
                    for (int i = 0; i < _componentPoolsBuffer.Count; i++)
                    {
                        DrawRuntimeComponent(entityID, _componentPoolsBuffer[i], 9, i);
                    }
                }
            }
        }
        private void DrawRuntimeComponent(int entityID, IEcsPool pool, int total, int index)
        {
            var meta = pool.ComponentType.ToMeta();
            if (meta.IsHidden == false || IsShowHidden)
            {
                Type componentType = pool.ComponentType;

                object data = pool.GetRaw(entityID);

                ExpandMatrix expandMatrix = ExpandMatrix.Take(componentType);

                float padding = EditorGUIUtility.standardVerticalSpacing;
                Rect optionButton = GUILayoutUtility.GetLastRect();
                optionButton.yMin = optionButton.yMax;
                optionButton.yMax += EcsGUI.HeadIconsRect.height;
                optionButton.xMin = optionButton.xMax - 64;
                optionButton.center += Vector2.up * padding * 2f;
                //Canceling isExpanded
                if (EcsGUI.ClickTest(optionButton))
                {
                    ref bool isExpanded = ref expandMatrix.Down();
                    isExpanded = !isExpanded;
                }

                Color panelColor = EcsGUI.SelectPanelColor(meta, index, total);

                using (EcsGUI.Layout.BeginVertical(panelColor.SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA)))
                {
                    EditorGUI.BeginChangeCheck();

                    //Close button
                    optionButton.xMin = optionButton.xMax - EcsGUI.HeadIconsRect.width;
                    if (EcsGUI.CloseButton(optionButton))
                    {
                        pool.Del(entityID);
                        return;
                    }
                    //Edit script button
                    if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                    {
                        optionButton = EcsGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                        EcsGUI.ScriptAssetButton(optionButton, script);
                    }
                    //Description icon
                    if (string.IsNullOrEmpty(meta.Description.Text) == false)
                    {
                        optionButton = EcsGUI.HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                        EcsGUI.DescriptionIcon(optionButton, meta.Description.Text);
                    }

                    RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);

                    if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData, 0))
                    {
                        pool.SetRaw(entityID, resultData);
                    }
                }
            }
        }
        #endregion



        #region draw data
        private bool DrawRuntimeData(ref RuntimeComponentReflectionCache.FieldInfoData fieldInfoData, GUIContent label, ExpandMatrix expandMatrix, object data, out object outData, int depth)
        {
            const int DEPTH_MAX = 24;

            using (EcsGUI.CheckChanged())
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

                RuntimeComponentReflectionCache cache = fieldInfoData.GetReflectionCache(type);
                if (depth >= DEPTH_MAX || cache == null)
                {
                    EditorGUILayout.TextField(label, "error");
                    return false;
                }

                ref bool isExpanded = ref expandMatrix.Down();
                bool childElementChanged = false;
                var eventType = Event.current.type;

                var label2 = "-";
                var drawerType = cache.DrawerType;

                if (isUnityObjectField)
                {
                    drawerType = DrawerType.UnityObject;
                }
                switch (drawerType)
                {
                    case DrawerType.UNDEFINED:
                        EditorGUILayout.LabelField(label, label2);
                        break;
                    case DrawerType.Ignored:
                        EditorGUILayout.LabelField(label, label2);
                        break;
                    case DrawerType.UnitySerializableComposite:

                        using (EcsGUI.CheckChanged())
                        {
                            RefEditorWrapper wrapper = cache.GetWrapper(_runtimeComponentsDepth);
                            wrapper.data = data;
                            wrapper.SO.Update();
                            wrapper.IsExpanded = isExpanded;
                            EditorGUILayout.PropertyField(wrapper.Property, label, true);

                            if (EcsGUI.Changed)
                            {
                                wrapper.SO.ApplyModifiedProperties();
                                newData = wrapper.Data;
                                childElementChanged = true;
                            }
                            isExpanded = wrapper.IsExpanded;
                        }

                        break;
                    case DrawerType.UnityNotSerializableComposite:

                        GUILayout.Space(EcsGUI.Spacing);
                        var foldoutStyle = EditorStyles.foldout;
                        Rect rect = GUILayoutUtility.GetRect(label, foldoutStyle);
                        //rect.xMin += EcsGUI.Indent;
                        isExpanded = EditorGUI.BeginFoldoutHeaderGroup(rect, isExpanded, label, foldoutStyle, null, null);
                        EditorGUILayout.EndFoldoutHeaderGroup();

                        if (isExpanded)
                        {
                            using (EcsGUI.UpIndentLevel())
                            {
                                for (int j = 0, jMax = cache.Fields.Length; j < jMax; j++)
                                {
                                    var field = cache.Fields[j];
                                    if (DrawRuntimeData(ref field, UnityEditorUtility.GetLabel(field.UnityFormatName), expandMatrix, field.FieldInfo.GetValue(data), out object fieldData, depth + 1))
                                    {
                                        field.FieldInfo.SetValue(data, fieldData);
                                        newData = data;
                                        childElementChanged = true;
                                    }
                                }
                            }
                        }

                        break;
                    case DrawerType.UnityObject:

                        using (EcsGUI.CheckChanged())
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

                        break;
                    case DrawerType.Enum:

                        if (eventType != EventType.Layout)
                        {
                            var enumData = UnsafeUtility.As<object, Enum>(ref data);
                            newData = EditorGUILayout.EnumPopup(label, enumData);
                        }
                        else
                        {
                            EditorGUILayout.EnumPopup(label, default);
                        }

                        break;
                    case DrawerType.EnumFlags:

                        if (eventType != EventType.Layout)
                        {
                            var enumData = UnsafeUtility.As<object, Enum>(ref data);
                            newData = EditorGUILayout.EnumFlagsField(label, enumData);
                        }
                        else
                        {
                            EditorGUILayout.EnumFlagsField(label, default);
                        }

                        break;
                    case DrawerType.Bool:
                        if (eventType != EventType.Layout)
                        {
                            newData = EditorGUILayout.Toggle(label, (bool)data);
                        }
                        else
                        {
                            EditorGUILayout.Toggle(label, default);
                        }
                        break;
                    case DrawerType.String:
                        if (eventType != EventType.Layout)
                        {
                            newData = EditorGUILayout.TextField(label, (string)data);
                        }
                        else
                        {
                            EditorGUILayout.TextField(label, default);
                        }
                        break;
                    case DrawerType.Float:
                        if (eventType != EventType.Layout)
                        {
                            newData = EditorGUILayout.FloatField(label, (float)data);
                        }
                        else
                        {
                            EditorGUILayout.FloatField(label, default);
                        }
                        break;
                    case DrawerType.Double:
                        if (eventType != EventType.Layout)
                        {
                            newData = EditorGUILayout.DoubleField(label, (double)data);
                        }
                        else
                        {
                            EditorGUILayout.DoubleField(label, default);
                        }
                        break;
                    case DrawerType.Byte:
                        if (eventType != EventType.Layout)
                        {
                            newData = (byte)EditorGUILayout.IntField(label, (byte)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.SByte:
                        if (eventType != EventType.Layout)
                        {
                            newData = (sbyte)EditorGUILayout.IntField(label, (sbyte)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.Short:
                        if (eventType != EventType.Layout)
                        {
                            newData = (short)EditorGUILayout.IntField(label, (short)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.UShort:
                        if (eventType != EventType.Layout)
                        {
                            newData = (ushort)EditorGUILayout.IntField(label, (ushort)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.Int:
                        if (eventType != EventType.Layout)
                        {
                            newData = (int)EditorGUILayout.IntField(label, (int)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.UInt:
                        if (eventType != EventType.Layout)
                        {
                            newData = (uint)EditorGUILayout.IntField(label, (int)(uint)data);
                        }
                        else
                        {
                            EditorGUILayout.IntField(label, default);
                        }
                        break;
                    case DrawerType.Long:
                        if (eventType != EventType.Layout)
                        {
                            newData = EditorGUILayout.LongField(label, (long)data);
                        }
                        else
                        {
                            EditorGUILayout.LongField(label, default);
                        }
                        break;
                    case DrawerType.ULong:
                        if (eventType != EventType.Layout)
                        {
                            newData = (ulong)EditorGUILayout.LongField(label, (long)(ulong)data);
                        }
                        else
                        {
                            EditorGUILayout.LongField(label, default);
                        }
                        break;
                    default:
                        EditorGUILayout.LabelField(label, label2);
                        break;
                }

                expandMatrix.Up();
                if (childElementChanged || EcsGUI.Changed)
                {
                    outData = newData;
                    return true;
                }
            }

            return false;
        }
        #endregion

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
    }
}
#endif