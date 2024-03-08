#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class EcsGUI
    {
        internal readonly static Color GrayColor = new Color32(100, 100, 100, 255);
        internal readonly static Color GreenColor = new Color32(75, 255, 0, 255);
        internal readonly static Color RedColor = new Color32(255, 0, 75, 255);

        private static readonly Rect RemoveButtonRect = new Rect(0f, 0f, 17f, 19f);
        private static readonly Rect TooltipIconRect = new Rect(0f, 0f, 21f, 15f);

        private static bool IsShowHidden
        {
            get { return DebugMonitorPrefs.instance.IsShowHidden; }
            set { DebugMonitorPrefs.instance.IsShowHidden = value; }
        }
        private static bool IsShowRuntimeComponents
        {
            get { return DebugMonitorPrefs.instance.IsShowRuntimeComponents; }
            set { DebugMonitorPrefs.instance.IsShowRuntimeComponents = value; }
        }

        public enum AddClearComponentButton : byte
        {
            None = 0,
            AddComponent,
            Clear,
        }

        //private static GUILayoutOption[] _defaultParams;
        //private static bool _isInit = false;
        //private static void Init()
        //{
        //    if (_isInit)
        //    {
        //        return;
        //    }
        //    _defaultParams = new GUILayoutOption[] { GUILayout.ExpandWidth(true) };
        //    _isInit = true;
        //}

        public static AddClearComponentButton AddClearComponentButtons(Rect position)
        {
            //Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 36f);
            position = RectUtility.AddPadding(position, 20f, 20f, 12f, 2f);
            var (left, right) = RectUtility.HorizontalSliceLerp(position, 0.75f);

            if (GUI.Button(left, "Add Component"))
            {
                return AddClearComponentButton.AddComponent;
            }
            if (GUI.Button(right, "Clear"))
            {
                return AddClearComponentButton.Clear;
            }
            return AddClearComponentButton.None;
        }

        public static class Layout
        {

            public static AddClearComponentButton AddClearComponentButtons()
            {
                return EcsGUI.AddClearComponentButtons(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 36f));
            }
            public static void DrawRuntimeComponents(entlong entity, bool isWithFoldout = true)
            {
                if (entity.TryUnpack(out int entityID, out EcsWorld world))
                {
                    DrawRuntimeComponents(entityID, world, isWithFoldout);
                }
            }
            public static void DrawRuntimeComponents(int entityID, EcsWorld world, bool isWithFoldout = true)
            {
                var componentTypeIDs = world.GetComponentTypeIDsFor(entityID);

                GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));

                if (isWithFoldout)
                {
                    IsShowRuntimeComponents = EditorGUILayout.BeginFoldoutHeaderGroup(IsShowRuntimeComponents, "RUNTIME COMPONENTS", EditorStyles.foldout);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                if (isWithFoldout == false || IsShowRuntimeComponents)
                {
                    switch (EcsGUI.Layout.AddClearComponentButtons())
                    {
                        case AddClearComponentButton.AddComponent:
                            GenericMenu genericMenu = RuntimeComponentsUtility.GetAddComponentGenericMenu(world);
                            RuntimeComponentsUtility.CurrentEntityID = entityID;
                            genericMenu.ShowAsContext();
                            break;
                        case AddClearComponentButton.Clear:
                            break;
                    }


                    GUILayout.Box("", UnityEditorUtility.GetStyle(GUI.color, 0.16f), GUILayout.ExpandWidth(true));
                    IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);

                    foreach (var componentTypeID in componentTypeIDs)
                    {
                        var pool = world.GetPoolInstance(componentTypeID);
                        {
                            DrawRuntimeComponent(entityID, pool);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            private static void DrawRuntimeComponent(int entityID, IEcsPool pool)
            {
                var meta = pool.ComponentType.ToMeta();
                if (meta.IsHidden == false || IsShowHidden)
                {
                    object data = pool.GetRaw(entityID);
                    Color panelColor = meta.Color.ToUnityColor().Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

                    float padding = EditorGUIUtility.standardVerticalSpacing;
                    Rect removeButtonRect = GUILayoutUtility.GetLastRect();

                    GUILayout.BeginVertical(UnityEditorUtility.GetStyle(panelColor, EscEditorConsts.COMPONENT_DRAWER_ALPHA));
                    EditorGUI.BeginChangeCheck();

                    bool isRemoveComponent = false;
                    removeButtonRect.yMin = removeButtonRect.yMax;
                    removeButtonRect.yMax += RemoveButtonRect.height;
                    removeButtonRect.xMin = removeButtonRect.xMax - RemoveButtonRect.width;
                    removeButtonRect.center += Vector2.up * padding * 2f;
                    if (GUI.Button(removeButtonRect, "x"))
                    {
                        isRemoveComponent = true;
                    }

                    Type componentType = pool.ComponentType;
                    ExpandMatrix expandMatrix = ExpandMatrix.Take(componentType);
                    bool changed = DrawRuntimeData(componentType, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData);
                    if (changed || isRemoveComponent)
                    {
                        if (isRemoveComponent)
                        {
                            pool.Del(entityID);
                        }
                        else
                        {
                            pool.SetRaw(entityID, resultData);
                        }
                    }

                    GUILayout.EndVertical();
                }
            }

            private static bool DrawRuntimeData(Type fieldType, GUIContent label, ExpandMatrix expandMatrix, object data, out object outData)
            {   
                outData = data;
                Type type = data == null ? typeof(void) : data.GetType();
 
                bool isUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(fieldType);

                if (isUnityObject == false && data == null)
                {
                    EditorGUILayout.TextField(label, "Null");
                    return false;
                }
                ref bool isExpanded = ref expandMatrix.Down();
                bool changed = false;
                outData = data;

                if (isUnityObject == false && (type.IsGenericType || !type.IsSerializable))
                {
                    isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(isExpanded, label, EditorStyles.foldout);
                    EditorGUILayout.EndFoldoutHeaderGroup();

                    if (isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var field in type.GetFields(fieldFlags))
                        {
                            GUIContent subLabel = UnityEditorUtility.GetLabel(UnityEditorUtility.TransformFieldName(field.Name));
                            if (DrawRuntimeData(field.FieldType, subLabel, expandMatrix, field.GetValue(data), out object fieldData))
                            {
                                field.SetValue(data, fieldData);
                                outData = data;
                                changed = true;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (isUnityObject)
                    {
                        EditorGUI.BeginChangeCheck();
                        var uobj = (UnityEngine.Object)data;

                        bool isComponent = (typeof(UnityEngine.Component)).IsAssignableFrom(fieldType);
                        if (isComponent)
                        {
                            uobj = EditorGUILayout.ObjectField(label, uobj, typeof(UnityEngine.Object), true);
                        }
                        else
                        {
                            uobj = EditorGUILayout.ObjectField(label, uobj, fieldType, true);
                        }
                        if (isComponent && uobj is GameObject go)
                        {
                            uobj = go.GetComponent(fieldType);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            outData = uobj;
                            changed = true;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        WrapperBase w = RefEditorWrapper.Take(data);

                        w.IsExpanded = isExpanded;
                        EditorGUILayout.PropertyField(w.Property, label, true);
                        isExpanded = w.IsExpanded;

                        if (EditorGUI.EndChangeCheck())
                        {
                            w.SO.ApplyModifiedProperties();
                            outData = w.Data;
                            changed = true;
                        }
                        w.Release();
                    }
                }

                expandMatrix.Up();
                return changed;
            }
        }
    }
}
#endif
