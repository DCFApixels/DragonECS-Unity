#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System.Reflection;
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

            var eventType = Event.current.type;
            Rect dropRect = position;
            dropRect.yMax = position.yMin + EditorGUIUtility.singleLineHeight;
            int controlID = GUIUtility.GetControlID(s_ObjectFieldHash, FocusType.Keyboard, position);

            bool containsMouse = dropRect.Contains(Event.current.mousePosition);

            if(containsMouse && eventType == EventType.Repaint && DragAndDrop.activeControlID == controlID)
            {
                EditorStyles.selectionRect.Draw(dropRect.AddPadding(-1), GUIContent.none, controlID, false, false);
            }


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




            if (!containsMouse || !GUI.enabled)
            {
                return;
            }
            if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }

            if (eventType == EventType.DragPerform || eventType == EventType.DragUpdated)
            {
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
                        else
                        {
                            foreach (var beh in go.GetComponents<MonoBehaviour>())
                            {
                                if (TryFindEntlong(beh, out ent))
                                {
                                    isValide = true;
                                    break;
                                }
                            }
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
                        else
                        {
                            if (TryFindEntlong(dragged, out ent))
                            {
                                isValide = true;
                            }
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
            }

        }

        private bool TryFindEntlong(Object uniObj, out entlong ent)
        {
            var fields = uniObj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if(field.FieldType == typeof(entlong))
                {
                    ent = (entlong)field.GetValue(uniObj);
                    return true;
                }
            }

            var iterator = new SerializedObject(uniObj).GetIterator();
            iterator.NextVisible(true);
            while (iterator.Next(true))
            {
                if (iterator.propertyType == SerializedPropertyType.Integer &&
                    iterator.propertyPath.Contains(nameof(entlong)))
                {
                    var l = iterator.longValue;
                    ent = *(entlong*)&l;
                    return true;
                }
            }
            ent = entlong.NULL;
            return false;
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