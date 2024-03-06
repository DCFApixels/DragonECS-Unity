using System;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [InitializeOnLoad]
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

            bool hasTag = string.IsNullOrEmpty(tag) == false;
            if (hasTag)
            {
                log = $".[{tag}] {v}";
                string taglower = tag.ToLower();
                if (taglower.Contains("warning"))
                {
                    Debug.LogWarning(log);
                    return;
                }
                if (taglower.Contains("error"))
                {
                    Debug.LogError(log);
                    return;
                }
                Debug.Log(log);
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
