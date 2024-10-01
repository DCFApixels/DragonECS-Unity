#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using DCFApixels.DragonECS.Unity.RefRepairer.Editors;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class RefRepairerWindow : EditorWindow
    {
        public const string TITLE = "Reference Repairer";
        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/" + TITLE)]
        public static void Open()
        {
            var wnd = GetWindow<RefRepairerWindow>();
            wnd.titleContent = new GUIContent(TITLE);
            wnd.minSize = new Vector2(140f, 140f);
            wnd.Show();
        }

        private MissingRefContainer _missingRefContainer = new MissingRefContainer();
        private MissingsResolvingData[] _cachedMissingsResolvingDatas = null;
        private MissingsResolvingData _selectedMissingsResolvingData = null;
        private ReorderableList _reorderableResolvingDataList;

        private float _lineWidth = 0f;
        private float _arrowWidth = 0f;

        private void InitList()
        {
            if (_reorderableResolvingDataList == null)
            {
                _reorderableResolvingDataList = new ReorderableList(_cachedMissingsResolvingDatas, typeof(MissingsResolvingData), false, false, false, false);
                _reorderableResolvingDataList.headerHeight = 0;
                _reorderableResolvingDataList.footerHeight = 0;

                _reorderableResolvingDataList.elementHeightCallback += OnReorderableResolvingDataListGetHeight;
                _reorderableResolvingDataList.drawElementBackgroundCallback += OnReorderableResolvingDataListDrawElement;
                _reorderableResolvingDataList.drawElementCallback += OnReorderableClearDrawElement;
                _reorderableResolvingDataList.showDefaultBackground = false;
            }
            _reorderableResolvingDataList.list = _cachedMissingsResolvingDatas;

            _lineWidth = GUI.skin.label.CalcSize(UnityEditorUtility.GetLabel("Namespace:")).x;
            _arrowWidth = GUI.skin.button.CalcSize(UnityEditorUtility.GetLabel("->")).x;
        }

        private readonly Color _activedColor = new Color(0f, 0.6f, 1f, 0.36f);
        private readonly Color _dragColor = new Color(0f, 0.60f, 1f, 0.16f);
        private float OnReorderableResolvingDataListGetHeight(int index)
        {
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3f + EditorGUIUtility.standardVerticalSpacing;
        }
        private void OnReorderableClearDrawElement(Rect rect, int index, bool isActive, bool isFocused) { }
        private void OnReorderableResolvingDataListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _cachedMissingsResolvingDatas.Length) { return; }
            ref var data = ref _cachedMissingsResolvingDatas[index];

            using (EcsGUI.SetAlpha(0))
            {
                if (GUI.Button(rect, ""))
                {
                    _selectedMissingsResolvingData = _cachedMissingsResolvingDatas[index];
                }
            }

            if (isActive)
            {
                EditorGUI.DrawRect(rect, _activedColor);
            }
            if (EcsGUI.HitTest(rect))
            {
                EditorGUI.DrawRect(rect, _dragColor);
            }

            rect = rect.AddPadding(EditorGUIUtility.standardVerticalSpacing, 0);
            Rect infoRect;
            (infoRect, rect) = rect.HorizontalSliceLeft(20f);
            if (data.IsResolved)
            {
                GUI.Label(infoRect, UnityEditorUtility.GetLabel(Icons.Instance.PassIcon, "Valid type"));
            }
            else if (data.IsEmpty)
            {
                GUI.Label(infoRect, UnityEditorUtility.GetLabel(Icons.Instance.WarningIcon, "Is empty"));
            }
            else
            {
                GUI.Label(infoRect, UnityEditorUtility.GetLabel(Icons.Instance.ErrorIcon, "Type not found"));
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            rect = rect.Move(0, EditorGUIUtility.standardVerticalSpacing);
            DrawLine(rect, "Name:", data.OldTypeData.ClassName, data.NewTypeData.ClassName);
            rect = rect.Move(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            DrawLine(rect, "Namespace:", data.OldTypeData.NamespaceName, data.NewTypeData.NamespaceName);
            rect = rect.Move(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            DrawLine(rect, "Assembly:", data.OldTypeData.AssemblyName, data.NewTypeData.AssemblyName);

            rect.yMin = rect.yMax - 2f;
            rect = rect.Move(0, EditorGUIUtility.standardVerticalSpacing * 2f);
            rect.xMin -= infoRect.width;
            //EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));
        }
        private void DrawLine(Rect rect, string name, string oldStr, string newStr)
        {
            Rect nameRect, oldRect, arrowRect, newRect;

            (nameRect, rect) = rect.HorizontalSliceLeft(_lineWidth);
            float tmp = (rect.width - _arrowWidth) / 2f;
            (oldRect, rect) = rect.HorizontalSliceLeft(tmp);
            (arrowRect, rect) = rect.HorizontalSliceLeft(_arrowWidth);
            (newRect, rect) = rect.HorizontalSliceLeft(tmp);

            GUI.Label(nameRect, name);
            EditorGUI.DrawRect(oldRect, new Color(0, 0, 0, 0.16f));
            GUI.Label(oldRect, oldStr);
            using (EcsGUI.SetAlignment(GUI.skin.label, TextAnchor.MiddleCenter))
            {
                GUI.Label(arrowRect, "->");
            }
            EditorGUI.DrawRect(newRect, new Color(0, 0, 0, 0.16f));
            GUI.Label(newRect, newStr);
        }
        private string DrawEditableLine(Rect rect, string name, string oldStr, string newStr)
        {
            Rect nameRect, oldRect, arrowRect, newRect;

            (nameRect, rect) = rect.HorizontalSliceLeft(_lineWidth);
            float tmp = (rect.width - _arrowWidth) / 2f;
            (oldRect, rect) = rect.HorizontalSliceLeft(tmp);
            (arrowRect, rect) = rect.HorizontalSliceLeft(_arrowWidth);
            (newRect, rect) = rect.HorizontalSliceLeft(tmp);

            GUI.Label(nameRect, name);
            GUI.TextField(oldRect, oldStr);
            if (GUI.Button(arrowRect.AddPadding(2.5f, 0), "->"))
            {
                newStr = oldStr;
            }
            return EditorGUI.TextField(newRect, newStr);
        }


        private Vector2 _scrollViewPosition;
        private void OnGUI()
        {
            if (_missingRefContainer.IsEmplty)
            {
                if (GUILayout.Button("Collect missing references"))
                {
                    if (TryInit())
                    {
                        _missingRefContainer.Collect();
                        _cachedMissingsResolvingDatas = _missingRefContainer.MissingsResolvingDatas.Values.ToArray();
                        InitList();
                    }
                }
                return;
            }

            if (GUILayout.Button("Repaire missing references"))
            {
                _selectedMissingsResolvingData = null;
                RepaireFileUtility.RepaieAsset(_missingRefContainer);
            }

            if (_missingRefContainer.MissingsResolvingDatas.Count != _cachedMissingsResolvingDatas.Length)
            {
                _cachedMissingsResolvingDatas = _missingRefContainer.MissingsResolvingDatas.Values.ToArray();
                InitList();
            }

            var bc = EcsGUI.SetBackgroundColor(Color.black, 0.5f);
            using (EcsGUI.Layout.BeginVertical(EditorStyles.helpBox))
            {
                bc.Dispose();
                _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition, GUILayout.ExpandHeight(true));
                _reorderableResolvingDataList.DoLayoutList();
                GUILayout.EndScrollView();
            }



            GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
            //GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(false));

            var data = _selectedMissingsResolvingData;
            Rect rect = GUILayoutUtility.GetRect(
                EditorGUIUtility.currentViewWidth,
                (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3f + EditorGUIUtility.standardVerticalSpacing * 2f)
                .AddPadding(EditorGUIUtility.standardVerticalSpacing);


            if (data == null)
            {
                using (EcsGUI.SetAlignment(TextAnchor.MiddleCenter))
                {
                    GUI.Label(rect, "Select any record");
                }
            }
            else
            {
                using (EcsGUI.CheckChanged())
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect = rect.Move(0, EditorGUIUtility.standardVerticalSpacing);
                    string ClassName = DrawEditableLine(rect, "Name:", data.OldTypeData.ClassName, data.NewTypeData.ClassName);
                    rect = rect.Move(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    string NamespaceName = DrawEditableLine(rect, "Namespace:", data.OldTypeData.NamespaceName, data.NewTypeData.NamespaceName);
                    rect = rect.Move(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                    string AssemblyName = DrawEditableLine(rect, "Assembly:", data.OldTypeData.AssemblyName, data.NewTypeData.AssemblyName);
                    if (EcsGUI.Changed)
                    {
                        data.NewTypeData = new RefRepairer.Internal.TypeData(ClassName, NamespaceName, AssemblyName);
                    }
                }
            }

            GUILayout.EndVertical();
        }



        private bool TryInit()
        {
            var allCurrentDirtyScenes = EditorSceneManager
                .GetSceneManagerSetup()
                .Where(sceneSetup => sceneSetup.isLoaded)
                .Select(sceneSetup => EditorSceneManager.GetSceneByPath(sceneSetup.path))
                .Where(scene => scene.isDirty)
                .ToArray();

            if (allCurrentDirtyScenes.Length != 0)
            {
                bool result = EditorUtility.DisplayDialog(
                    "Current active scene(s) is dirty",
                    "Please save all active scenes as they may be overwritten",
                    "Save active scene and Continue",
                    "Cancel update"
                );
                if (result == false)
                    return false;

                foreach (var dirtyScene in allCurrentDirtyScenes)
                    EditorSceneManager.SaveScene(dirtyScene);
            }

            _missingRefContainer.Collect();
            return true;
        }
    }
}
#endif