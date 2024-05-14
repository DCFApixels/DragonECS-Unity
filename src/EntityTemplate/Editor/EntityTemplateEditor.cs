#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal abstract class EntityTemplateEditorBase : Editor
    {
        private static readonly Rect RemoveButtonRect = new Rect(0f, 0f, 19f, 19f);
        private static readonly Rect TooltipIconRect = new Rect(0f, 0f, 19f, 19f);

        private GUIStyle _removeButtonStyle;
        private GenericMenu _genericMenu;
        private bool _isInit = false;

        private static AutoColorMode AutoColorMode
        {
            get { return SettingsPrefs.instance.AutoColorMode; }
            set { SettingsPrefs.instance.AutoColorMode = value; }
        }

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
                if (dummy.Type.GetCustomAttribute<SerializableAttribute>() == null)
                {
                    Debug.LogWarning($"Type {dummy.Type.Name} does not have the [Serializable] attribute");
                    continue;
                }
                ITypeMeta meta = dummy is ITypeMeta metaOverride ? metaOverride : dummy.Type.ToMeta();
                string name = meta.Name;
                string description = meta.Description.Text;
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
                DrawComponentData(componentsProp.GetArrayElementAtIndex(i), componentsProp.arraySize, i);
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

        private void DrawComponentData(SerializedProperty componentRefProp, int total, int index)
        {
            IComponentTemplate template = componentRefProp.managedReferenceValue as IComponentTemplate;
            if (template == null || componentRefProp.managedReferenceValue == null)
            {
                DrawDamagedComponent_Replaced(componentRefProp, index);
                return;
            }

            Type componentType;
            SerializedProperty componentProperty = componentRefProp;
            try
            {
                ComponentTemplateBase customTemplate = componentProperty.managedReferenceValue as ComponentTemplateBase;
                if (customTemplate != null)
                {
                    componentProperty = componentRefProp.FindPropertyRelative("component");
                    componentType = customTemplate.GetType().GetField("component", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FieldType;
                }
                else
                {
                    componentType = componentProperty.managedReferenceValue.GetType();
                }

                if (componentType == null || componentProperty == null)
                {
                    throw new NullReferenceException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, serializedObject.targetObject);
                DrawDamagedComponent(index, "Damaged component template.");
                return;
            }


            //сюда попадают уже валидные компоненты


            ITypeMeta meta = template is ITypeMeta metaOverride ? metaOverride : template.Type.ToMeta();
            string name = meta.Name;
            string description = meta.Description.Text;

            bool isEmpty = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length <= 0;
            var propsCounter = componentProperty.Copy();
            int lastDepth = propsCounter.depth;
            bool next = propsCounter.Next(true) && lastDepth < propsCounter.depth;
            int propCount = next ? -1 : 0;
            while (next)
            {
                propCount++;
                next = propsCounter.Next(false);
            }
            float padding = EditorGUIUtility.standardVerticalSpacing;

            Color panelColor;
            if (meta.IsCustomColor)
            {
                panelColor = meta.Color.ToUnityColor();
            }
            else
            {
                switch (AutoColorMode)
                {
                    case AutoColorMode.Name:
                        panelColor = meta.Color.ToUnityColor().Desaturate(0.48f) / 1.18f; //.Desaturate(0.48f) / 1.18f;
                        break;
                    case AutoColorMode.Rainbow:
                        Color hsv = Color.HSVToRGB(1f / (Mathf.Max(total, EscEditorConsts.AUTO_COLOR_RAINBOW_MIN_RANGE)) * index, 1, 1);
                        panelColor = hsv.Desaturate(0.48f) / 1.18f;
                        break;
                    default:
                        panelColor = index % 2 == 0 ? new Color(0.40f, 0.40f, 0.40f) : new Color(0.54f, 0.54f, 0.54f);
                        break;
                }
            }
            panelColor = panelColor.Desaturate(EscEditorConsts.COMPONENT_DRAWER_DESATURATE);

            Color alphaPanelColor = panelColor;
            alphaPanelColor.a = EscEditorConsts.COMPONENT_DRAWER_ALPHA;

            Rect removeButtonRect = GUILayoutUtility.GetLastRect();

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical(UnityEditorUtility.GetStyle(alphaPanelColor));

            #region Draw Component Block 
            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += RemoveButtonRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - RemoveButtonRect.width;
            removeButtonRect.center += Vector2.up * padding * 2f;

            bool isRemoveComponent = EcsGUI.CloseButton(removeButtonRect);

            if (propCount <= 0)
            {
                GUIContent label = UnityEditorUtility.GetLabel(name);
                EditorGUILayout.LabelField(name);
                Rect emptyPos = GUILayoutUtility.GetLastRect();
                emptyPos.xMin += EditorGUIUtility.labelWidth;
                if (isEmpty)
                {
                    using (new EcsGUI.ContentColorScope(1f, 1f, 1f, 0.4f))
                    {
                        GUI.Label(emptyPos, "empty");
                    }
                }
                EditorGUI.BeginProperty(GUILayoutUtility.GetLastRect(), label, componentRefProp);
                EditorGUI.EndProperty();
            }
            else
            {
                GUIContent label = UnityEditorUtility.GetLabel(name);
                if (componentProperty.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.PropertyField(componentProperty, label, true);
                }
                else
                {
                    Rect r = RectUtility.AddPadding(GUILayoutUtility.GetRect(label, EditorStyles.objectField), 0, 20f, 0, 0);
                    EditorGUI.PropertyField(r, componentProperty, label, true);
                }
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
        private void DrawDamagedComponent_Replaced(SerializedProperty componentRefProp, int index)
        {
            DrawDamagedComponent(index, $"Damaged component template. If the problem occurred after renaming a component or initializer. use MovedFromAttrubute");
        }
        private void DrawDamagedComponent(int index, string message)
        {
            Rect removeButtonRect = GUILayoutUtility.GetLastRect();

            GUILayout.BeginHorizontal();

            float padding = EditorGUIUtility.standardVerticalSpacing;

            removeButtonRect.yMin = removeButtonRect.yMax;
            removeButtonRect.yMax += RemoveButtonRect.height;
            removeButtonRect.xMin = removeButtonRect.xMax - RemoveButtonRect.width;
            removeButtonRect.center += Vector2.up * padding * 2f;

            bool isRemoveComponent = EcsGUI.CloseButton(removeButtonRect);

            EditorGUILayout.HelpBox(message, MessageType.Warning);

            if (isRemoveComponent)
            {
                OnRemoveComponentAt(index);
            }

            GUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(ScriptableEntityTemplate), true)]
    internal class EntityTemplatePresetEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
    [CustomEditor(typeof(MonoEntityTemplate), true)]
    internal class EntityTemplateEditor : EntityTemplateEditorBase
    {
        public override void OnInspectorGUI()
        {
            Draw((ITemplateInternal)target);
        }
    }
}
#endif    
