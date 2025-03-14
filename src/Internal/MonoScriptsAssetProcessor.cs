#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal class MonoScriptsAssetProcessor : AssetPostprocessor
    {
        private static long _timeTicks;
        public static long Version
        {
            get { return _timeTicks; }
        }
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            _removedScriptGuids.Clear();
            _newScriptIDs.Clear();

            foreach (string str in importedAssets)
            {
                //Debug.Log("Reimported Asset: " + str);
                ProcessAssetPath(str);
            }
            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
                RemoveAssetPath(str);
            }

            for (int i = 0; i < movedFromAssetPaths.Length; i++)
            {
                RemoveAssetPath(movedFromAssetPaths[i]);
            }
            for (int i = 0; i < movedAssets.Length; i++)
            {
                //Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
                ProcessAssetPath(movedAssets[i]);
            }

            //if (didDomainReload)
            //{
            //    Debug.Log("Domain has been reloaded");
            //}


            _timeTicks = DateTime.Now.Ticks;
        }


        private static List<string> _removedScriptGuids = new List<string>();
        private static List<string> _newScriptIDs = new List<string>();
        public static IReadOnlyCollection<string> RemovedScriptPaths
        {
            get { return _removedScriptGuids; }
        }
        public static IReadOnlyCollection<string> NewScriptPaths
        {
            get { return _newScriptIDs; }
        }

        private static void RemoveAssetPath(string filePath)
        {
            if (IsScript(filePath) == false) { return; }
            //Debug.Log("RemoveAssetPath: " + filePath);
            _removedScriptGuids.Add(filePath);
        }

        private static void ProcessAssetPath(string filePath)
        {
            if (IsScript(filePath) == false) { return; }
            //Debug.Log("ProcessAssetPath: " + filePath);

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath).text;
            _newScriptIDs.Add(filePath);
        }

        private static bool IsScript(string filePath)
        {
            if (filePath.Length <= 3) { return false; }
            int i = filePath.Length - 3;
            return filePath[i++] == '.'
                && filePath[i++] == 'c'
                && filePath[i++] == 's';
        }
    }
}
#endif