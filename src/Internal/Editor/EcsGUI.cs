#if UNITY_EDITOR
using DCFApixels.DragonECS.Core;
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Color = UnityEngine.Color;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static partial class EcsGUI
    {
        #region Scores
        private static int _changedCounter = 0;
        private static bool _changed = false;
        private static bool _delayedChanged = false;

        public static int ChangedCounter
        {
            get { return _changedCounter; }
        }
        public static bool Changed
        {
            get
            {
                _changed = _changed || GUI.changed;
                GUI.changed = _changed;
                return _changed;
            }
            set
            {
                _changed = Changed || value;
                GUI.changed = _changed;
            }
        }
        public static bool DelayedChanged
        {
            get
            {
                return _delayedChanged;
            }
            set
            {
                _delayedChanged = DelayedChanged || value;
                Changed = _delayedChanged;
            }
        }
        public readonly struct CheckChangedScope : IDisposable
        {
            private readonly bool _value;
            public CheckChangedScope(bool value)
            {
                _value = value;
                _changedCounter++;
                _changed = false;
            }
            public static CheckChangedScope New() { return new CheckChangedScope(Changed); }
            public void Dispose()
            {
                Changed = Changed || _value;
                _changedCounter--;
                //if(_changedCounter <= 0 && Event.current.type == EventType.Repaint)
                if (_changedCounter <= 0)
                {
                    _changedCounter = 0;
                    _changed = _delayedChanged;
                    _delayedChanged = false;
                }
            }
        }
        public readonly struct CheckChangedScopeWithAutoApply : IDisposable
        {
            private readonly CheckChangedScope _scope;
            private readonly SerializedObject _serializedObject;
            public CheckChangedScopeWithAutoApply(SerializedObject serializedObject)
            {
                _scope = CheckChangedScope.New();
                _serializedObject = serializedObject;
            }
            public void Dispose()
            {
                if (Changed)
                {
                    _serializedObject.ApplyModifiedProperties();
                }
                _scope.Dispose();
            }
        }
        public struct ScrollViewScope : IDisposable
        {
            public ScrollViewScope(Rect position, ref Vector2 pos, Rect viewRect) { pos = GUI.BeginScrollView(position, pos, viewRect); }
            public void Dispose() { GUI.EndScrollView(); }
        }
        public readonly struct LabelWidthScope : IDisposable
        {
            private readonly float _value;
            public LabelWidthScope(float value)
            {
                _value = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = value;
            }
            public void Dispose() { EditorGUIUtility.labelWidth = _value; }
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
            public void Dispose() { GUI.color = _value; }
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
            public void Dispose() { GUI.contentColor = _value; }
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
            public void Dispose() { GUI.backgroundColor = _value; }
        }
        public struct IndentLevelScope : IDisposable
        {
            private readonly int _value;
            public IndentLevelScope(int value)
            {
                _value = EditorGUI.indentLevel;
                EditorGUI.indentLevel = value;
            }
            public void Dispose() { EditorGUI.indentLevel = _value; }
        }
        public struct AlignmentScope : IDisposable
        {
            public readonly GUIStyle Target;
            private readonly TextAnchor _value;
            public AlignmentScope(GUIStyle target, TextAnchor value)
            {
                Target = target;
                _value = Target.alignment;
                Target.alignment = value;
            }
            public AlignmentScope(GUIStyle target)
            {
                Target = target;
                _value = Target.alignment;
            }
            public void Dispose() { Target.alignment = _value; }
        }
        public struct FontSizeScope : IDisposable
        {
            public readonly GUIStyle Target;
            private readonly int _value;
            public FontSizeScope(GUIStyle target, int value)
            {
                Target = target;
                _value = Target.fontSize;
                Target.fontSize = value;
            }
            public FontSizeScope(GUIStyle target)
            {
                Target = target;
                _value = Target.fontSize;
            }
            public void Dispose() { Target.fontSize = _value; }
        }
        public struct FontStyleScope : IDisposable
        {
            public readonly GUIStyle Target;
            private readonly FontStyle _value;
            public FontStyleScope(GUIStyle target, FontStyle value)
            {
                Target = target;
                _value = Target.fontStyle;
                Target.fontStyle = value;
            }
            public FontStyleScope(GUIStyle target)
            {
                Target = target;
                _value = Target.fontStyle;
            }
            public void Dispose() { Target.fontStyle = _value; }
        }


        public static partial class Layout
        {
            public struct VerticalScope : IDisposable
            {
                public VerticalScope(GUILayoutOption[] options) { GUILayout.BeginVertical(options); }
                public VerticalScope(GUIStyle style, GUILayoutOption[] options) { GUILayout.BeginVertical(style, options); }
                public VerticalScope(Color backgroundColor, GUILayoutOption[] options)
                {
                    using (SetColor(backgroundColor))
                    {
                        GUILayout.BeginVertical(UnityEditorUtility.GetWhiteStyle(), options);
                    }
                }
                public void Dispose() { GUILayout.EndVertical(); }
            }
            public struct HorizontalScope : IDisposable
            {
                public HorizontalScope(GUILayoutOption[] options) { GUILayout.BeginHorizontal(options); }
                public HorizontalScope(GUIStyle style, GUILayoutOption[] options) { GUILayout.BeginHorizontal(style, options); }
                public HorizontalScope(Color backgroundColor, GUILayoutOption[] options)
                {
                    using (SetColor(backgroundColor))
                    {
                        GUILayout.BeginHorizontal(UnityEditorUtility.GetWhiteStyle(), options);
                    }
                }
                public void Dispose() { GUILayout.EndHorizontal(); }
            }
            public struct ScrollViewScope : IDisposable
            {
                public ScrollViewScope(ref Vector2 pos, GUILayoutOption[] options) { pos = GUILayout.BeginScrollView(pos, options); }
                public ScrollViewScope(ref Vector2 pos, GUIStyle style, GUILayoutOption[] options) { pos = GUILayout.BeginScrollView(pos, style, options); }
                public void Dispose() { GUILayout.EndScrollView(); }
            }

            public static ScrollViewScope BeginScrollView(ref Vector2 pos) => new ScrollViewScope(ref pos, Array.Empty<GUILayoutOption>());
            public static ScrollViewScope BeginScrollView(ref Vector2 pos, params GUILayoutOption[] options) => new ScrollViewScope(ref pos, options);
            public static ScrollViewScope BeginScrollView(ref Vector2 pos, GUIStyle style, params GUILayoutOption[] options) => new ScrollViewScope(ref pos, style, options);
            public static HorizontalScope BeginHorizontal() => new HorizontalScope(Array.Empty<GUILayoutOption>());
            public static HorizontalScope BeginHorizontal(params GUILayoutOption[] options) => new HorizontalScope(options);
            public static HorizontalScope BeginHorizontal(GUIStyle style, params GUILayoutOption[] options) => new HorizontalScope(style, options);
            public static HorizontalScope BeginHorizontal(Color backgroundColor, params GUILayoutOption[] options) => new HorizontalScope(backgroundColor, options);
            public static VerticalScope BeginVertical() => new VerticalScope(Array.Empty<GUILayoutOption>());
            public static VerticalScope BeginVertical(params GUILayoutOption[] options) => new VerticalScope(options);
            public static VerticalScope BeginVertical(GUIStyle style, params GUILayoutOption[] options) => new VerticalScope(style, options);
            public static VerticalScope BeginVertical(Color backgroundColor, params GUILayoutOption[] options) => new VerticalScope(backgroundColor, options);
        }
        public static CheckChangedScope CheckChanged() => CheckChangedScope.New();
        public static CheckChangedScopeWithAutoApply CheckChanged(SerializedObject serializedObject) => new CheckChangedScopeWithAutoApply(serializedObject);
        public static ScrollViewScope BeginScrollView(Rect position, ref Vector2 pos, Rect viewRect) => new ScrollViewScope(position, ref pos, viewRect);
        public static FontStyleScope SetFontStyle(GUIStyle target, FontStyle value) => new FontStyleScope(target, value);
        public static FontStyleScope SetFontStyle(FontStyle value) => new FontStyleScope(GUI.skin.label, value);
        public static FontStyleScope SetFontStyle(GUIStyle target) => new FontStyleScope(target);
        public static FontSizeScope SetFontSize(GUIStyle target, int value) => new FontSizeScope(target, value);
        public static FontSizeScope SetFontSize(int value) => new FontSizeScope(GUI.skin.label, value);
        public static FontSizeScope SetFontSize(GUIStyle target) => new FontSizeScope(target);
        public static AlignmentScope SetAlignment(GUIStyle target, TextAnchor value) => new AlignmentScope(target, value);
        public static AlignmentScope SetAlignment(TextAnchor value) => new AlignmentScope(GUI.skin.label, value);
        public static AlignmentScope SetAlignment(GUIStyle target) => new AlignmentScope(target);
        public static IndentLevelScope SetIndentLevel(int level) => new IndentLevelScope(level);
        public static IndentLevelScope UpIndentLevel() => new IndentLevelScope(EditorGUI.indentLevel + 1);
        public static ContentColorScope SetContentColor(Color value) => new ContentColorScope(value);
        public static ContentColorScope SetContentColor(Color value, float a) => new ContentColorScope(value.r, value.g, value.b, a);
        public static ContentColorScope SetContentColor(float r, float g, float b, float a = 1f) => new ContentColorScope(r, g, b, a);
        public static BackgroundColorScope SetBackgroundColor(Color value) => new BackgroundColorScope(value);
        public static BackgroundColorScope SetBackgroundColor(Color value, float a) => new BackgroundColorScope(value.r, value.g, value.b, a);
        public static BackgroundColorScope SetBackgroundColor(float r, float g, float b, float a = 1f) => new BackgroundColorScope(r, g, b, a);
        public static ColorScope SetColor(Color value) => new ColorScope(value);
        public static ColorScope SetColor(Color value, float a) => new ColorScope(value.a, value.g, value.b, a);
        public static ColorScope SetColor(float r, float g, float b, float a = 1f) => new ColorScope(r, g, b, a);
        public static ColorScope SetAlpha(float a) => new ColorScope(GUI.color * new Color(1, 1, 1, a));
        public static EditorGUI.DisabledScope Enable => new EditorGUI.DisabledScope(false);
        public static EditorGUI.DisabledScope Disable => new EditorGUI.DisabledScope(true);
        public static EditorGUI.DisabledScope SetEnable(bool value) => new EditorGUI.DisabledScope(!value);
        public static LabelWidthScope SetLabelWidth(float value) => new LabelWidthScope(value);
        #endregion

        private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal readonly static Color GrayColor = new Color32(100, 100, 100, 255);
        internal readonly static Color GreenColor = new Color32(75, 255, 0, 255);
        internal readonly static Color RedColor = new Color32(255, 0, 75, 255);

        internal static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        public static float EntityBarHeight => EditorGUIUtility.singleLineHeight + 3f;

        private static float indent => (float)EditorGUI.indentLevel * 15f;
        private static float indentLevel => EditorGUI.indentLevel;

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
        //private static bool IsFastModeRuntimeComponents
        //{
        //    get { return UserSettingsPrefs.instance.IsFastModeRuntimeComponents; }
        //    set { UserSettingsPrefs.instance.IsFastModeRuntimeComponents = value; }
        //}
        private static float OneLineHeight
        {
            get => EditorGUIUtility.singleLineHeight;
        }
        private static float Spacing
        {
            get => EditorGUIUtility.standardVerticalSpacing;
        }
        #endregion

        #region enums
        public enum AddClearButton : byte
        {
            None = 0,
            Add = 1,
            Clear = 2,
        }
        public enum EntityStatus : byte
        {
            NotAlive = 0,
            Alive = 1,
            Undefined = 2,
        }
        #endregion

        #region HitTest/ClickTest
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
        internal static bool ClickTest(Rect rect)
        {
            Event evt = Event.current;
            return ClickTest(rect, evt);
        }
        internal static bool ClickTest(Rect rect, Event evt)
        {
            return HitTest(rect, evt.mousePosition) && evt.type == EventType.MouseUp;
        }
        #endregion

        #region small elems
        public static void DrawTextureSoftColor(Rect position, Texture texture)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawTexture(position, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, GUI.color * 0.5f, null);
            }
        }
        public static void DrawTexture(Rect position, Texture texture)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Graphics.DrawTexture(position, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, GUI.color, null);
            }
        }
        public static void DrawRectSoftColor(Rect position, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture texture = UnityEditorUtility.GetWhiteTexture();
                Graphics.DrawTexture(position, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, GUI.color * color * 0.5f, null);
            }
        }
        public static void DrawRect(Rect position, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture texture = UnityEditorUtility.GetWhiteTexture();
                Graphics.DrawTexture(position, texture, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, GUI.color * color, null);
            }
        }
        public static void DrawIcon(Rect position, Texture icon, float iconPadding, string tooltip)
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
                GUI.Label(position, UnityEditorUtility.GetLabel(string.Empty, tooltip));
                DrawTextureSoftColor(RectUtility.AddPadding(position, iconPadding), icon);
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

        public static void EntityHyperlinkButton(Rect position, EcsWorld world, int entityID)
        {
            var current = Event.current;
            var hover = IconHoverScan(position, current);

            var click = IconButton(position, Icons.Instance.HyperlinkIcon, 2f, string.Empty);
            if (GUI.enabled)
            {
                if (click)
                {
                    var obj = world.Get<EntityLinksComponent>().GetLink(entityID);
                    if (obj != null)
                    {
                        EditorGUIUtility.PingObject(obj);
                        Selection.activeObject = obj;
                    }
                }
            }
        }
        public static void ScriptAssetButton(Rect position, MonoScript script)
        {
            var current = Event.current;
            var hover = IconHoverScan(position, current);
            if (GUI.enabled)
            {
                using (SetColor(1f, 1f, 1f, hover ? 1f : 0.8f))
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
                        //UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(); //TODO
                        AssetDatabase.OpenAsset(script);
                    }
                }
            }
            else
            {
                using (SetColor(0.85f, 0.85f, 0.85f, 0.7f))
                {
                    DrawIcon(position, Icons.Instance.FileIcon, 2f, "One click - Ping File. Double click - Edit Script");
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

        public static bool NewEntityButton(Rect position)
        {
            return IconButton(position, Icons.Instance.PassIcon, 2f, "Create entity");
        }
        public static bool ValidateButton(Rect position)
        {
            return IconButton(position, Icons.Instance.RepaireIcon, 2f, "Validate");
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
        internal readonly struct EntityLinksComponent : IEcsWorldComponent<EntityLinksComponent>
        {
            private readonly Storage _storage;
            private EntityLinksComponent(Storage storage) { _storage = storage; }
            public void SetConnectLink(int entityID, EcsEntityConnect link) { _storage.links[entityID].connect = link; }
            public void SetMonitorLink(int entityID, EntityMonitor link) { _storage.links[entityID].monitor = link; }
            public EcsEntityConnect GetConnectLink(int entityID) { return _storage.links[entityID].connect; }
            public EntityMonitor GetMonitorLink(int entityID) { return _storage.links[entityID].monitor; }
            public UnityEngine.Object GetLink(int entityID)
            {
                ref var links = ref _storage.links[entityID];
                if (links.connect != null)
                {
                    return links.connect;
                }
                return links.monitor;
            }
            void IEcsWorldComponent<EntityLinksComponent>.Init(ref EntityLinksComponent component, EcsWorld world)
            {
                component = new EntityLinksComponent(new Storage(world));
            }
            void IEcsWorldComponent<EntityLinksComponent>.OnDestroy(ref EntityLinksComponent component, EcsWorld world)
            {
                component = default;
            }
            private class Storage : IEcsWorldEventListener
            {
                private readonly EcsWorld _world;
                public (EcsEntityConnect connect, EntityMonitor monitor)[] links;
                public Storage(EcsWorld world)
                {
                    _world = world;
                    _world.AddListener(this);
                    links = new (EcsEntityConnect, EntityMonitor)[_world.Capacity];
                }
                public void OnWorldResize(int newSize) { Array.Resize(ref links, newSize); }
                public void OnReleaseDelEntityBuffer(ReadOnlySpan<int> buffer) { }
                public void OnWorldDestroy() { }
            }
        }
        public static void EntityField(Rect position, entlong entity)
        {
            EntityField(position, DragonGUIContent.Empty, entity);
        }
        public static unsafe void EntityField(Rect position, DragonGUIContent label, entlong entity)
        {
            EntityField(position, label, (EntitySlotInfo)entity);
        }
        public static void EntityField(Rect position, EntitySlotInfo entity)
        {
            EntityField(position, DragonGUIContent.Empty, entity);
        }
        public static void EntityField(Rect position, DragonGUIContent label, EntitySlotInfo entity)
        {
            bool isAlive = false;
            if (EcsWorld.TryGetWorld(entity.world, out EcsWorld world))
            {
                isAlive = world.IsAlive(entity.id, entity.gen);
            }
            EntityField_Internal(position, label, entity.id == 0, isAlive ? EntityStatus.Alive : EntityStatus.NotAlive, entity.id, entity.gen, entity.world);
        }
        public static void EntityField(Rect position, SerializedProperty property)
        {
            EntityField(position, property, DragonGUIContent.Empty);
        }
        public static void EntityField(Rect position, SerializedProperty property, DragonGUIContent label)
        {
            EntitySlotInfo entity = new EntitySlotInfo(property.FindPropertyRelative("_full").longValue);

            if (property.hasMultipleDifferentValues)
            {
                EntityField_Internal(position, label, true, EntityStatus.Undefined, 0, 0, 0);
            }
            else
            {
                bool isAlive = false;
                if (EcsWorld.TryGetWorld(entity.world, out EcsWorld world))
                {
                    isAlive = world.IsAlive(entity.id, entity.gen);
                }
                EntityField_Internal(position, label, entity.id == 0, isAlive ? EntityStatus.Alive : EntityStatus.NotAlive, entity.id, entity.gen, entity.world);
            }
        }

        internal static void EntityField_Internal(Rect position, GUIContent label, bool isPlaceholder, EntityStatus status, int id, short gen, short world)
        {
            if (label != null)
            {
                Rect labelRect;
                (labelRect, position) = position.HorizontalSliceLeft(EditorGUIUtility.labelWidth * 0.65f);
                EditorGUI.LabelField(labelRect, label);
            }

            using (SetLabelWidth(0f))
            {
                var (entityInfoRect, statusRect) = RectUtility.VerticalSliceBottom(position, 3f);

                Color statusColor;
                switch (status)
                {
                    case EntityStatus.NotAlive:
                        statusColor = RedColor;
                        break;
                    case EntityStatus.Alive:
                        statusColor = GreenColor;
                        break;
                    default:
                        statusColor = new Color32(200, 200, 200, 255);
                        break;
                }

                statusColor.a = 0.6f;
                EditorGUI.DrawRect(statusRect, statusColor);

                EntityFieldContent_Internal(entityInfoRect, isPlaceholder, id, gen, world);
            }
        }
        private static void EntityFieldContent_Internal(Rect position, bool isPlaceHolder, int id, short gen, short world)
        {
            using (SetLabelWidth(0f)) using (SetIndentLevel(0))
            {
                Color w = Color.gray;
                w.a = 0.6f;
                Color b = Color.black;
                b.a = 0.55f;
                DrawRectSoftColor(position, w);

                var (idRect, genWorldRect) = RectUtility.HorizontalSliceLerp(position, 0.4f);
                var (genRect, worldRect) = RectUtility.HorizontalSliceLerp(genWorldRect, 0.5f);

                idRect = RectUtility.AddPadding(idRect, 2, 1, 0, 0);
                genRect = RectUtility.AddPadding(genRect, 1, 1, 0, 0);
                worldRect = RectUtility.AddPadding(worldRect, 1, 2, 0, 0);
                DrawRectSoftColor(idRect, b);
                DrawRectSoftColor(genRect, b);
                DrawRectSoftColor(worldRect, b);

                GUIStyle style = UnityEditorUtility.GetInputFieldCenterAnhor();

                if (isPlaceHolder)
                {
                    using (SetAlpha(0.85f)) using (Disable)
                    {
                        GUI.Label(idRect, "Entity ID", style);
                        GUI.Label(genRect, "Generation", style);
                        GUI.Label(worldRect, "World ID", style);
                    }
                }
                else
                {
                    EditorGUI.IntField(idRect, id, style);
                    using (SetAlpha(0.85f))
                    {
                        EditorGUI.IntField(genRect, gen, style);
                        EditorGUI.IntField(worldRect, world, style);
                    }
                }
            }
        }
        #endregion

        #region DrawTypeMetaBlock
        private static float DrawTypeMetaBlockPadding => EditorGUIUtility.standardVerticalSpacing;
        private static float SingleLineWithPadding => EditorGUIUtility.singleLineHeight + DrawTypeMetaBlockPadding * 4f;
        public static float GetTypeMetaBlockHeight(float contentHeight)
        {
            return DrawTypeMetaBlockPadding * 2 + contentHeight;
        }
        public static bool DrawTypeMetaElementBlock(ref Rect position, SerializedProperty arrayProperty, int elementIndex, SerializedProperty elementRootProperty, ITypeMeta meta)
        {
            var result = DrawTypeMetaBlock_Internal(ref position, elementRootProperty, meta, elementIndex, arrayProperty.arraySize);
            if (result.HasFlag(DrawTypeMetaBlockResult.CloseButtonClicked))
            {
                arrayProperty.DeleteArrayElementAtIndex(elementIndex);
            }
            return result != DrawTypeMetaBlockResult.None;
        }
        public static bool DrawTypeMetaBlock(ref Rect position, SerializedProperty rootProperty, ITypeMeta meta, int index = -1, int total = -1)
        {
            var result = DrawTypeMetaBlock_Internal(ref position, rootProperty, meta, index, total);
            if (result.HasFlag(DrawTypeMetaBlockResult.CloseButtonClicked))
            {
                rootProperty.ResetValues();
            }
            return result.HasFlag(DrawTypeMetaBlockResult.Drop);
        }

        private enum DrawTypeMetaBlockResult
        {
            None = 0,
            Drop = 1 << 0,
            CloseButtonClicked = 1 << 1,
        }
        private static DrawTypeMetaBlockResult DrawTypeMetaBlock_Internal(ref Rect position, SerializedProperty rootProperty, ITypeMeta meta, int index = -1, int total = -1)
        {
            Color alphaPanelColor;
            if (meta == null)
            {
                alphaPanelColor = Color.black;
                alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;
                EditorGUI.DrawRect(position, alphaPanelColor);
                position = position.AddPadding(DrawTypeMetaBlockPadding * 2f);
                return DrawTypeMetaBlockResult.None;
            }

            string name = meta.Name;
            string description = meta.Description.Text;

            int positionIndex;
            if (index < 0)
            {
                positionIndex = int.MaxValue;
                var counter = rootProperty.Copy();
                int depth = -1;
                while (counter.NextVisibleDepth(false, ref depth))
                {
                    positionIndex--;
                }
            }
            else
            {
                positionIndex = index;
            }

            alphaPanelColor = SelectPanelColor(meta, positionIndex, total).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE).SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA);

            DrawTypeMetaBlockResult result = DrawTypeMetaBlockResult.None;
            using (CheckChanged())
            {
                EditorGUI.DrawRect(position, alphaPanelColor);

                Rect optionButton = position;
                position = position.AddPadding(DrawTypeMetaBlockPadding * 2f);

                optionButton.center -= new Vector2(0, optionButton.height);
                optionButton.yMin = optionButton.yMax;
                optionButton.yMax += HeadIconsRect.height;
                optionButton.xMin = optionButton.xMax - 64;
                optionButton.center += Vector2.up * DrawTypeMetaBlockPadding;

                //Canceling isExpanded
                bool oldIsExpanded = rootProperty.isExpanded;
                if (ClickTest(optionButton))
                {
                    rootProperty.isExpanded = oldIsExpanded;
                    result |= DrawTypeMetaBlockResult.Drop;
                }

                //Close button
                optionButton.xMin = optionButton.xMax - HeadIconsRect.width;
                if (CloseButton(optionButton))
                {
                    result |= DrawTypeMetaBlockResult.CloseButtonClicked;
                    return result;
                }
                //Edit script button
                if (ScriptsCache.TryGetScriptAsset(meta.FindRootTypeMeta(), out MonoScript script))
                {
                    optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                    ScriptAssetButton(optionButton, script);
                }
                //Description icon
                if (string.IsNullOrEmpty(description) == false)
                {
                    optionButton = HeadIconsRect.MoveTo(optionButton.center - (Vector2.right * optionButton.width));
                    DescriptionIcon(optionButton, description);
                }
            }
            return result;
        }
        #endregion

        #region NextDepth/GetChildPropertiesCount
        internal static bool NextVisibleDepth(this SerializedProperty property, bool child, ref int depth)
        {
            if (depth < 0)
            {
                depth = property.depth;
            }
            var next = property.NextVisible(child);
            return next && property.depth >= depth;
        }
        internal static bool NextDepth(this SerializedProperty property, bool child, ref int depth)
        {
            if (depth < 0)
            {
                depth = property.depth;
            }
            return property.Next(child) && property.depth >= depth;
        }
        internal static int GetChildPropertiesCount(this SerializedProperty property, Type type, out bool isEmpty)
        {
            int result = GetChildPropertiesCount(property);
            isEmpty = result <= 0 && type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;
            return result;
        }
        internal static int GetChildPropertiesCount(this SerializedProperty property)
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
        #endregion

        #region SelectPanelColor
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
                        return GetGenericPanelColor(index);
                }
            }
        }
        public static Color GetGenericPanelColor(int index)
        {
            return index % 2 == 0 ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.54f, 0.54f, 0.54f);
        }
        #endregion

        #region Other Elements
        public static void ManuallySerializeButton(Rect position, UnityEngine.Object obj)
        {
            if (GUI.Button(position, UnityEditorUtility.GetLabel("Manually serialize")))
            {
                var so = new SerializedObject(obj);
                EditorUtility.SetDirty(obj);
                so.UpdateIfRequiredOrScript();
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        public static bool AddComponentButton(Rect position, out Rect dropDownRect)
        {
            dropDownRect = RectUtility.AddPadding(position, 20f, 20f, 1f, 1f); ;
            return GUI.Button(dropDownRect, "Add Component");
        }
        public static AddClearButton AddClearComponentButtons(Rect position, out Rect dropDownRect)
        {
            return AddClearButtons(position, "Add Component", "Clear", out dropDownRect);
        }
        public static AddClearButton AddClearSystemButtons(Rect position, out Rect dropDownRect)
        {
            return AddClearButtons(position, "Add Record", "Clear", out dropDownRect);
        }
        public static AddClearButton AddClearButtons(Rect position, string addText, string clearText, out Rect dropDownRect)
        {
            position = RectUtility.AddPadding(position, 20f, 20f, 1f, 1f);
            var (left, right) = RectUtility.HorizontalSliceLerp(position, 0.75f);

            dropDownRect = left;

            if (GUI.Button(left, addText))
            {
                return AddClearButton.Add;
            }
            if (GUI.Button(right, clearText))
            {
                return AddClearButton.Clear;
            }
            return AddClearButton.None;
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
        #endregion

        #region SerializeReference utils

        private static Dictionary<PredicateTypesKey, ReferenceDropDown> _predicatTypesMenus = new Dictionary<PredicateTypesKey, ReferenceDropDown>();
        [ThreadStatic]
        private static SerializedProperty _currentProperty;

        #region Init
        private static ReferenceDropDown GetReferenceDropDown(Type[] predicatTypes, Type[] sortedWithOutTypes)
        {
            if (_predicatTypesMenus.TryGetValue((predicatTypes, sortedWithOutTypes), out ReferenceDropDown menu) == false)
            {
                menu = new ReferenceDropDown(predicatTypes, sortedWithOutTypes);
                menu.OnSelected += SelectComponent;
                _predicatTypesMenus.Add((predicatTypes, sortedWithOutTypes), menu);
            }

            return menu;
        }
        private static void SelectComponent(ReferenceDropDown.Item item)
        {
            Type type = item.Type;
            if (type == null)
            {
                _currentProperty.managedReferenceValue = null;
            }
            else
            {
                _currentProperty.managedReferenceValue = Activator.CreateInstance(type);
                _currentProperty.isExpanded = true;
            }

            _currentProperty.serializedObject.ApplyModifiedProperties();
            DelayedChanged = true;
        }
        #endregion

        #region PredicateTypesKey
        private readonly struct PredicateTypesKey : IEquatable<PredicateTypesKey>
        {
            public readonly Type[] types;
            public readonly Type[] withoutTypes;
            public PredicateTypesKey(Type[] types, Type[] withoutTypes)
            {
                this.types = types;
                this.withoutTypes = withoutTypes;
            }
            public bool Equals(PredicateTypesKey other)
            {
                if (types.Length != other.types.Length) { return false; }
                if (withoutTypes.Length != other.withoutTypes.Length) { return false; }
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] != other.types[i])
                    {
                        return false;
                    }
                }
                for (int i = 0; i < withoutTypes.Length; i++)
                {
                    if (withoutTypes[i] != other.withoutTypes[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            public override bool Equals(object obj)
            {
                return obj is PredicateTypesKey key && Equals(key);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(types);
            }
            public static implicit operator PredicateTypesKey((Type[], Type[]) types) { return new PredicateTypesKey(types.Item1, types.Item2); }
        }
        #endregion

        #region ReferenceDropDown
        private class ReferenceDropDown : AdvancedDropdown
        {
            public readonly Type[] PredicateTypes;
            public readonly Type[] WithOutTypes;
            public ReferenceDropDown(Type[] predicateTypes, Type[] withOutTypes) : base(new AdvancedDropdownState())
            {
                PredicateTypes = predicateTypes;
                WithOutTypes = withOutTypes;
                minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * 30);
            }
            protected override AdvancedDropdownItem BuildRoot()
            {
                int increment = 0;
                var root = new Item(null, "Select Type", increment++);
                root.AddChild(new Item(null, "<NULL>", increment++));

                Dictionary<Key, Item> dict = new Dictionary<Key, Item>();

                foreach (var type in UnityEditorUtility._serializableTypes)
                {
                    bool isAssignable = false;
                    foreach (Type predicateTypes in PredicateTypes)
                    {
                        if (predicateTypes.IsAssignableFrom(type))
                        {
                            isAssignable = true;
                            break;
                        }
                    }
                    foreach (Type withoutType in WithOutTypes)
                    {
                        if (withoutType.IsAssignableFrom(type))
                        {
                            isAssignable = false;
                            break;
                        }
                    }
                    if (isAssignable)
                    {
                        ITypeMeta meta = type.ToMeta();
                        string description = meta.Description.Text;
                        MetaGroup group = meta.Group;
                        var splitedGroup = group.Splited;

                        Item parent = root;
                        if (splitedGroup.Count > 0)
                        {
                            int i = 1;
                            foreach (var subgroup in splitedGroup)
                            {
                                Key key = new Key(group, i);
                                if (dict.TryGetValue(key, out Item item) == false)
                                {
                                    item = new Item(null, subgroup, increment++);
                                    parent.AddChild(item);
                                    dict.Add(key, item);
                                }
                                parent = item;
                                i++;
                            }
                        }

                        var leafItem = new Item(type, meta.Name, increment++);
                        parent.AddChild(leafItem);
                    }
                }
                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                OnSelected((Item)item);
            }

            public event Action<Item> OnSelected = delegate { };

            public class Item : AdvancedDropdownItem
            {
                public readonly Type Type;
                public Item(Type type, string name, int id) : base(name)
                {
                    Type = type;
                    this.id = id;
                }
            }

            #region Key
            private readonly struct Key : IEquatable<Key>
            {
                public readonly MetaGroup Group;
                public readonly int Length;
                public Key(MetaGroup group, int length)
                {
                    Group = group;
                    Length = length;
                }
                public bool Equals(Key other)
                {
                    if (Length != other.Length)
                    {
                        return false;
                    }
                    IEnumerator<string> splitedEnum = Group.Splited.GetEnumerator();
                    IEnumerator<string> splitedEnumOther = other.Group.Splited.GetEnumerator();
                    for (int i = 0; i < Length; i++)
                    {
                        splitedEnum.MoveNext();
                        splitedEnumOther.MoveNext();
                        if (splitedEnum.Current != splitedEnumOther.Current)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                public override bool Equals(object obj)
                {
                    return obj is Key key && Equals(key);
                }
                public override int GetHashCode()
                {
                    unchecked
                    {
                        int state = Length;
                        state ^= state << 13;
                        state ^= state >> 17;
                        state ^= state << 5;
                        var x = Group.Splited.GetEnumerator();
                        x.MoveNext();
                        return x.Current.GetHashCode() ^ state;
                    }
                    ;
                }
            }
            #endregion
        }
        #endregion

        public static void DrawSelectReferenceButton(Rect position, SerializedProperty property, Type[] sortedPredicateTypes, Type[] sortedWithOutTypes, bool isHideButtonIfNotNull)
        {
            object obj = property.hasMultipleDifferentValues ? null : property.managedReferenceValue;

            string text = obj == null ? "Select..." : obj.GetMeta().Name;
            if (!isHideButtonIfNotNull || obj == null)
            {
                if (GUI.Button(position, text, EditorStyles.layerMaskField))
                {
                    DrawSelectReferenceMenu(position, property, sortedPredicateTypes, sortedWithOutTypes);
                }
            }
            else
            {
                GUI.Label(position, text);
            }
        }
        public static void DrawSelectReferenceMenu(Rect position, SerializedProperty property, Type[] sortedPredicateTypes, Type[] sortedWithOutTypes)
        {
            _currentProperty = property;
            GetReferenceDropDown(sortedPredicateTypes, sortedWithOutTypes).Show(position);
        }

        #endregion
    }
}
#endif