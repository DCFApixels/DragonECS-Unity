using UnityEditor;
using UnityEngine;

namespace DCFApixels
{
    public abstract class Config<TSelf> : ScriptableObject where TSelf : ScriptableObject
    {
        private static object _lock = new object();
        private static TSelf _instance;
        public static TSelf Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        string path = typeof(TSelf).ToString();
                        _instance = Resources.Load<TSelf>(typeof(TSelf).Name);
                        if (_instance == null)
                        {
                            TSelf data = CreateInstance<TSelf>();
#if UNITY_EDITOR
                            if (AssetDatabase.IsValidFolder("Assets/Resources/") == false)
                            {
                                System.IO.Directory.CreateDirectory(Application.dataPath + "/Resources/");
                                AssetDatabase.Refresh();
                            }
                            AssetDatabase.CreateAsset(data, "Assets/Resources/" + typeof(TSelf).Name + ".asset");
                            AssetDatabase.Refresh();
#endif
                            _instance = data;
                        }
                    }
                    return _instance;
                }
            }
        }
    }
}