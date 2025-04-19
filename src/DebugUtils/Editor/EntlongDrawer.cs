#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomPropertyDrawer(typeof(entlong))]
    internal unsafe class EntlongDrawer : ExtendedPropertyDrawer
    {
        private float heightCache = 0;
        private static readonly int s_ObjectFieldHash = "s_ObjectFieldHash".GetHashCode();
        protected override void DrawCustom(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect labelRect, hyperlinkButtonRect;
            (labelRect, position) = position.HorizontalSliceLeft(EditorGUIUtility.labelWidth * 0.65f);
            (position, hyperlinkButtonRect) = position.HorizontalSliceRight(18f);

            bool drawFoldout = property.hasMultipleDifferentValues == false;

            bool isExpanded = false;
            if (drawFoldout)
            {
                EditorGUI.BeginChangeCheck();
                isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);
                if (EditorGUI.EndChangeCheck())
                {
                    property.isExpanded = isExpanded;
                }
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            SerializedProperty fulleProperty = property.FindPropertyRelative("_full");
            EntitySlotInfo entity = new EntitySlotInfo(fulleProperty.longValue);
            EcsWorld.TryGetWorld(entity.world, out EcsWorld world);

            if (drawFoldout && isExpanded)
            {
                using (EcsGUI.UpIndentLevel())
                {
                    if (world != null && world.IsAlive(entity.id, entity.gen))
                    {
                        EcsGUI.Layout.DrawRuntimeComponents(entity.id, world, false, false);
                        if (Event.current.type == EventType.Layout)
                        {
                            heightCache = GUILayoutUtility.GetLastRect().height;
                        }
                    }
                }
            }
            EcsGUI.EntityField(position, property);
            using (EcsGUI.SetEnable(world != null))
            {
                EcsGUI.EntityHyperlinkButton(hyperlinkButtonRect, world, entity.id);
            }


            Rect dropRect = position;
            dropRect.yMax = position.yMin + EditorGUIUtility.singleLineHeight;


            var eventType = Event.current.type;
            int controlID = GUIUtility.GetControlID(s_ObjectFieldHash, FocusType.Keyboard, position);
            if (!dropRect.Contains(Event.current.mousePosition) || !GUI.enabled)
            {
                return;
            }
            if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            if (eventType == EventType.DragPerform)
            {
                entlong ent = default;
                bool isValide = false;
                var dragged = DragAndDrop.objectReferences[0];
                if (dragged is GameObject go)
                {
                    if (go.TryGetComponent(out EcsEntityConnect connect))
                    {
                        ent = connect.Entity;
                        isValide = true;
                    }
                    else if (go.TryGetComponent(out EntityMonitor monitor))
                    {
                        ent = monitor.Entity;
                        isValide = true;
                    }
                }
                else
                {
                    if (dragged is EcsEntityConnect connect)
                    {
                        ent = connect.Entity;
                        isValide = true;
                    }
                    else if (dragged is EntityMonitor monitor)
                    {
                        ent = monitor.Entity;
                        isValide = true;
                    }
                }

                if (isValide)
                {
                    long entityLong = *(long*)&ent;
                    fulleProperty.longValue = entityLong;
                }


                EcsGUI.Changed = true;
                DragAndDrop.AcceptDrag();
                DragAndDrop.activeControlID = 0;
            }
            else
            {
                DragAndDrop.activeControlID = controlID;
            }

            //Event.current.Use();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            const float UNITY_HEIGHT_CONSTANT = 18f;
            if (property.hasMultipleDifferentValues)
            {
                return UNITY_HEIGHT_CONSTANT;
            }
            return Mathf.Max(heightCache, UNITY_HEIGHT_CONSTANT);
        }
    }
}
#endif