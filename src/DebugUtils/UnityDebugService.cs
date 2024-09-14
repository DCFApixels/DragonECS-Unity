using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class UnityDebugService : DebugService
    {
        private ProfilerMarker[] _profilerMarkers = new ProfilerMarker[64];

        static UnityDebugService()
        {
            Activate();
        }
        public static void Activate()
        {
            Set<UnityDebugService>();
        }
        public override void Print(string tag, object v)
        {
            string log;
            if (v is Exception e)
            {
                Debug.LogException(e);
                return;
            }

            string msg = AutoConvertObjectToString(v);
            bool hasTag = string.IsNullOrEmpty(tag) == false;
            if (hasTag)
            {
                string taglower = tag.ToLower();
                switch (taglower)
                {
                    case "pass":
                        log = $"[<color=#00ff00>{tag}</color>] {msg}";
                        Debug.Log(log);
                        break;
                    case "warning":
                        log = $"[<color=#ffff00>{tag}</color>] {msg}";
                        Debug.LogWarning(log);
                        break;
                    case "error":
                        log = $"[<color=#ff4028>{tag}</color>] {msg}";
                        Debug.LogError(log);
                        break;
                    default:
                        log = $"[{tag}] {msg}";
                        Debug.Log(log);
                        break;
                }
                return;
            }
            Debug.Log(v);
        }
        public override void Break()
        {
            Debug.Break();
        }
        public sealed override void ProfilerMarkBegin(int id)
        {
            _profilerMarkers[id].Begin();
        }
        public sealed override void ProfilerMarkEnd(int id)
        {
            _profilerMarkers[id].End();
        }
        protected sealed override void OnDelProfilerMark(int id)
        {
            _profilerMarkers[id] = default;
        }
        protected sealed override void OnNewProfilerMark(int id, string name)
        {
            if (id >= _profilerMarkers.Length)
            {
                Array.Resize(ref _profilerMarkers, _profilerMarkers.Length << 1);
            }
            _profilerMarkers[id] = new ProfilerMarker(ProfilerCategory.Scripts, name);
        }
    }
}
