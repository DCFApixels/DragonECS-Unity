#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public abstract class EntityTemplateEditorBase : Editor
    {
        private static readonly Rect RemoveButtonRect = new Rect(0f, 0f, 19f, 19f);
        private static readonly Rect TooltipIconRect = new Rect(0f, 0f, 19f, 19f);

        private GUIStyle _removeButtonStyle;
        private GenericMenu _genericMenu;
        private bool _isInit = false;

        #region Init
        private void Init()
        {
            if (_genericMenu == null) { _isInit = false; }
            if (_isInit) { return; }

            var tmpstylebase = UnityEditorUtility.GetStyle(new Color(0.9f, 0f, 0.22f), 0.5f);
            var tmpStyle = UnityEditorUtility.GetStyle(new Color(1f, 0.5f, 0.7f), 0.5f);

            _removeButtonStyle = new GUIStyle(EditorStyles.linkLabel);
            _removeButtonStyle.alignment = TextAnchor.MiddleCenter;

            _removeButtonStyle.normal = tmpstylebase.normal;
            _removeButtonStyle.hover = tmpStyle.normal;
            _removeButtonStyle.active = tmpStyle.normal;
            _removeButtonStyle.focused = tmpStyle.normal;

            _removeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            _removeButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _removeButtonStyle.border = new RectOffset(0, 0, 0, 0);

            _genericMenu = new GenericMenu();

            var componentTemplateDummies = ComponentTemplateTypeCache.Dummies;
            foreach (var dummy in componentTemplateDummies)
            {
                ITypeMeta meta = dummy is ITypeMeta metaOverride ? metaOverride : dummy.Type.ToMeta();
                string name = meta.Name;
                string description = meta.Description;
                MetaGroup group = meta.Group;

                if (group.Name.Length > 0)
                {
                    name = group.Name + name;
                }

                if (string.IsNullOrEmpty(description) == false)
                {
                    name = $"{name} [i]";
                }
                _genericMenu.AddItem(new GUIContent(name, description), false, OnAddComponent, dummy);
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

            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(Color.black, 0.2f));
            DrawTop(target);
            GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
            for (int i = 0; i < componentsProp.arraySize; i++)
            {
                DrawComponentData(componentsProp.GetArrayElementAtIndex(i), i);
            }
            GUILayout.EndVertical();
        }
        private void DrawTop(ITemplateInternal target)
        {
            switch (EcsGUI.Layout.AddClearComponentButtons())
            {
                case EcsGUI.AddClearComponentButton.AddComponent:
                    Init();
                    _genericMenu.ShowAsContext();
                    break;
                case EcsGUI.AddClearComponentButton.Clear:
                    Init();
                    serializedObject.FindProperty(target.ComponentsPropertyName).ClearArray();
                    serializedObject.ApplyModifiedProperties();
                    break;
            }
        }

        private void DrawComponentData(SerializedProperty componentRefProp, int index)
        {
            IComponentTemplate template = componentRefProp.managedReferenceValue as IComponentTemplate;
            if (template == null || componentRefProp.managedReferenceValue == null)
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

            ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();
            string name = meta.Name;
            string description = meta.Description;
            Color panelColor = meta.Color.ToUnityColor().Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            //GUIContent label = new GUIContent(name);
            GUIContent label = UnityEditorUtility.GetLabel(name);
            bool isEmpty = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;
            float padding = EditorGUIUtility.standardVerticalSpacing;
            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

            Rect removeButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(alphaPanelColor));

            #region Draw Component Block 
            bool isRemoveComponent = false;
            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += RemoveButtonRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - RemoveButtonRect.width;
            removeButtonRect.center += Vector2.up * padding * 2f;

            if (EcsGUI.CloseButton(removeButtonRect))
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
            if (string.IsNullOrEmpty(description) == false)
            {
                Rect tooltipIconRect = TooltipIconRect;
                tooltipIconRect.center = removeButtonRect.center;
                tooltipIconRect.center -= Vector2.right * tooltipIconRect.width;
                EcsGUI.DescriptionIcon(tooltipIconRect, description);
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
            if (GUI.Button(removeButtonRect, "x", _removeButtonStyle))
            {
                OnRemoveComponentAt(index);
            }

            GUILayout.EndHorizontal();
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
