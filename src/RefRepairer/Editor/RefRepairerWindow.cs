#if UNITY_EDITOR
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
        static void Open()
        {
            var wnd = GetWindow<RefRepairerWindow>();
            wnd.titleContent = new GUIContent(TITLE);
            wnd.minSize = new Vector2(100f, 120f);
            wnd.Show();
        }

        private MissingRefContainer _missingRefContainer = new MissingRefContainer();
        private MissingsResolvingData[] _cachedMissingsResolvingDatas = null;

        private ReorderableList _reorderableList;

        private void InitList()
        {
            if(_reorderableList == null)
            {
                _reorderableList = new ReorderableList(_cachedMissingsResolvingDatas, typeof(MissingsResolvingData), false, false, false, false);
                _reorderableList.headerHeight = 0;
                _reorderableList.footerHeight = 0;
            }
            _reorderableList.list = _cachedMissingsResolvingDatas;
        }

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
                Debug.Log(_missingRefContainer.IsEmplty);
                var x = _missingRefContainer.collectedMissingTypesBuffer[0];
                Debug.Log(x.ResolvingData.NewTypeData.AutoToString());
                RepaireFileUtility.RepaieAsset(_missingRefContainer);
            }

            if (_missingRefContainer.MissingsResolvingDatas.Count != _cachedMissingsResolvingDatas.Length)
            {
                _cachedMissingsResolvingDatas = _missingRefContainer.MissingsResolvingDatas.Values.ToArray();
                InitList();
            }

            _reorderableList.DoLayoutList();
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