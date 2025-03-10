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

        private static Color _emptyColor = new Color32(255, 0, 75, 100);
        private static Color _destroyedColor = new Color32(255, 75, 0, 100);
        private static Color _aliveColor = new Color32(75, 255, 0, 100);

        public override void OnInspectorGUI()
        {
            EcsWorld world = Target.GetCurrentWorldRaw();

            Color labelBackColor;
            string labelText;

            if (world == null)
            {
                labelBackColor = _emptyColor;
                labelText = "Is Empty";
            }
            else
            {
                if (world.IsDestroyed)
                {
                    labelBackColor = _destroyedColor;
                    labelText = $"{world.GetMeta().Name} ( {world.ID} ) Destroyed";
                }
                else
                {
                    labelBackColor = _aliveColor;
                    labelText = $"{world.GetMeta().Name} ( {world.ID} )";
                }
            }

            using (EcsGUI.SetBackgroundColor(labelBackColor))
            {
                GUILayout.Box("Is Empty", UnityEditorUtility.GetWhiteStyle(), GUILayout.ExpandWidth(true));
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