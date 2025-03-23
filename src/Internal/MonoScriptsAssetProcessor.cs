#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
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
                ProcessAssetPath(str);
            }
            foreach (string str in deletedAssets)
            {
                RemoveAssetPath(str);
            }
            foreach (var str in movedFromAssetPaths)
            {
                RemoveAssetPath(str);
            }
            foreach (string str in movedAssets)
            {
                ProcessAssetPath(str);
            }
            //if (didDomainReload)
            //{
            //    Debug.Log("Domain has been reloaded");
            //}
            _timeTicks = DateTime.Now.Ticks;
        }


        private static List<string> _removedScriptGuids = new List<string>();
        private static List<string> _newScriptIDs = new List<string>();
        public static ReadOnlyList<string> RemovedScriptPaths
        {
            get { return _removedScriptGuids; }
        }
        public static ReadOnlyList<string> NewScriptPaths
        {
            get { return _newScriptIDs; }
        }

        private static void RemoveAssetPath(string filePath)
        {
            if (IsScript(filePath) == false) { return; }
            _removedScriptGuids.Add(filePath);
        }

        private static void ProcessAssetPath(string filePath)
        {
            if (IsScript(filePath) == false) { return; }
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