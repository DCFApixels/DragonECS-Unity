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
                    {
                        return;
                    }
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
            GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
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

            Rect removeButtonRect = GUILayoutUtility.GetLastRect();

            GUIContent label = new GUIContent(name);
            bool isEmpty = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;
            float padding = EditorGUIUtility.standardVerticalSpacing;
            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical(EcsEditor.GetStyle(alphaPanelColor));

            #region Draw Component Block 
            bool isRemoveComponent = false;
            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += RemoveButtonRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - RemoveButtonRect.width;
            removeButtonRect.center += Vector2.up * padding * 2f;
            if (GUI.Button(removeButtonRect, "x"))
            {
                isRemoveComponent = true;
            }

            if (isEmpty)
            {
                GUILayout.Label(label);
            }
            else
            {
                EditorGUILayout.PropertyField(componentProperty, label, true);
            }
            if (isRemoveComponent)
            {
                OnRemoveComponentAt(index);
            }
            if (!string.IsNullOrEmpty(description))
            {
                Rect tooltipIconRect = TooltipIconRect;
                tooltipIconRect.center = removeButtonRect.center;
                tooltipIconRect.center -= Vector2.right * tooltipIconRect.width;
                GUIContent descriptionLabel = new GUIContent(EcsUnityConsts.INFO_MARK, description);
                GUI.Label(tooltipIconRect, descriptionLabel, EditorStyles.boldLabel);
            }
            #endregion

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                componentProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(componentProperty.serializedObject.targetObject);
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
            {
                OnRemoveComponentAt(index);
            }

            GUILayout.EndHorizontal();
        }

        public string GetLastPathComponent(string input)
        {
            int lastSlashIndex = input.LastIndexOfAny(new char[] { '/', '\\' });
            if (lastSlashIndex == -1)
            {
                return input;
            }
            else
            {
                return input.Substring(lastSlashIndex + 1);
            }
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
