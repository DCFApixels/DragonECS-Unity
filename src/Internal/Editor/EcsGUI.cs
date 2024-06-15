#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static PlasticGui.WebApi.Responses.CloudOrganizationHelpActionsResponse;
using UnityComponent = UnityEngine.Component;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class EcsGUI
    {
        #region Scores
        public struct LabelWidthScore : IDisposable
        {
            private readonly float _value;
            public LabelWidthScore(float value)
            {
                _value = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = value;
            }
            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _value;
            }
        }
        public struct ColorScope : IDisposable
        {
            private readonly Color _value;
            public ColorScope(float r, float g, float b, float a = 1f) : this(new Color(r, g, b, a)) { }
            public ColorScope(Color value)
            {
                _value = GUI.color;
                GUI.color = value;
            }
            public void Dispose()
            {
                GUI.color = _value;
            }
        }
        public struct ContentColorScope : IDisposable
        {
            private readonly Color _value;
            public ContentColorScope(float r, float g, float b, float a = 1f) : this(new Color(r, g, b, a)) { }
            public ContentColorScope(Color value)
            {
                _value = GUI.contentColor;
                GUI.contentColor = value;
            }
            public void Dispose()
            {
                GUI.contentColor = _value;
            }
        }
        public struct BackgroundColorScope : IDisposable
        {
            private readonly Color _value;
            public BackgroundColorScope(float r, float g, float b, float a = 1f) : this(new Color(r, g, b, a)) { }
            public BackgroundColorScope(Color value)
            {
                _value = GUI.backgroundColor;
                GUI.backgroundColor = value;
            }
            public void Dispose()
            {
                GUI.backgroundColor = _value;
            }
        }
        public struct IndentLevelScope : IDisposable
        {
            private readonly int _value;
            public IndentLevelScope(int value)
            {
                _value = EditorGUI.indentLevel;
                EditorGUI.indentLevel = value;
            }
            public void Dispose()
            {
                EditorGUI.indentLevel = _value;
            }
        }
        public struct AlignmentScope : IDisposable
        {
            private readonly GUIStyle _target;
            private readonly TextAnchor _value;
            public AlignmentScope(GUIStyle target, TextAnchor value)
            {
                _target = target;
                _value = _target.alignment;
                _target.alignment = value;
            }
            public void Dispose()
            {
                _target.alignment = _value;
            }
        }
        #endregion

        public static AlignmentScope SetAlignment(GUIStyle target, TextAnchor value) => new AlignmentScope(target, value);
        public static IndentLevelScope SetIndentLevel(int level) => new IndentLevelScope(level);
        public static ContentColorScope SetContentColor(Color value) => new ContentColorScope(value);
        public static ContentColorScope SetContentColor(float r, float g, float b, float a = 1f) => new ContentColorScope(r, g, b, a);
        public static BackgroundColorScope SetBackgroundColor(Color value) => new BackgroundColorScope(value);
        public static BackgroundColorScope SetBackgroundColor(float r, float g, float b, float a = 1f) => new BackgroundColorScope(r, g, b, a);
        public static ColorScope SetColor(Color value) => new ColorScope(value);
        public static ColorScope SetColor(float r, float g, float b, float a = 1f) => new ColorScope(r, g, b, a);
        public static ColorScope SetAlpha(float a) => new ColorScope(GUI.color * new Color(1, 1, 1, a));
        public static EditorGUI.DisabledScope Enable => new EditorGUI.DisabledScope(false);
        public static EditorGUI.DisabledScope Disable => new EditorGUI.DisabledScope(true);
        public static EditorGUI.DisabledScope SetEnable(bool value) => new EditorGUI.DisabledScope(!value);



        private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal readonly static Color GrayColor = new Color32(100, 100, 100, 255);
        internal readonly static Color GreenColor = new Color32(75, 255, 0, 255);
        internal readonly static Color RedColor = new Color32(255, 0, 75, 255);

        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        public static float EntityBarHeight => EditorGUIUtility.singleLineHeight + 3f;

        #region Properties
        private static ComponentColorMode AutoColorMode
        {
            get { return SettingsPrefs.instance.ComponentColorMode; }
            set { SettingsPrefs.instance.ComponentColorMode = value; }
        }
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
        #endregion

        #region enums
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
        #endregion

        #region HitTest
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
        #endregion

        #region small elems
        public static void DrawIcon(Rect position, Texture icon, float iconPadding, string description)
        {
            if (position.width != position.height)
            {
                Vector2 center = position.center;
                float size = Mathf.Min(position.width, position.height);
                position.height = size;
                position.width = size;
                position.center = center;
            }
            using (SetColor(GUI.enabled ? GUI.color : GUI.color * new Color(1f, 1f, 1f, 0.4f)))
            {
                GUI.Label(position, UnityEditorUtility.GetLabel(string.Empty, description));
                GUI.DrawTexture(RectUtility.AddPadding(position, iconPadding), icon);
            }
        }
        public static (bool, bool) IconButtonGeneric(Rect position)
        {
            using (SetAlpha(0))
            {
                bool result = GUI.Button(position, string.Empty, EditorStyles.miniButtonMid);
                var current = Event.current;
                return (GUI.enabled && HitTest(position, current), result);
            }
        }
        public static bool IconHoverScan(Rect position, Event current)
        {
            using (Disable) using (SetAlpha(0))
            {
                GUI.Button(position, string.Empty, EditorStyles.miniButtonMid);
                return HitTest(position, current);
            }
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
                DrawIcon(position, Icons.Instance.HelpIcon, 0, description);
            }
        }
        public static void ScriptAssetButton(Rect position, MonoScript script)
        {
            var current = Event.current;

            var hover = IconHoverScan(position, current);

            using (new ColorScope(new Color(1f, 1f, 1f, hover ? 1f : 0.8f)))
            {
                DrawIcon(position, Icons.Instance.FileIcon, hover ? 1f : 2f, "One click - Ping File. Double click - Edit Script");
            }

            if (hover)
            {
                if (current.type == EventType.MouseUp)
                {
                    EditorGUIUtility.PingObject(script);
                }
                else if (current.type == EventType.MouseDown && current.clickCount >= 2)
                {
                    AssetDatabase.OpenAsset(script);
                }
            }
        }
        public static bool CloseButton(Rect position, string description = null)
        {
            using (new ColorScope(new Color(1f, 1f, 1f, 0.8f)))
            {
                var (hover, click) = IconButtonGeneric(position);
                if (hover)
                {
                    DrawIcon(position, Icons.Instance.CloseIconOn, -4f, description);
                }
                else
                {
                    DrawIcon(position, Icons.Instance.CloseIcon, 0, description);
                }
                return click;
            }
        }
        public static bool AutosetCascadeButton(Rect position)
        {
            return IconButton(position, Icons.Instance.AutosetCascadeIcon, 0f, "Autoset Cascade");
        }
        public static bool AutosetButton(Rect position)
        {
            return IconButton(position, Icons.Instance.AuotsetIcon, 1f, "Autoset");
        }
        public static bool UnlinkButton(Rect position)
        {
            return IconButton(position, Icons.Instance.UnlinkIcon, 1f, "Unlink Entity");
        }
        public static bool DelEntityButton(Rect position)
        {
            return IconButton(position, Icons.Instance.CloseIcon, 0f, "Delete Entity");
        }
        #endregion

        #region entity bar
        public static void EntityBarForAlive(Rect position, EntityStatus status, int id, short gen, short world)
        {
            EntityBar(position, status != EntityStatus.Alive, status, id, gen, world);
        }
        public static void EntityBar(Rect position, int id, short gen, short world)
        {
            EntityBar_Internal(position, false, id, gen, world);
        }
        public static void EntityBar(Rect position)
        {
            EntityBar_Internal(position, true);
        }
        public static void EntityBar(Rect position, bool isPlaceholder, EntityStatus status, int id = 0, short gen = 0, short world = 0)
        {
            using (new LabelWidthScore(0f))
            {
                var (entityInfoRect, statusRect) = RectUtility.VerticalSliceBottom(position, 3f);

                Color statusColor;
                switch (status)
                {
                    case EntityStatus.NotAlive:
                        statusColor = EcsGUI.RedColor;
                        break;
                    case EntityStatus.Alive:
                        statusColor = EcsGUI.GreenColor;
                        break;
                    default:
                        statusColor = new Color32(200, 200, 200, 255);
                        break;
                }

                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                EntityBar_Internal(entityInfoRect, isPlaceholder, id, gen, world);
            }
        }
        private static void EntityBar_Internal(Rect position, bool isPlaceHolder, int id = 0, short gen = 0, short world = 0)
        {
            using (new LabelWidthScore(0f))
            {
                Color w = Color.gray;
                w.a = 0.6f;
                Color b = Color.black;
                b.a = 0.55f;
                EditorGUI.DrawRect(position, w);

                var (idRect, genWorldRect) = RectUtility.HorizontalSliceLerp(position, 0.4f);
                var (genRect, worldRect) = RectUtility.HorizontalSliceLerp(genWorldRect, 0.5f);

                idRect = RectUtility.AddPadding(idRect, 2, 1, 0, 0);
                genRect = RectUtility.AddPadding(genRect, 1, 1, 0, 0);
                worldRect = RectUtility.AddPadding(worldRect, 1, 2, 0, 0);
                EditorGUI.DrawRect(idRect, b);
                EditorGUI.DrawRect(genRect, b);
                EditorGUI.DrawRect(worldRect, b);

                GUIStyle style = UnityEditorUtility.GetInputFieldCenterAnhor();

                if (isPlaceHolder)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUI.Label(idRect, "Entity ID", style);
                        GUI.Label(genRect, "Generation", style);
                        GUI.Label(worldRect, "World ID", style);
                    }
                }
                else
                {
                    EditorGUI.IntField(idRect, id, style);
                    EditorGUI.IntField(genRect, gen, style);
                    EditorGUI.IntField(worldRect, world, style);
                }
            }
        }
        #endregion

        internal static int GetChildPropertiesCount(SerializedProperty property, Type type, out bool isEmpty)
        {
            int result = GetChildPropertiesCount(property);
            isEmpty = result <= 0 && type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;
            return result;
        }
        internal static int GetChildPropertiesCount(SerializedProperty property)
        {
            var propsCounter = property.Copy();
            int lastDepth = propsCounter.depth;
            bool next = propsCounter.Next(true) && lastDepth < propsCounter.depth;
            int result = 0;
            while (next)
            {
                result++;
                next = propsCounter.Next(true);
            }
            return result;
        }
        public static Color SelectPanelColor(ITypeMeta meta, int index, int total)
        {
            var trueMeta = meta.Type.ToMeta();
            bool isCustomColor = trueMeta.IsCustomColor || meta.Color != trueMeta.Color;
            return SelectPanelColor(meta.Color, isCustomColor, index, total);
        }
        public static Color SelectPanelColor(MetaColor color, bool isCustomColor, int index, int total)
        {
            if (isCustomColor)
            {
                return color.ToUnityColor();
            }
            else
            {
                switch (AutoColorMode)
                {
                    case ComponentColorMode.Auto:
                        return color.ToUnityColor().Desaturate(0.48f) / 1.18f; //.Desaturate(0.48f) / 1.18f;
                    case ComponentColorMode.Rainbow:
                        int localTotal = Mathf.Max(total, EscEditorConsts.AUTO_COLOR_RAINBOW_MIN_RANGE);
                        Color hsv = Color.HSVToRGB(1f / localTotal * (index % localTotal), 1, 1);
                        return hsv.Desaturate(0.48f) / 1.18f;
                    default:
                        return index % 2 == 0 ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.54f, 0.54f, 0.54f);
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

        public static void DrawEmptyComponentProperty(Rect position, SerializedProperty property, string name, bool isDisplayEmpty)
        {
            DrawEmptyComponentProperty(position, property, UnityEditorUtility.GetLabel(name), isDisplayEmpty);
        }
        public static void DrawEmptyComponentProperty(Rect position, SerializedProperty property, GUIContent label, bool isDisplayEmpty)
        {
            EditorGUI.LabelField(position, label);
            if (isDisplayEmpty)
            {
                using (SetContentColor(1f, 1f, 1f, 0.4f))
                {
                    GUI.Label(position.AddPadding(EditorGUIUtility.labelWidth, 0, 0, 0), "empty");
                }
            }
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.EndProperty();
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public static class Layout
        {
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
                if(style == null)
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
                    if (AddComponentButtons())
                    {
                        GenericMenu genericMenu = RuntimeComponentsUtility.GetAddComponentGenericMenu(world);
                        RuntimeComponentsUtility.CurrentEntityID = entityID;
                        genericMenu.ShowAsContext();
                    }

                    GUILayout.Box("", UnityEditorUtility.GetStyle(GUI.color, 0.16f), GUILayout.ExpandWidth(true));
                    IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);

                    int i = 0;
                    foreach (var componentTypeID in componentTypeIDs)
                    {
                        var pool = world.GetPoolInstance(componentTypeID);
                        {
                            DrawRuntimeComponent(componentTypeIDs.Length, i++, entityID, pool);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            private static void DrawRuntimeComponent(int total, int index, int entityID, IEcsPool pool)
            {
                var meta = pool.ComponentType.ToMeta();
                if (meta.IsHidden == false || IsShowHidden)
                {
                    object data = pool.GetRaw(entityID);

                    Color panelColor = SelectPanelColor(meta, index, total).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

                    float padding = EditorGUIUtility.standardVerticalSpacing;
                    Rect optionButton = GUILayoutUtility.GetLastRect();

                    GUILayout.BeginVertical(UnityEditorUtility.GetStyle(panelColor, EscEditorConsts.COMPONENT_DRAWER_ALPHA));
                    EditorGUI.BeginChangeCheck();

                    bool isRemoveComponent = false;
                    optionButton.yMin = optionButton.yMax;
                    optionButton.yMax += HeadIconsRect.height;
                    optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
                    optionButton.center += Vector2.up * padding * 2f;
                    if (CloseButton(optionButton))
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


                    if (UnityEditorUtility.TryGetScriptAsset(componentType, out MonoScript script))
                    {
                        optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                        EcsGUI.ScriptAssetButton(optionButton, script);
                    }
                    if (string.IsNullOrEmpty(meta.Description.Text) == false)
                    {
                        optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                        DescriptionIcon(optionButton, meta.Description.Text);
                    }

                    GUILayout.EndVertical();
                }
            }

            private static bool DrawRuntimeData(Type fieldType, GUIContent label, ExpandMatrix expandMatrix, object data, out object outData)
            {
                outData = data;
                Type type = data == null ? typeof(void) : data.GetType();

                bool isUnityObject = typeof(UnityObject).IsAssignableFrom(fieldType);

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
                        var uobj = (UnityObject)data;

                        bool isComponent = typeof(UnityComponent).IsAssignableFrom(fieldType);
                        if (isComponent)
                        {
                            uobj = EditorGUILayout.ObjectField(label, uobj, typeof(UnityObject), true);
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
