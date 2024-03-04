using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;

#if UNITY_EDITOR
namespace DCFApixels.DragonECS.Unity.Editors
{
    using UnityEditor;
    using UnityEngine;

    public abstract class EntityTemplateEditorBase : Editor
    {
        private static readonly Rect RemoveButtonRect = new Rect(0f, 0f, 17f, 19f);
        private static readonly Rect TooltipIconRect = new Rect(0f, 0f, 21f, 15f);

        private GUIStyle removeButtonStyle;
        private GenericMenu genericMenu;
        private bool _isInit = false;

        #region Init
        private void Init()
        {
            if (genericMenu == null)
                _isInit = false;
            if (_isInit)
                return;

            var tmpstylebase = EcsEditor.GetStyle(new Color(0.9f, 0f, 0.22f), 0.5f);
            var tmpStyle = EcsEditor.GetStyle(new Color(1f, 0.5f, 0.7f), 0.5f);

            removeButtonStyle = new GUIStyle(EditorStyles.linkLabel);
            removeButtonStyle.alignment = TextAnchor.MiddleCenter;

            removeButtonStyle.normal = tmpstylebase.normal;
            removeButtonStyle.hover = tmpStyle.normal;
            removeButtonStyle.active = tmpStyle.normal;
            removeButtonStyle.focused = tmpStyle.normal;

            removeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            removeButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            removeButtonStyle.border = new RectOffset(0, 0, 0, 0);

            genericMenu = new GenericMenu();

            var dummies = ComponentTemplateTypeCache.Dummies;
            foreach (var dummy in dummies)
            {
                string name = dummy.Name;
                string description = dummy.Description;
                if (string.IsNullOrEmpty(description) == false)
                {
                    name = $"{name} {EcsUnityConsts.INFO_MARK}";
                }
                genericMenu.AddItem(new GUIContent(name, description), false, OnAddComponent, dummy);
            }

            _isInit = true;
        }
        #endregion

        #region Add/Remove
        private void OnAddComponent(object obj)
        {
            Type componentType = obj.GetType();
            if (this.target is ITemplateInternal target)
            {
                SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
                for (int i = 0; i < componentsProp.arraySize; i++)
                {
                    if (componentsProp.GetArrayElementAtIndex(i).managedReferenceValue.GetType() == componentType)
                        return;
                }

                componentsProp.InsertArrayElementAtIndex(0);

                componentsProp.GetArrayElementAtIndex(0).managedReferenceValue = ((IComponentTemplate)obj).Clone();

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this.target);
            }
        }
        private void OnRemoveComponentAt(int index)
        {
            if (this.target is ITemplateInternal target)
            {
                SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
                componentsProp.DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(this.target);
            }
        }
        #endregion

