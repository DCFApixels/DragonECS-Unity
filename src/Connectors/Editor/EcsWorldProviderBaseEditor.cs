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
            base.OnInspectorGUI();
            if (Target.IsEmpty)
            {
                var style = EcsEditor.GetStyle(new Color32(255, 0, 75, 100));
                GUILayout.Box("Is Empty", style, GUILayout.ExpandWidth(true));
            }
            else
            {
                var style = EcsEditor.GetStyle(new Color32(75, 255, 0, 100));
                EcsWorld world = Target.GetRaw();
                GUILayout.Box($"{world.GetMeta().Name} ( {world.id} )", style, GUILayout.ExpandWidth(true));
            }
        }
    }
}
#endif