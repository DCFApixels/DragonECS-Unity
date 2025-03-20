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

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static partial class EcsGUI
    {
        public static partial class Layout
        {

            public static void ManuallySerializeButton(UnityObject obj)
            {
                if (GUILayout.Button(UnityEditorUtility.GetLabel("Manually serialize")))
                {
                    var so = new SerializedObject(obj);
                    EditorUtility.SetDirty(obj);
                    so.UpdateIfRequiredOrScript();
                    so.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            public static void ScriptAssetButton(MonoScript script, params GUILayoutOption[] options)
            {
                EcsGUI.ScriptAssetButton(GUILayoutUtility.GetRect(UnityEditorUtility.GetLabelTemp(), EditorStyles.miniButton, options), script);
            }


            public static void CopyMetaIDButton(string metaID, params GUILayoutOption[] options)
            {
                Rect r = GUILayoutUtility.GetRect(UnityEditorUtility.GetLabelTemp(), EditorStyles.miniButton, options);
                var current = Event.current;
                var hover = IconHoverScan(r, current);
                using (new ColorScope(new Color(1f, 1f, 1f, hover ? 1f : 0.8f)))
                {
                    DrawIcon(r, Icons.Instance.MetaIDIcon, hover ? 1f : 2f, metaID);
                    if (hover && current.type == EventType.MouseUp)
                    {
                        GUIUtility.systemCopyBuffer = metaID;
                    }
                }
            }
            public static bool IconButton(Texture icon, params GUILayoutOption[] options)
            {
                bool result = GUILayout.Button(UnityEditorUtility.GetLabel(string.Empty), options);
                DrawIcon(GUILayoutUtility.GetLastRect(), icon, 0, null);
                return result;
            }
            public static bool IconButton(Texture icon, float iconPadding = 0, string description = null)
            {
                bool result = GUILayout.Button(UnityEditorUtility.GetLabel(string.Empty));
                DrawIcon(GUILayoutUtility.GetLastRect(), icon, iconPadding, description);
                return result;
            }
            public static bool IconButton(Texture icon, float iconPadding = 0, string description = null, GUIStyle style = null, params GUILayoutOption[] options)
            {
                bool result;
                if (style == null)
                {
                    result = GUILayout.Button(UnityEditorUtility.GetLabel(string.Empty), options);
                }
                else
                {
                    result = GUILayout.Button(UnityEditorUtility.GetLabel(string.Empty), style, options);
                }
                DrawIcon(GUILayoutUtility.GetLastRect(), icon, iconPadding, description);
                return result;
            }

            public static void DrawEmptyComponentProperty(SerializedProperty property, string name, bool isDisplayEmpty)
            {
                EcsGUI.DrawEmptyComponentProperty(GUILayoutUtility.GetRect(UnityEditorUtility.GetLabel(name), EditorStyles.label), property, name, isDisplayEmpty);
            }
            public static void DrawEmptyComponentProperty(SerializedProperty property, GUIContent label, bool isDisplayEmpty)
            {
                EcsGUI.DrawEmptyComponentProperty(GUILayoutUtility.GetRect(label, EditorStyles.label), property, label, isDisplayEmpty);
            }
            public static void DrawWorldBaseInfo(EcsWorld world)
            {
                bool isNull = world == null || world.IsDestroyed || world.ID == 0;
                int entitesCount = isNull ? 0 : world.Count;
                int capacity = isNull ? 0 : world.Capacity;
                long Version = isNull ? 0 : world.Version;
                int leakedEntitesCount = isNull ? 0 : world.CountLeakedEntitesDebug();
                EditorGUILayout.IntField("Entities", entitesCount, EditorStyles.boldLabel);
                EditorGUILayout.IntField("Capacity", capacity, EditorStyles.boldLabel);
                EditorGUILayout.LongField("Version", Version, EditorStyles.boldLabel);
                Color color = leakedEntitesCount > 0 ? Color.yellow : GUI.contentColor;
                using (new ContentColorScope(color))
                {
                    EditorGUILayout.IntField("Leaked Entites", leakedEntitesCount, EditorStyles.boldLabel);
                }
            }
            public static void DrawWorldComponents(EcsWorld world)
            {
                bool isNull = world == null || world.IsDestroyed || world.ID == 0;
                if (isNull) { return; }
                using (BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
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
                            optionButton.yMax += HeadIconsRect.height;
                            optionButton.xMin = optionButton.xMax - 64;
                            optionButton.center += Vector2.up * padding * 2f;
                            //Canceling isExpanded
                            if (ClickTest(optionButton))
                            {
                                ref bool isExpanded = ref expandMatrix.Down();
                                isExpanded = !isExpanded;
                            }

                            Color panelColor = SelectPanelColor(meta, index, total);
                            using (BeginVertical(panelColor.SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA)))
                            {
                                EditorGUI.BeginChangeCheck();

                                ////Close button
                                //optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
                                //if (CloseButton(optionButton))
                                //{
                                //    cmp.Del(worldID);
                                //    return;
                                //}

                                //Edit script button
                                if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                                {
                                    optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                    EcsGUI.ScriptAssetButton(optionButton, script);
                                }
                                //Description icon
                                if (string.IsNullOrEmpty(meta.Description.Text) == false)
                                {
                                    optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                                    DescriptionIcon(optionButton, meta.Description.Text);
                                }

                                RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);
                                if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData))
                                {
                                    cmp.SetRaw(worldID, resultData);
                                }

                            }
                        }
                    }
                }
            }

            #region entity bar
            public static void EntityBarForAlive(EntityStatus status, int id, short gen, short world)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityBarForAlive(GUILayoutUtility.GetRect(width, height), status, id, gen, world);
            }
            public static void EntityBar(EntityStatus status, bool isPlaceholder, int id, short gen, short world)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityBar(GUILayoutUtility.GetRect(width, height), isPlaceholder, status, id, gen, world);
            }
            public static void EntityBar(int id, short gen, short world)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityBar(GUILayoutUtility.GetRect(width, height), id, gen, world);
            }
            public static void EntityBar()
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityBar(GUILayoutUtility.GetRect(width, height));
            }
            #endregion

            public static bool AddComponentButtons(out Rect dropDownRect)
            {
                return EcsGUI.AddComponentButton(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24f), out dropDownRect);
            }
            public static AddClearButton AddClearComponentButtons(out Rect dropDownRect)
            {
                return EcsGUI.AddClearComponentButtons(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24f), out dropDownRect);
            }
            public static AddClearButton AddClearSystemButtons(out Rect dropDownRect)
            {
                return EcsGUI.AddClearSystemButtons(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 24f), out dropDownRect);
            }
            public static void DrawRuntimeComponents(entlong entity, bool isWithFoldout = true)
            {
                if (entity.TryUnpackForUnityEditor(out int entityID, out _, out _, out EcsWorld world))
                {
                    DrawRuntimeComponents(entityID, world, isWithFoldout);
                }
            }

            [ThreadStatic]
            private static List<IEcsPool> _componentPoolsBuffer;
            public static void DrawRuntimeComponents(int entityID, EcsWorld world, bool isWithFoldout = true)
            {
                using (BeginVertical(UnityEditorUtility.GetTransperentBlackBackgrounStyle()))
                {
                    if (isWithFoldout)
                    {
                        IsShowRuntimeComponents = EditorGUILayout.BeginFoldoutHeaderGroup(IsShowRuntimeComponents, "RUNTIME COMPONENTS", EditorStyles.foldout);
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    }
                    if (isWithFoldout == false || IsShowRuntimeComponents)
                    {
                        if (AddComponentButtons(out Rect dropDownRect))
                        {
                            RuntimeComponentsUtility.GetAddComponentGenericMenu(world).Open(dropDownRect, entityID);
                        }

                        using (SetBackgroundColor(GUI.color.SetAlpha(0.16f)))
                        {
                            GUILayout.Box("", UnityEditorUtility.GetWhiteStyle(), GUILayout.ExpandWidth(true));
                        }
                        IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);

                        if (_componentPoolsBuffer == null)
                        {
                            _componentPoolsBuffer = new List<IEcsPool>(64);
                        }
                        world.GetComponentPoolsFor(entityID, _componentPoolsBuffer);
                        int i = 0;
                        //int iMax = _componentPoolsBuffer.Count;
                        foreach (var componentPool in _componentPoolsBuffer)
                        {
                            DrawRuntimeComponent(entityID, componentPool, 9, i++);
                        }
                    }
                }
            }
            private static void DrawRuntimeComponent(int entityID, IEcsPool pool, int total, int index)
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
                    optionButton.yMax += HeadIconsRect.height;
                    optionButton.xMin = optionButton.xMax - 64;
                    optionButton.center += Vector2.up * padding * 2f;
                    //Canceling isExpanded
                    if (ClickTest(optionButton))
                    {
                        ref bool isExpanded = ref expandMatrix.Down();
                        isExpanded = !isExpanded;
                    }

                    Color panelColor = SelectPanelColor(meta, index, total);

                    using (BeginVertical(panelColor.SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA)))
                    {
                        EditorGUI.BeginChangeCheck();

                        //Close button
                        optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
                        if (CloseButton(optionButton))
                        {
                            pool.Del(entityID);
                            return;
                        }
                        //Edit script button
                        if (ScriptsCache.TryGetScriptAsset(meta, out MonoScript script))
                        {
                            optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            EcsGUI.ScriptAssetButton(optionButton, script);
                        }
                        //Description icon
                        if (string.IsNullOrEmpty(meta.Description.Text) == false)
                        {
                            optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                            DescriptionIcon(optionButton, meta.Description.Text);
                        }

                        RuntimeComponentReflectionCache.FieldInfoData componentInfoData = new RuntimeComponentReflectionCache.FieldInfoData(null, componentType, meta.Name);

                        if (DrawRuntimeData(ref componentInfoData, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData))
                        {
                            pool.SetRaw(entityID, resultData);
                        }
                    }
                }
            }

            #region Default DrawRuntimeData
            [InitializeOnLoadMethod]
            private static void ResetRuntimeComponentReflectionCache()
            {
                _runtimeComponentReflectionCaches.Clear();
            }
            internal class RuntimeComponentReflectionCache
            {
                public readonly Type Type;

                public readonly bool IsUnityObjectType;
                public readonly bool IsUnitySerializable;
                public readonly bool IsUnmanaged;

                public readonly FieldInfoData[] Fields;

                public readonly RefEditorWrapper Wrapper;

                public RuntimeComponentReflectionCache(Type type)
                {
                    Type = type;

                    IsUnmanaged = UnsafeUtility.IsUnmanaged(type);
                    IsUnityObjectType = typeof(UnityObject).IsAssignableFrom(type);
                    IsUnitySerializable = IsUnityObjectType || (!type.IsGenericType && type.IsSerializable);

                    Wrapper = RefEditorWrapper.Take();

                    if (type == typeof(void)) { return; }

                    if (IsUnitySerializable == false)
                    {
                        var fs = type.GetFields(fieldFlags);
                        Fields = new FieldInfoData[fs.Length];
                        for (int i = 0; i < fs.Length; i++)
                        {
                            var f = fs[i];
                            Fields[i] = new FieldInfoData(f);
                        }
                    }
                }
                public readonly struct FieldInfoData
                {
                    public readonly FieldInfo FieldInfo;
                    public readonly Type FieldType;
                    public readonly string UnityFormatName;
                    public readonly bool IsUnityObjectField;
                    public FieldInfoData(FieldInfo fieldInfo)
                    {
                        FieldInfo = fieldInfo;
                        FieldType = fieldInfo.FieldType;
                        IsUnityObjectField = typeof(UnityObject).IsAssignableFrom(fieldInfo.FieldType);
                        UnityFormatName = UnityEditorUtility.TransformFieldName(fieldInfo.Name);
                    }
                    public FieldInfoData(FieldInfo fieldInfo, Type fieldType, string unityFormatName)
                    {
                        FieldInfo = fieldInfo;
                        FieldType = fieldType;
                        UnityFormatName = unityFormatName;
                        IsUnityObjectField = typeof(UnityObject).IsAssignableFrom(fieldType);
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
            private static bool DrawRuntimeData(ref RuntimeComponentReflectionCache.FieldInfoData fieldInfoData, GUIContent label, ExpandMatrix expandMatrix, object data, out object outData)
            {
                outData = data;
                Type type = data == null ? typeof(void) : data.GetType();

                RuntimeComponentReflectionCache cache = GetRuntimeComponentReflectionCache(type);

                bool isUnityObjectField = fieldInfoData.IsUnityObjectField;
                if (isUnityObjectField == false && data == null)
                {
                    EditorGUILayout.TextField(label, "Null");
                    return false;
                }
                bool isUnityObjectType = cache.IsUnityObjectType;

                ref bool isExpanded = ref expandMatrix.Down();
                bool changed = false;


                if (cache.IsUnitySerializable == false)
                {
                    isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, label, EditorStyles.foldout);
                    EditorGUILayout.EndFoldoutHeaderGroup();

                    if (isExpanded)
                    {
                        using (UpIndentLevel())
                        {
                            for (int j = 0, jMax = cache.Fields.Length; j < jMax; j++)
                            {
                                var field = cache.Fields[j];
                                if (DrawRuntimeData(ref field, UnityEditorUtility.GetLabel(field.UnityFormatName), expandMatrix, field.FieldInfo.GetValue(data), out object fieldData))
                                {
                                    field.FieldInfo.SetValue(data, fieldData);
                                    outData = data;
                                    changed = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Type fieldType = fieldInfoData.FieldType;
                    if (isUnityObjectType || isUnityObjectField)
                    {
                        EditorGUI.BeginChangeCheck();
                        var uobj = UnsafeUtility.As<object, UnityObject>(ref data);

                        bool isComponent = typeof(UnityComponent).IsAssignableFrom(fieldType);
                        if (isComponent)
                        {
                            uobj = EditorGUILayout.ObjectField(label, uobj, typeof(UnityObject), true);
                        }
                        else
                        {
                            uobj = EditorGUILayout.ObjectField(label, uobj, fieldType, true);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (isComponent && uobj is GameObject go)
                            {
                                uobj = go.GetComponent(fieldType);
                            }

                            outData = uobj;
                            changed = true;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        //WrapperBase wrapper = RefEditorWrapper.Take(data);

                        RefEditorWrapper wrapper = cache.Wrapper;

                        wrapper.data = data;
                        wrapper.SO.Update();

                        wrapper.IsExpanded = isExpanded;
                        try
                        {
                            EditorGUILayout.PropertyField(wrapper.Property, label, true);
                        }
                        catch (ArgumentException)
                        {
                            if (Event.current.type != EventType.Repaint)
                            {
                                throw;
                            }
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            isExpanded = wrapper.IsExpanded;
                            wrapper.SO.ApplyModifiedProperties();
                            outData = wrapper.Data;
                            changed = true;
                        }
                        //wrapper.Release();
                    }
                }

                expandMatrix.Up();
                return changed;
            }
            #endregion
        }
    }
}
#endif