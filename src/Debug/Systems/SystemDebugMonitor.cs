using DCFApixels.DragonECS.Unity.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity
{
    public class SystemDebugMonitor :MonoBehaviour
    {
        [SerializeReference]
        private IEcsSystem _target; //TODO переделать подручнуюотрисовку, потому как [SerializeReference] не работает с generic-ами

        public static SystemDebugMonitor CreateMonitor(Transform parent, IEcsSystem system, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            go.SetActive(false);

            SystemDebugMonitor result = go.AddComponent<SystemDebugMonitor>();

            result._target = system;

            return result;
        }
    }
}