        protected void Draw(ITemplateInternal target)
        {
            Init();
            SerializedProperty componentsProp = serializedObject.FindProperty(target.ComponentsPropertyName);
            if (componentsProp == null)
            {
                return;
            }

            DrawTop(target);
            GUILayout.BeginVertical(EcsEditor.GetStyle(Color.black, 0.2f));
            for (int i = 0; i < componentsProp.arraySize; i++)
            {
                DrawComponentData(componentsProp.GetArrayElementAtIndex(i), i);
            }
            GUILayout.EndVertical();
            DrawFooter(target);
        }
        private void DrawTop(ITemplateInternal target)
        {
            if (GUILayout.Button("Add Component", GUILayout.Height(24f)))
            {
                Init();
                genericMenu.ShowAsContext();
            }
        }
        private void DrawFooter(ITemplateInternal target)
        {
            if (GUILayout.Button("Clear", GUILayout.Height(24f)))
            {
                Init();
                serializedObject.FindProperty(target.ComponentsPropertyName).ClearArray();
                serializedObject.ApplyModifiedProperties();
            }
        }
        private void DrawComponentData(SerializedProperty componentRefProp, int index)
        {
            IComponentTemplate template = componentRefProp.managedReferenceValue as IComponentTemplate;
            if (template == null)
            {
                DrawDamagedComponent(componentRefProp, index);
                return;
            }

            if (componentRefProp.managedReferenceValue == null)
            {
                DrawDamagedComponent(componentRefProp, index);
                return;
            }

            Type componentType;
            SerializedProperty componentProperty = componentRefProp;
            ComponentTemplateBase customInitializer = componentProperty.managedReferenceValue as ComponentTemplateBase;
            if (customInitializer != null)
            {
                componentProperty = componentRefProp.FindPropertyRelative("component");
                componentType = customInitializer.GetType().GetField("component", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FieldType;
            }
            else
            {
                componentType = componentProperty.managedReferenceValue.GetType(); ;
            }

            string name = template.Name;
            string description = template.Description;
            Color panelColor = template.Color.Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            EditorGUI.BeginChangeCheck();

            GUIContent label = new GUIContent(name);
            bool isEmpty = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;

            float width = EditorGUIUtility.currentViewWidth;
            float height = isEmpty ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(componentProperty, label, true);
            float padding = EditorGUIUtility.standardVerticalSpacing;

            Rect propertyFullRect = GUILayoutUtility.GetRect(width, height + padding * 3f);
            propertyFullRect = RectUtility.AddPadding(propertyFullRect, padding / 2f);
            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

            var (propertyRect, controlRect) = RectUtility.HorizontalSliceRight(propertyFullRect, RemoveButtonRect.width);
            var (removeButtonRect, _) = RectUtility.VerticalSliceTop(controlRect, RemoveButtonRect.height);

            #region Draw Component Block 
            EditorGUI.DrawRect(propertyFullRect, alphaPanelColor);
            propertyRect = RectUtility.AddPadding(propertyRect, padding);
            if (isEmpty)
            {
                GUI.Label(propertyRect, label);
            }
            else
            {
                EditorGUI.PropertyField(propertyRect, componentProperty, label, true);
            }
            if (GUI.Button(removeButtonRect, "x"))
            {
                OnRemoveComponentAt(index);
            }
            #endregion

            if (!string.IsNullOrEmpty(description))
            {
                Rect tooltipIconRect = TooltipIconRect;
                tooltipIconRect.center = new Vector2(propertyRect.xMax - removeButtonRect.width / 2f, propertyRect.yMin + removeButtonRect.height / 2f);
                GUIContent descriptionLabel = new GUIContent("( i )", description);
                GUI.Label(tooltipIconRect, descriptionLabel, EditorStyles.boldLabel);
            }

            if (EditorGUI.EndChangeCheck())
            {
                componentProperty.serializedObject.ApplyModifiedProperties();
                componentProperty.serializedObject.SetIsDifferentCacheDirty();
            }
        }

        private void DrawDamagedComponent(SerializedProperty componentRefProp, int index)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.HelpBox($"Damaged component. If the problem occurred after renaming a component or initializer. use MovedFromAttrubute", MessageType.Warning);

            Rect lastrect = GUILayoutUtility.GetLastRect();
            Rect removeButtonRect = RemoveButtonRect;
            removeButtonRect.center = new Vector2(lastrect.xMax + removeButtonRect.width, lastrect.yMin + removeButtonRect.height / 2f);

            GUILayout.Label("", GUILayout.Width(removeButtonRect.width));
            if (GUI.Button(removeButtonRect, "x", removeButtonStyle))
                OnRemoveComponentAt(index);

            GUILayout.EndHorizontal();
        }

        public string GetLastPathComponent(string input)
        {
            int lastSlashIndex = input.LastIndexOfAny(new char[] { '/', '\\' });
            if (lastSlashIndex == -1)
                return input;
            else
                return input.Substring(lastSlashIndex + 1);
        }
    }

    [CustomEditor(typeof(ScriptableEntityTemplate), true)]
    public class EntityTemplatePresetEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    public class EntityTemplateEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
}
#endif    
