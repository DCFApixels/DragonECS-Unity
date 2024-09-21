#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
    [Serializable]
    internal class UnityEditorCacheBlock
    {
        [SerializeField]
        private int _id;

        [SerializeField]
        private int _startIndex;

        [SerializeField]
        private string[] _jsons = new string[256];
        [SerializeField]
        private byte _jsonsCount;

        [SerializeField]
        private byte[] _recycledID = new byte[256];
        [SerializeField]
        private byte _recycledIDCount = 0;

        [SerializeField]
        private int _fullLength = 0;

        private UnityEditorCacheBlock() { }
        public UnityEditorCacheBlock(int id)
        {
            _id = id;
        }

        public int StartIndex
        {
            get { return _startIndex; }
        }
        public int Count
        {
            get { return _jsonsCount; }
        }
        public int FullLength
        {
            get { return _fullLength; }
        }

        public void Set(string json, ref int id)
        {
            const int MASK = ~(byte.MaxValue);
            byte slotID = (byte)id;
            if (slotID == 0)
            {
                if (_recycledIDCount > 0)
                {
                    id = _recycledID[_recycledIDCount--];
                }
                else
                {
                    id = _jsonsCount++;
                }
                id = (id & MASK) | slotID;
            }
            slotID--;

            string oldJson = _jsons[slotID];
            if (oldJson != null)
            {
                _fullLength -= oldJson.Length;
            }
            _fullLength += json.Length;
            _jsons[slotID] = json;
        }
        public string Get(int id)
        {
            byte slotID = (byte)id;
            _recycledID[_recycledIDCount++] = slotID;
            return _jsons[slotID];
        }

        public bool IsValide()
        {
            return
                _jsons != null &&
                _recycledID != null &&
                _jsons.Length == 256 &&
                _recycledID.Length == 256;
        }
        public void Reset()
        {
            _jsons = new string[256];
            _recycledID = new byte[256];
            _jsonsCount = 0;
            _recycledIDCount = 0;
        }
    }

    [FilePath(_CACHE_BLOCKS_FOLDER + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    public class UnityEditorCache : ScriptableSingleton<UnityEditorCache>, ISerializationCallbackReceiver
    {
        private const string _CACHE_BLOCKS_FOLDER = EcsUnityConsts.LOCAL_CACHE_FOLDER + "/" + nameof(UnityEditorCache);
        private const int _THRESHOLD_LENGTH = 2048;

        private static bool _isInit;
        private static string _projectPath;
        private static string _cachePath;

        private static void Init()
        {
            if (_isInit) { return; }
            _projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            _cachePath = _projectPath + _CACHE_BLOCKS_FOLDER;
            _isInit = true;
        }

        [SerializeField]
        private string[] _blockNames = new string[512];
        [SerializeField]
        private int _blockNamesCount = 0;

        [SerializeField]
        private int[] _recycledID = new int[512];
        [SerializeField]
        private int _recycledIDCount = 0;

        public void Set<T>(T data, ref int id)
        {
            Init();
            //try
            {
                int blockID;
                if (id == -1)
                {
                    if (_recycledIDCount > 0)
                    {
                        blockID = _recycledID[_recycledIDCount--];
                    }
                    else
                    {
                        if (_blockNamesCount >= _blockNames.Length)
                        {
                            Array.Resize(ref _blockNames, _blockNamesCount << 1);
                        }
                        blockID = _blockNamesCount++;
                    }
                    id = blockID << 8;
                }
                else
                {
                    blockID = (id >> 8) & 16_777_215;
                    if (blockID >= _blockNames.Length)
                    {
                        Array.Resize(ref _blockNames, blockID << 1);
                    }
                }

                ref string blockName = ref _blockNames[blockID];
                string filePath;
                if (string.IsNullOrEmpty(blockName))
                {
                    blockName = blockID.ToString();

                    filePath = Path.Combine(_cachePath, blockName);
                    string directoryName = _cachePath;
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }
                else
                {
                    filePath = Path.Combine(_cachePath, blockName);
                }

                var json = JsonUtility.ToJson(data);
                UnityEditorCacheBlock block;
                if (File.Exists(filePath))
                {
                    block = JsonUtility.FromJson<UnityEditorCacheBlock>(File.ReadAllText(filePath));
                }
                else
                {
                    block = new UnityEditorCacheBlock(blockID);
                }
                block.Set(json, ref id);
                File.WriteAllText(filePath, JsonUtility.ToJson(block));

                if (block.FullLength < _THRESHOLD_LENGTH)
                {
                    if (_recycledIDCount >= _recycledID.Length)
                    {
                        Array.Resize(ref _recycledID, _recycledIDCount == 0 ? 64 : _recycledIDCount << 1);
                    }
                    _recycledID[_recycledIDCount++] = blockID;
                }
                Save(true);
            }
            //catch (Exception e)
            //{
            //    Reset();
            //    throw e;
            //}
        }
        public T Get<T>(ref int id)
        {
            Init();
            //try
            {
                int blockID = (id >> 8) & 16_777_215;
                id = -1;

                ref string blockName = ref _blockNames[blockID];
                string filePath;
                if (string.IsNullOrEmpty(blockName))
                {
                    blockName = blockID.ToString();

                    filePath = Path.Combine(_cachePath, blockName);
                    string directoryName = _cachePath;
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }
                else
                {
                    filePath = Path.Combine(_cachePath, blockName);
                }

                UnityEditorCacheBlock block;
                if (File.Exists(filePath))
                {
                    block = JsonUtility.FromJson<UnityEditorCacheBlock>(File.ReadAllText(filePath));
                }
                else
                {
                    block = new UnityEditorCacheBlock(blockID);
                }

                string json = block.Get(id);



                if (block.FullLength < _THRESHOLD_LENGTH)
                {
                    if (_recycledIDCount >= _recycledID.Length)
                    {
                        Array.Resize(ref _recycledID, _recycledIDCount == 0 ? 64 : _recycledIDCount << 1);
                    }
                    _recycledID[_recycledIDCount++] = blockID;
                }


                Save(true);
                return json == null ? default : JsonUtility.FromJson<T>(json);
            }
            //catch (Exception e)
            //{
            //    Reset();
            //    throw e;
            //}
        }

        public void Save()
        {
            Save(true);
            EcsDebug.PrintPass("Save Save Save");
        }

        private void Reset()
        {
            _blockNames = new string[512];
            _blockNamesCount = 0;
            _recycledID = new int[512];
            _recycledIDCount = 0;
            Save(true);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }




        #region TODO
        //public int SetUnityObject(UnityObject obj)
        //{
        //    if (_jsonDatasCount >= _jsonDatas.Length)
        //    {
        //        Array.Resize(ref _jsonDatas, _jsonDatas.Length << 1);
        //    }
        //
        //    _jsonDatas[_jsonDatasCount] = JsonUtility.ToJson(obj);
        //    return _jsonDatasCount++;
        //}
        //public void GetOverwriteUnityObject(UnityObject obj, int id)
        //{
        //    JsonUtility.FromJsonOverwrite(_jsonDatas[id], obj);
        //}
        #endregion
    }
}
#endif