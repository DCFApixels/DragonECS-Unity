#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace DCFApixels.DragonECS.Editors
{
    public sealed class TemplateGenerator
    {
        private const int MENU_ITEM_PRIORITY = -198;

        private const string TITLE = "DragonECS Template Generator";

        private const string MENU_ITEM_PATH = "Assets/Create/DragonECS/";

        private const string NAMESPACE_TAG = "#NAMESPACE#";
        private const string SCRIPTANAME_TAG = "#SCRIPTNAME#";

        #region Properties
        private static Texture2D ScriptIcon => EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
        private static string GetCurrentFilePath([System.Runtime.CompilerServices.CallerFilePath] string fileName = null)
        {
            return Path.GetFullPath(Path.Combine(fileName, "../"));
        }
        private static string TemplatesPath => GetCurrentFilePath();
        #endregion

        #region GenerateMethods
        [MenuItem(MENU_ITEM_PATH + "[Template]Startup", false, MENU_ITEM_PRIORITY)]
        public static void CreateSturtupScript() => CreateScript("Startup");

        [MenuItem(MENU_ITEM_PATH + "[Template]System", false, MENU_ITEM_PRIORITY)]
        public static void CreateSystemSimpleScript() => CreateScript("System");

        [MenuItem(MENU_ITEM_PATH + "[Template]Component", false, MENU_ITEM_PRIORITY)]
        public static void CreateComponentSimpleScript() => CreateScript("Component");

        [MenuItem(MENU_ITEM_PATH + "[Template]Runner", false, MENU_ITEM_PRIORITY)]
        public static void CreateRunnerSimpleScript() => CreateScript("Runner");

        [MenuItem(MENU_ITEM_PATH + "[Template]System Extended", false, MENU_ITEM_PRIORITY)]
        public static void CreateSystemScript() => CreateScript("SystemExtended");

        [MenuItem(MENU_ITEM_PATH + "[Template]Component Extended", false, MENU_ITEM_PRIORITY)]
        public static void CreateComponentScript() => CreateScript("ComponentExtended");

        [MenuItem(MENU_ITEM_PATH + "[Template]Runner Extended", false, MENU_ITEM_PRIORITY)]
        public static void CreateRunnerScript() => CreateScript("RunnerExtended");


        private static void CreateScript(string templateName)
        {
            CreateAndRenameAsset($"{GetAssetPath()}/Ecs{templateName}.cs", name => GenerateEndWrtieScript($"{templateName}.cs.txt", name));
        }
        #endregion

        private static void GenerateEndWrtieScript(string templateFileName, string generatedFileName)
        {
            string script;
            try
            {
                script = File.ReadAllText(Path.Combine(TemplatesPath, templateFileName));
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog(TITLE, $"[ERROR] Template {templateFileName} cannot be read.\r\n{exception.Message}", "Close");
                return;
            }

            var ns = EditorSettings.projectGenerationRootNamespace.Trim();
            if (string.IsNullOrEmpty(ns))
            {
                ns = "Client";
            }

            script = script.Replace(NAMESPACE_TAG, ns);
            script = script.Replace(SCRIPTANAME_TAG, NormalizeClassName(Path.GetFileNameWithoutExtension(generatedFileName)));

            try
            {
                File.WriteAllText(AssetDatabase.GenerateUniqueAssetPath(generatedFileName), script);
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog(TITLE, $"[ERROR] The result was not written to the file.\r\n{exception.Message}", "Close");
                return;
            }
            if (EditorPrefs.GetBool("kAutoRefresh")) AssetDatabase.Refresh();
        }

        private static string NormalizeClassName(string className)
        {
            StringBuilder result = new StringBuilder();
            bool isUpper = true;
            foreach (var c in className)
            {
                if (char.IsLetterOrDigit(c))
                {
                    result.Append(isUpper ? char.ToUpperInvariant(c) : c);
                    isUpper = false;
                }
                else
                {
                    isUpper = true;
                }
            }
            return result.ToString();
        }

        private static string GetAssetPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path) && AssetDatabase.Contains(Selection.activeObject))
            {
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = Path.GetDirectoryName(path);
                }
            }
            else
            {
                path = "Assets";
            }
            return path;
        }

        private static void CreateAndRenameAsset(string pathName, Action<string> onSuccess)
        {
            var action = ScriptableObject.CreateInstance<CustomEndNameAction>();
            action.Callback = onSuccess;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, pathName, ScriptIcon, null);
        }

        #region Utils
        private sealed class CustomEndNameAction : EndNameEditAction
        {
            [NonSerialized]
            public Action<string> Callback;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                if (string.IsNullOrEmpty(pathName))
                {
                    EditorUtility.DisplayDialog(TITLE, "Invalid filename", "Close");
                    return;
                }
                Callback?.Invoke(pathName);
            }
        }
        #endregion        
    }
}
#endif

