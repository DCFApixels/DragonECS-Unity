#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class EcsGUI
    {
        public struct ColorScope : IDisposable
        {
            private readonly Color _oldColor;
            public ColorScope(Color color)
            {
                _oldColor = GUI.color;
                GUI.color = color;
            }
            public void Dispose()
            {
                GUI.color = _oldColor;
            }
        }

        public struct ContentColorScope : IDisposable
        {
            private readonly Color _oldColor;
            public ContentColorScope(Color color)
            {
                _oldColor = GUI.contentColor;
                GUI.contentColor = color;
            }
            public void Dispose()
            {
                GUI.contentColor = _oldColor;
            }
        }

        internal readonly static Color GrayColor = new Color32(100, 100, 100, 255);
        internal readonly static Color GreenColor = new Color32(75, 255, 0, 255);
        internal readonly static Color RedColor = new Color32(255, 0, 75, 255);

        private static readonly Rect RemoveButtonRect = new Rect(0f, 0f, 19f, 19f);
        private static readonly Rect TooltipIconRect = new Rect(0f, 0f, 19f, 19f);

        public static float EntityBarHeight => EditorGUIUtility.singleLineHeight + 3f;

        private static bool IsShowHidden
        {
            get { return SettingsPrefs.instance.IsShowHidden; }
            set { SettingsPrefs.instance.IsShowHidden = value; }
        }
        private static bool IsShowRuntimeComponents
        {
            get { return SettingsPrefs.instance.IsShowRuntimeComponents; }
            set { SettingsPrefs.instance.IsShowRuntimeComponents = value; }
        }

        public enum AddClearComponentButton : byte
        {
            None = 0,
            AddComponent,
            Clear,
        }
        [Flags]
        public enum EntityStatus
        {
            NotAlive = 0,
            Alive = 1 << 0,
            Undefined = 1 << 1,
        }

        internal static bool HitTest(Rect rect)
        {
            return HitTest(rect, Event.current.mousePosition);
        }
        internal static bool HitTest(Rect rect, Event evt)
        {
            return HitTest(rect, evt.mousePosition);
        }
        internal static bool HitTest(Rect rect, Vector2 point)
        {
            int offset = 0;
            return HitTest(rect, point, offset);
        }
        internal static bool HitTest(Rect rect, Vector2 point, int offset)
        {
            return point.x >= rect.xMin - (float)offset && point.x < rect.xMax + (float)offset && point.y >= rect.yMin - (float)offset && point.y < rect.yMax + (float)offset;
        }

        public static void DrawIcon(Rect position, Texture icon, float iconPadding, string description)
        {
            GUI.Label(position, UnityEditorUtility.GetLabel(string.Empty, description));
            GUI.DrawTexture(RectUtility.AddPadding(position, iconPadding), icon);
        }
        public static (bool, bool) IconButtonGeneric(Rect position)
        {
            Color dc = GUI.color;
            GUI.color = Color.clear; //Хак чтобы сделать реакцию от курсора мыши без лага
            bool result = GUI.Button(position, "", EditorStyles.miniButtonMid);
            GUI.color = dc;

            var current = Event.current;
            return (GUI.enabled && HitTest(position, current), result);
        }
        public static bool IconButton(Rect position, Texture icon, float iconPadding, string description)
        {
            bool result = GUI.Button(position, UnityEditorUtility.GetLabel(string.Empty));
            DrawIcon(position, icon, iconPadding, description);
            return result;
        }
        public static void DescriptionIcon(Rect position, string description)
        {
            using (new ColorScope(new Color(1f, 1f, 1f, 0.8f)))
            {
                DrawIcon(position, Icons.Instance._helpIcon, 0, description);
            }
        }
        public static bool CloseButton(Rect position)
        {
            using (new ColorScope(new Color(1f, 1f, 1f, 0.8f)))
            {
                var (hover, click) = IconButtonGeneric(position);
                if (hover)
                {
                    DrawIcon(position, Icons.Instance._closeIconOn, -4f, null);
                }
                else
                {
                    DrawIcon(position, Icons.Instance._closeIcon, 0, null);
                }
                return click;
            }
        }
        public static bool AutosetCascadeButton(Rect position)
        {
            return IconButton(position, Icons.Instance._auotsetCascadeIcon, 0f, "Autoset Cascade");
        }
        public static bool AutosetButton(Rect position)
        {
            return IconButton(position, Icons.Instance._auotsetIcon, 1f, "Autoset");
        }
        public static bool UnlinkButton(Rect position)
        {
            return IconButton(position, Icons.Instance._unlinkIcon, 1f, "Unlink Entity");
        }
        public static bool DelEntityButton(Rect position)
        {
            return IconButton(position, Icons.Instance._closeIcon, 0f, "Delete Entity");
        }
        public static void EntityBar(Rect position, EntityStatus status, int id, short gen, short world)
        {
            var (entityInfoRect, statusRect) = RectUtility.VerticalSliceBottom(position, 3f);

            Color w = Color.gray;
            w.a = 0.6f;
            Color b = Color.black;
            b.a = 0.55f;
            EditorGUI.DrawRect(entityInfoRect, w);

            var (idRect, genWorldRect) = RectUtility.HorizontalSliceLerp(entityInfoRect, 0.4f);
            var (genRect, worldRect) = RectUtility.HorizontalSliceLerp(genWorldRect, 0.5f);

            idRect = RectUtility.AddPadding(idRect, 2, 1, 0, 0);
            genRect = RectUtility.AddPadding(genRect, 1, 1, 0, 0);
            worldRect = RectUtility.AddPadding(worldRect, 1, 2, 0, 0);
            EditorGUI.DrawRect(idRect, b);
            EditorGUI.DrawRect(genRect, b);
            EditorGUI.DrawRect(worldRect, b);


            GUIStyle style = new GUIStyle(EditorStyles.numberField);
            style.alignment = TextAnchor.MiddleCenter;
            style.font = EditorStyles.boldFont;
            if (status == EntityStatus.Alive)
            {
                Color statusColor = EcsGUI.GreenColor;
                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                EditorGUI.IntField(idRect, id, style);
                EditorGUI.IntField(genRect, gen, style);
                EditorGUI.IntField(worldRect, world, style);
            }
            else
            {
                Color statusColor = status == EntityStatus.Undefined ? new Color32(200, 200, 200, 255) : EcsGUI.RedColor;
                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(idRect, "Entity ID", style);
                    GUI.Label(genRect, "Generation", style);
                    GUI.Label(worldRect, "World ID", style);
                }
            }
        }

        public static bool AddComponentButtons(Rect position)
        {
            position = RectUtility.AddPadding(position, 20f, 20f, 12f, 2f);
            return GUI.Button(position, "Add Component");
        }
        public static AddClearComponentButton AddClearComponentButtons(Rect position)
        {
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
            public static void DrawWorldBaseInfo(EcsWorld world)
            {
                bool isNull = world == null || world.IsDestroyed || world.id == 0;
                int entitesCount = isNull ? 0 : world.Count;
                int capacity = isNull ? 0 : world.Capacity;
                int leakedEntitesCount = isNull ? 0 : world.CountLeakedEntitesDebug();
                EditorGUILayout.IntField("Entities", entitesCount, EditorStyles.boldLabel);
                EditorGUILayout.IntField("Capacity", capacity, EditorStyles.boldLabel);
                Color color = leakedEntitesCount > 0 ? Color.yellow : GUI.contentColor;
                using (new ContentColorScope(color))
                {
                    EditorGUILayout.IntField("Leaked Entites", leakedEntitesCount, EditorStyles.boldLabel);
                }
            }
            public static void EntityBar(EntityStatus status, int id, short gen, short world)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityBar(GUILayoutUtility.GetRect(width, height), status, id, gen, world);
            }
            public static bool AddComponentButtons()
            {
                return EcsGUI.AddComponentButtons(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 36f));
            }
            public static AddClearComponentButton AddClearComponentButtons()
            {
                return EcsGUI.AddClearComponentButtons(GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 36f));
            }
            public static void DrawRuntimeComponents(entlong entity, bool isWithFoldout = true)
            {
                if (entity.TryUnpackForUnityEditor(out int entityID, out _, out _, out EcsWorld world))
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
                    if (EcsGUI.Layout.AddComponentButtons())
                    {
                        GenericMenu genericMenu = RuntimeComponentsUtility.GetAddComponentGenericMenu(world);
                        RuntimeComponentsUtility.CurrentEntityID = entityID;
                        genericMenu.ShowAsContext();
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
                    if (EcsGUI.CloseButton(removeButtonRect))
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

                    if (string.IsNullOrEmpty(meta.Description.Text) == false)
                    {
                        Rect tooltipIconRect = TooltipIconRect;
                        tooltipIconRect.center = removeButtonRect.center;
                        tooltipIconRect.center -= Vector2.right * tooltipIconRect.width;
                        EcsGUI.DescriptionIcon(tooltipIconRect, meta.Description.Text);
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
