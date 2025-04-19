#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using DCFApixels.DragonECS.Unity.RefRepairer.Editors;
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class DeepDebuggerWindow : EditorWindow
    {
        public const string TITLE = nameof(DeepDebuggerWindow);

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

        public enum Page
        {
            ScriptsCache,
            MetaIDRegistry,
        }
        private Page _page;
        private Vector2 pos;
        private void OnGUI()
        {
            _page = (Page)EditorGUILayout.EnumPopup(_page);

            switch (_page)
            {
                case Page.ScriptsCache:
                    DrawScriptsCache();
                    break;
                case Page.MetaIDRegistry:
                    DrawMetaIDRegistry();
                    break;
            }
        }

        private void DrawScriptsCache()
        {
            if (GUILayout.Button("Reset"))
            {
                ScriptsCache.Reinit();
            }
            var dicst = ScriptsCache.MetaIDScriptPathPairs;
            pos = GUILayout.BeginScrollView(pos);
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

        private void DrawMetaIDRegistry()
        {
            if (GUILayout.Button("Reset"))
            {
                MetaIDRegistry.instance.Reinit();
            }
            var dicst = MetaIDRegistry.instance.TypeKeyMetaIDPairs;
            pos = GUILayout.BeginScrollView(pos);
            foreach (var (typeData, scriptPath) in dicst)
            {
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                Rect rect = GUILayoutUtility.GetLastRect();
                var (leftRect, rightRect) = rect.HorizontalSliceLerp(0.5f);
                Rect preLeftRect = default;
                (preLeftRect, leftRect) = rect.HorizontalSliceLeft(18f);
                GUI.Label(preLeftRect, typeData.ToType() == null ? "-" : "+");
                GUI.Label(leftRect, typeData.ToString());
                GUI.Label(rightRect, scriptPath);
            }
            GUILayout.EndScrollView();
        }
    }
}
#endif
