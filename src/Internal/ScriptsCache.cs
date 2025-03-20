#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [FilePath(EcsUnityConsts.LOCAL_CACHE_FOLDER + "/" + nameof(ScriptsCache) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    internal class ScriptsCache : ScriptableSingleton<ScriptsCache>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private bool _isInit = false;
        [SerializeField]
        private long _version;

        #region [SerializeField]
        [SerializeField]
        private Pair[] _serializableMetaIDScriptPathPairs;
        [Serializable]
        private struct Pair
        {
            public string metaID;
            public string scriptPath;
            public Pair(string metaID, string scriptPath)
            {
                this.metaID = metaID;
                this.scriptPath = scriptPath;
            }
        }
        #endregion
        private static Dictionary<string, string> _metaIDScriptPathPairs = new Dictionary<string, string>();
        public static IReadOnlyDictionary<string, string> MetaIDScriptPathPairs
        {
            get
            {
                instance.InitUpdate();
                return _metaIDScriptPathPairs;
            }
        }

        private static SparseArray<MonoScript> _scriptsAssets = new SparseArray<MonoScript>(256);

        public static void Reinit()
        {
            instance._isInit = false;
            _metaIDScriptPathPairs.Clear();
            instance.InitUpdate();
        }

        #region Init/Update
        private static object _lock = new object();
        private void InitUpdate()
        {
            Init();
            if (MonoScriptsAssetProcessor.Version <= _version) { return; }
            lock (_lock)
            {
                if (MonoScriptsAssetProcessor.Version <= _version) { return; }

                var paths = MonoScriptsAssetProcessor.RemovedScriptPaths;
                if (paths.Count > 0)
                {
                    List<string> removedKeys = new List<string>();
                    foreach (var metaIDScriptPathPair in _metaIDScriptPathPairs)
                    {
                        for (int j = 0; j < paths.Count; j++)
                        {
                            if (paths[j] == metaIDScriptPathPair.Value)
                            {
                                removedKeys.Add(metaIDScriptPathPair.Key);
                            }
                        }
                    }

                    foreach (var key in removedKeys)
                    {
                        _metaIDScriptPathPairs.Remove(key);
                    }
                }

                paths = MonoScriptsAssetProcessor.NewScriptPaths;
                if (paths.Count > 0)
                {
                    List<string> metaIDs = new List<string>();
                    foreach (var assetPath in paths)
                    {
                        ExtractMetaIDs(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath).text, metaIDs);
                        foreach (var metaID in metaIDs)
                        {
                            _metaIDScriptPathPairs[metaID] = assetPath;
                        }
                    }
                }

                _version = MonoScriptsAssetProcessor.Version;

                Save(true);
            }
        }

        private void Init()
        {
            if (_isInit && _metaIDScriptPathPairs.Count > 0) { return; }

            if (_metaIDScriptPathPairs == null)
            {
                _metaIDScriptPathPairs = new Dictionary<string, string>();
            }

            _metaIDScriptPathPairs.Clear();
            var scriptGuids = AssetDatabase.FindAssets($"* t:MonoScript");

            List<string> metaIDsBuffer = new List<string>();

            foreach (var guid in scriptGuids)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                string script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath).text;

                if (scriptPath.EndsWith("MetaIDAttribute.cs") == false)
                {
                    ExtractMetaIDs(script, metaIDsBuffer);
                }

                foreach (var metaID in metaIDsBuffer)
                {
                    _metaIDScriptPathPairs[metaID] = scriptPath;
                }
                metaIDsBuffer.Clear();
            }

            //foreach (var pair in _metaIDScriptPathPairs)
            //{
            //    EcsDebug.PrintPass($"k:{pair.Key} v:{pair.Value}");
            //}

            _isInit = true;

            Save(true);
        }
        #endregion

        #region Get
        public static bool TryGetScriptAsset(TypeMeta meta, out MonoScript script)
        {
            int metaUniqueID = meta.GetHashCode();

            if (_scriptsAssets.TryGetValue(metaUniqueID, out script) == false)
            {
                script = null;

                //Ищем по мета айди совпадения
                if (UnityEditorUtility.IsHasAnyMetaIDCollision == false)
                {
                    if (meta.IsHasMetaID())
                    {
                        instance.InitUpdate();

                        string metaID = meta.MetaID;
                        if (_metaIDScriptPathPairs.TryGetValue(metaID, out string assetPath))
                        {
                            if (assetPath == null)
                            {
                                _metaIDScriptPathPairs.Remove(metaID);
                            }
                            else
                            {
                                MonoScript textAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                                if (textAsset != null)
                                {
                                    script = textAsset;
                                }
                            }
                        }
                    }
                }


                if (script == null)
                {
                    //Ищем совпадения имен в ассетах
                    string name = meta.TypeName;
                    int genericTypeCharIndex = name.IndexOf('<');
                    if (genericTypeCharIndex >= 0)
                    {
                        name = name.Substring(0, genericTypeCharIndex);
                    }
                    string[] guids = AssetDatabase.FindAssets($"{name} t:MonoScript");
                    string[] skipped = Array.Empty<string>();
                    int skippedCount = 0;

                    for (var i = 0; i < guids.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                        if (assetPath.IndexOf("Packages/com.unity.") == 0)
                        {
                            if (skippedCount == 0)
                            {
                                skipped = new string[guids.Length];
                            }
                            skipped[skippedCount++] = assetPath;
                            continue;
                        }
                        MonoScript textAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                        if (textAsset != null && textAsset.name == name)
                        {
                            script = textAsset;
                            break;
                        }
                    }

                    if (script == null)
                    {
                        foreach (var assetPath in new ReadOnlySpan<string>(skipped, 0, skippedCount))
                        {
                            MonoScript textAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                            if (textAsset != null && textAsset.name == name)
                            {
                                script = textAsset;
                                break;
                            }
                        }
                    }

                }


                _scriptsAssets.Add(metaUniqueID, script);
            }
            return script != null;
        }
        #endregion

        #region ParseUtils
        private static void ExtractMetaIDs(string script, List<string> outputList)
        {
            const string PATTERN = "MetaID";

            int lastIndex = 0;
            while (true)
            {
                int index = script.IndexOf(PATTERN, lastIndex) + PATTERN.Length;
                if (index < lastIndex || index < PATTERN.Length) { break; }
                lastIndex = index;
                for (int i = index; i < script.Length; i++)
                {
                    char chr = script[i];
                    if (char.IsWhiteSpace(chr) == false)
                    {
                        if (chr != '(')
                        {
                            index = -1;
                        }
                        break;
                    }
                }
                if (index < 0) { continue; }

                int startIndex = -1, endIndex = -1;
                bool isVerbal = false;
                for (int i = index; i < script.Length; i++)
                {
                    char chr = script[i];
                    if (chr == '@')
                    {
                        isVerbal = true;
                        if (script.Length <= ++i) { break; }
                        chr = script[i];
                    }
                    if (chr == '"')
                    {
                        if (script.Length <= ++i) { break; }
                        startIndex = i;
                        break;
                    }
                }
                if (startIndex < 0) { continue; }

                for (int i = startIndex; i < script.Length; i++)
                {
                    char chr = script[i];
                    if (chr == '\\')
                    {
                        if (script.Length <= ++i) { break; }
                        continue;
                    }

                    if (chr == '"')
                    {
                        if (isVerbal)
                        {
                            if (script.Length <= ++i) { break; }
                            if (script[i] != '"')
                            {
                                endIndex = i - 2;
                                break;
                            }
                        }
                        else
                        {
                            endIndex = --i;
                            break;
                        }
                    }
                }

                if (endIndex < startIndex) { continue; }

                string substring = script.Substring(startIndex, endIndex - startIndex + 1);
                if (isVerbal)
                {
                    outputList.Add(substring.Replace("\"\"", "\""));
                }
                else
                {
                    outputList.Add(Regex.Unescape(substring));
                }
            }
        }


        private static bool IsScript(string filePath)
        {
            int i = filePath.Length - 3;
            return filePath[i++] == '.'
                && filePath[i++] == 'c'
                && filePath[i++] == 's';
        }
        #endregion

        #region ISerializationCallbackReceiver
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _serializableMetaIDScriptPathPairs = new Pair[_metaIDScriptPathPairs.Count];
            int i = 0;
            foreach (var item in _metaIDScriptPathPairs)
            {
                _serializableMetaIDScriptPathPairs[i++] = new Pair(item.Key, item.Value);
            }
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_serializableMetaIDScriptPathPairs == null) { return; }
            foreach (var item in _serializableMetaIDScriptPathPairs)
            {
                if (string.IsNullOrEmpty(item.scriptPath) == false)
                {
                    _metaIDScriptPathPairs.Add(item.metaID, item.scriptPath);
                }
            }
        }
        #endregion

        #region Utils
        private bool CheckFileExists()
        {
            string filePath = GetFilePath();
            return File.Exists(filePath);
        }
        #endregion
    }
}
#endif