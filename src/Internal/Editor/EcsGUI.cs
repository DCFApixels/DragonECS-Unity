#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityComponent = UnityEngine.Component;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class EcsGUI
    {
        #region Scores
        private static int _changedCounter = 0;
        private static bool _changed = false;
        private static bool _delayedChanged = false;

        public static int ChangedCounter => _changedCounter;
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
                public void Dispose() { GUILayout.EndVertical(); }
            }
            public struct HorizontalScope : IDisposable
            {
                public HorizontalScope(GUILayoutOption[] options) { GUILayout.BeginHorizontal(options); }
                public HorizontalScope(GUIStyle style, GUILayoutOption[] options) { GUILayout.BeginHorizontal(style, options); }
                public void Dispose() { GUILayout.EndHorizontal(); }
            }
            public struct ScrollViewScope : IDisposable
            {
                public ScrollViewScope(ref Vector2 pos, GUILayoutOption[] options) { pos = GUILayout.BeginScrollView(pos, options); }
                public ScrollViewScope(ref Vector2 pos, GUIStyle style, GUILayoutOption[] options) { pos = GUILayout.BeginScrollView(pos, style, options); }
                public void Dispose() { GUILayout.EndScrollView(); }
            }

            public static ScrollViewScope BeginScrollView(ref Vector2 pos) => new ScrollViewScope(ref pos, Array.Empty<GUILayoutOption>());
            public static HorizontalScope BeginHorizontal() => new HorizontalScope(Array.Empty<GUILayoutOption>());
            public static VerticalScope BeginVertical() => new VerticalScope(Array.Empty<GUILayoutOption>());
            public static ScrollViewScope BeginScrollView(ref Vector2 pos, params GUILayoutOption[] options) => new ScrollViewScope(ref pos, options);
            public static HorizontalScope BeginHorizontal(params GUILayoutOption[] options) => new HorizontalScope(options);
            public static VerticalScope BeginVertical(params GUILayoutOption[] options) => new VerticalScope(options);
            public static ScrollViewScope BeginScrollView(ref Vector2 pos, GUIStyle style, params GUILayoutOption[] options) => new ScrollViewScope(ref pos, style, options);
            public static HorizontalScope BeginHorizontal(GUIStyle style, params GUILayoutOption[] options) => new HorizontalScope(style, options);
            public static VerticalScope BeginVertical(GUIStyle style, params GUILayoutOption[] options) => new VerticalScope(style, options);
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

        private static readonly Rect HeadIconsRect = new Rect(0f, 0f, 19f, 19f);

        public static float EntityBarHeight => EditorGUIUtility.singleLineHeight + 3f;

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
        [Flags]
        public enum EntityStatus : byte
        {
            NotAlive = 0,
            Alive = 1 << 0,
            Undefined = 1 << 1,
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
            using (SetLabelWidth(0f))
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
            using (SetLabelWidth(0f))
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
                        return index % 2 == 0 ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.54f, 0.54f, 0.54f);
                }
            }
        }
        #endregion

        #region Other Elements
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
        private static ReferenceDropDown GetReferenceDropDown(Type[] predicatTypes)
        {
            if (_predicatTypesMenus.TryGetValue(predicatTypes, out ReferenceDropDown menu) == false)
            {
                menu = new ReferenceDropDown(predicatTypes);
                menu.OnSelected += SelectComponent;
                _predicatTypesMenus.Add(predicatTypes, menu);
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
            public PredicateTypesKey(Type[] types)
            {
                this.types = types;
            }
            public bool Equals(PredicateTypesKey other)
            {
                if (types.Length != other.types.Length) { return false; }
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] != other.types[i])
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
            public static implicit operator PredicateTypesKey(Type[] types) { return new PredicateTypesKey(types); }
            public static implicit operator Type[](PredicateTypesKey key) { return key.types; }
        }
        #endregion

        #region ReferenceDropDown
        private class ReferenceDropDown : AdvancedDropdown
        {
            public readonly Type[] PredicateTypes;
            public ReferenceDropDown(Type[] predicateTypes) : base(new AdvancedDropdownState())
            {
                PredicateTypes = predicateTypes;
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
                    };
                }
            }
            #endregion
        }
        #endregion

        public static void DrawSelectReferenceButton(Rect position, SerializedProperty property, Type[] sortedPredicateTypes, bool isHideButtonIfNotNull)
        {
            object obj = property.hasMultipleDifferentValues ? null : property.managedReferenceValue;
            string text = obj == null ? "Select..." : obj.GetMeta().Name;
            if (!isHideButtonIfNotNull || obj == null)
            {
                if (GUI.Button(position, text, EditorStyles.layerMaskField))
                {
                    DrawSelectReferenceMenu(position, property, sortedPredicateTypes);
                }
            }
            else
            {
                GUI.Label(position, text);
            }
        }
        public static void DrawSelectReferenceMenu(Rect position, SerializedProperty property, Type[] sortedPredicateTypes)
        {
            _currentProperty = property;
            GetReferenceDropDown(sortedPredicateTypes).Show(position);
        }
        #endregion


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public static partial class Layout
        {
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

                using (EcsGUI.Layout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f)))
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

                        GUILayout.Box("", UnityEditorUtility.GetStyle(GUI.color, 0.16f), GUILayout.ExpandWidth(true));
                        IsShowHidden = EditorGUI.Toggle(GUILayoutUtility.GetLastRect(), "Show Hidden", IsShowHidden);

                        if (_componentPoolsBuffer == null)
                        {
                            _componentPoolsBuffer = new List<IEcsPool>(64);
                        }
                        world.GetComponentPoolsFor(entityID, _componentPoolsBuffer);
                        int i = 0;
                        int iMax = _componentPoolsBuffer.Count;
                        foreach (var componentPool in _componentPoolsBuffer)
                        {
                            DrawRuntimeComponent(entityID, componentPool, iMax, i++);
                        }
                    }
                }
            }
            private static void DrawRuntimeComponent(int entityID, IEcsPool pool, int total, int index)
            {
                var meta = pool.ComponentType.ToMeta();
                if (meta.IsHidden == false || IsShowHidden)
                {
                    object data = pool.GetRaw(entityID);

                    Type componentType = pool.ComponentType;
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

                    Color panelColor = SelectPanelColor(meta, index, total).Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE).SetAlpha(EscEditorConsts.COMPONENT_DRAWER_ALPHA);
                    GUILayout.BeginVertical(UnityEditorUtility.GetStyle(panelColor));
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


                    if (DrawRuntimeData(componentType, UnityEditorUtility.GetLabel(meta.Name), expandMatrix, data, out object resultData))
                    {
                        pool.SetRaw(entityID, resultData);
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
                        using (UpIndentLevel())
                        {
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
                        }
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