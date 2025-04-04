#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{

    internal class MetaIDGenerator : EditorWindow
    {
        public const string TITLE = nameof(MetaIDGenerator);

        [MenuItem("Tools/" + EcsConsts.FRAMEWORK_NAME + "/" + TITLE)]
        static void Open()
        {
            var wnd = GetWindow<MetaIDGenerator>();
            wnd.titleContent = new GUIContent(TITLE);
            wnd.minSize = new Vector2(100f, 120f);
            wnd.Show();
        }


        private string _lastID;
        private string _lastIDAttribute;
        private string _template;

        private void OnGUI()
        {
            EditorGUILayout.TextField("MetaID", _lastID);
            EditorGUILayout.TextField("Attribute", _lastIDAttribute);
            EditorGUILayout.TextField("Template Type", _template);

            if (GUILayout.Button("Generate new MetaID"))
            {
                _lastID = MetaID.GenerateNewUniqueID();
                _lastIDAttribute = MetaID.IDToAttribute(_lastID);
                _template = "Tempalte" + MetaID.ConvertIDToTypeName(_lastID);
                GUI.FocusControl(null);
            }
        }
    }
}
#endif