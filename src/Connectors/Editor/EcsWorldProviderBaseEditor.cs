#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsWorldProviderBase), true)]
    [CanEditMultipleObjects]
    internal class EcsWorldProviderBaseEditor : Editor
    {
        private EcsWorldProviderBase Target => (EcsWorldProviderBase)target;

        public override void OnInspectorGUI()
        {
            EcsWorld world = Target.GetCurrentWorldRaw();
            if (world == null)
            {
                var style = UnityEditorUtility.GetStyle(new Color32(255, 0, 75, 100));
                GUILayout.Box("Is Empty", style, GUILayout.ExpandWidth(true));
            }
            else
            {
                if (world.IsDestroyed)
                {
                    var style = UnityEditorUtility.GetStyle(new Color32(255, 75, 0, 100));
                    GUILayout.Box($"{world.GetMeta().Name} ( {world.ID} ) Destroyed", style, GUILayout.ExpandWidth(true));
                }
                else
                {
                    var style = UnityEditorUtility.GetStyle(new Color32(75, 255, 0, 100));
                    GUILayout.Box($"{world.GetMeta().Name} ( {world.ID} )", style, GUILayout.ExpandWidth(true));
                }
            }
            EcsGUI.Layout.DrawWorldBaseInfo(Target.GetCurrentWorldRaw());

            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Destroy & Clear"))
            {
                var w = Target.GetCurrentWorldRaw();
                if (w != null && w.IsDestroyed == false)
                {
                    w.Destroy();
                }
                Target.SetRaw(null);
            }

            Rect r = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 2f);
            Color c = Color.white;
            c.a = 0.3f;
            EditorGUI.DrawRect(r, c);
            GUILayout.Space(10);

            EcsGUI.Layout.DrawWorldComponents(Target.GetCurrentWorldRaw());
        }
    }
}
#endif