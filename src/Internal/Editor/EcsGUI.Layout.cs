#if UNITY_EDITOR
using DCFApixels.DragonECS.Core.Unchecked;
using DCFApixels.DragonECS.Unity.Editors.X;
using DCFApixels.DragonECS.Unity.Internal;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static partial class EcsGUI
    {
        public static partial class Layout
        {
            public static void ManuallySerializeButton(IEnumerable<UnityObject> objects)
            {
                if (GUILayout.Button(UnityEditorUtility.GetLabel("Manually serialize")))
                {
                    foreach (var obj in objects)
                    {
                        EditorUtility.SetDirty(obj);
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            public static void ManuallySerializeButton(UnityObject obj)
            {
                if (GUILayout.Button(UnityEditorUtility.GetLabel("Manually serialize")))
                {
                    EditorUtility.SetDirty(obj);
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
                RuntimeComponentsDrawer.DrawWorldComponents(world);
            }

            #region entity bar
            public static void EntityField(entlong entity)
            {
                EntityField(default(GUIContent), entity);
            }
            public static void EntityField(string label, entlong entity)
            {
                EntityField(UnityEditorUtility.GetLabel(label), entity);
            }
            public static unsafe void EntityField(GUIContent label, entlong entity)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityField(GUILayoutUtility.GetRect(width, height), label, entity);
            }
            public static void EntityField(EntitySlotInfo entity)
            {
                EntityField(default(GUIContent), entity);
            }
            public static void EntityField(string label, EntitySlotInfo entity)
            {
                EntityField(UnityEditorUtility.GetLabel(label), entity);
            }
            public static void EntityField(GUIContent label, EntitySlotInfo entity)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityField(GUILayoutUtility.GetRect(width, height), label, entity);
            }
            public static void EntityField(SerializedProperty property)
            {
                EntityField(property, default(GUIContent));
            }
            public static void EntityField(SerializedProperty property, string label)
            {
                EntityField(property, UnityEditorUtility.GetLabel(label));
            }
            public static void EntityField(SerializedProperty property, GUIContent label)
            {
                float width = EditorGUIUtility.currentViewWidth;
                float height = EntityBarHeight;
                EcsGUI.EntityField(GUILayoutUtility.GetRect(width, height), property, label);
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
            public static void DrawRuntimeComponents(entlong entity, bool isWithFoldout, bool isRoot = true)
            {
                if (entity.TryUnpackForUnityEditor(out int entityID, out _, out _, out EcsWorld world))
                {
                    DrawRuntimeComponents(entityID, world, isWithFoldout, isRoot);
                }
            }

            public static void DrawRuntimeComponents(int entityID, EcsWorld world, bool isWithFoldout, bool isRoot)
            {
                RuntimeComponentsDrawer.DrawRuntimeComponents(entityID, world, isWithFoldout, isRoot);
            }
        }
    }
}
#endif