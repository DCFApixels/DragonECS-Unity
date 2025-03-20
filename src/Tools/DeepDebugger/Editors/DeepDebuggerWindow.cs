#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class DeepDebuggerWindow : EditorWindow
    {
        public const string TITLE = "DeepDebuggerWindow";

#if DRAGONECS_DEEP_DEBUG
        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/" + TITLE)]
        static void Open()
        {
            var wnd = GetWindow<DeepDebuggerWindow>();
            wnd.titleContent = new GUIContent(TITLE);
            wnd.minSize = new Vector2(100f, 120f);
            wnd.Show();
        }
#endif

        private Vector2 pos;
        private void OnGUI()
        {
            var dicst = ScriptsCache.MetaIDScriptPathPairs;

            pos = GUILayout.BeginScrollView(pos);

            if (GUILayout.Button("Reset"))
            {
                ScriptsCache.Reinit();
            }

            foreach (var (metaID, scriptPath) in dicst)
            {
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                var (leftRect, rightRect) = rect.HorizontalSliceLerp(0.5f);
                GUI.Label(leftRect, metaID);
                GUI.Label(rightRect, scriptPath);
            }
            GUILayout.EndScrollView();
        }
    }
}
#endif
