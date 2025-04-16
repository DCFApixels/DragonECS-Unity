#if UNITY_EDITOR
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal struct DragonGUIContent
    {
        public static readonly DragonGUIContent Empty = new DragonGUIContent();
        public GUIContent value;
        public DragonGUIContent(GUIContent value) { this.value = value; }
        public DragonGUIContent(Texture texture)
        {
            value = UnityEditorUtility.GetLabelOrNull(texture);
        }
        public DragonGUIContent(Texture texture, string tooltip)
        {
            value = UnityEditorUtility.GetLabelOrNull(texture);
            if (value != null)
            {
                value.tooltip = tooltip;
            }
        }
        public DragonGUIContent(string text)
        {
            value = UnityEditorUtility.GetLabelOrNull(text);
        }
        public DragonGUIContent(string text, string tooltip)
        {
            value = UnityEditorUtility.GetLabelOrNull(text);
            if (value != null)
            {
                value.tooltip = tooltip;
            }
        }
        public static implicit operator DragonGUIContent(GUIContent a) { return new DragonGUIContent(a); }
        public static implicit operator DragonGUIContent(Texture a) { return new DragonGUIContent(a); }
        public static implicit operator DragonGUIContent(string a) { return new DragonGUIContent(a); }
        public static implicit operator GUIContent(DragonGUIContent a) { return a.value; }
    }
}
#endif