#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.RefRepairer.Editors;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        private List<ContainerMissingTypes> _missingTypes;
        //private readonly CollectorMissingTypes _collectorMissingTypes = new CollectorMissingTypes();

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

            //_missingTypes = _collectorMissingTypes.Collect();
            return true;
        }
    }
}
#endif