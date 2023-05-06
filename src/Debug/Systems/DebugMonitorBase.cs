using UnityEngine;


namespace DCFApixels.DragonECS.Unity.Debug
{
    public class DebugMonitorBase : MonoBehaviour
    {
        internal string monitorName;


        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            gameObject.SetActive(false);
        }
    }
}
