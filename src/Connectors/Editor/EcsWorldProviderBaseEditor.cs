#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(EcsWorldProviderBase), true)]
    [CanEditMultipleObjects]
    public class EcsWorldProviderBaseEditor : Editor
    {
        private EcsWorldProviderBase Target => (EcsWorldProviderBase)target;

        public override void OnInspectorGUI()
        {
            if (Target.IsEmpty)
            {
                var style = UnityEditorUtility.GetStyle(new Color32(255, 0, 75, 100));
                GUILayout.Box("Is Empty", style, GUILayout.ExpandWidth(true));
            }
            else
            {
                var style = UnityEditorUtility.GetStyle(new Color32(75, 255, 0, 100));
                EcsWorld world = Target.GetRaw();
                GUILayout.Box($"{world.GetMeta().Name} ( {world.id} )", style, GUILayout.ExpandWidth(true));
            }
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Destroy"))
            {
                var w = Target.GetRaw();
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
        }
    }
}
#endif