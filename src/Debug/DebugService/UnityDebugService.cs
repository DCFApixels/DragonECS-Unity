using System;
using Unity.Profiling;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class UnityDebugService : DebugService
    {
        public static void Init() => Set<UnityDebugService>();

        private ProfilerMarker[] _profilerMarkers = new ProfilerMarker[64];

        public override void Print(string tag, object v)
        {
            string log;
            if (!string.IsNullOrEmpty(tag))
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

        public override void ProfilerMarkBegin(int id)
        {
            _profilerMarkers[id].Begin();
        }

        public override void ProfilerMarkEnd(int id)
        {
            _profilerMarkers[id].End();
        }

        protected override void OnDelProfilerMark(int id)
        {
            _profilerMarkers[id] = default;
        }

        protected override void OnNewProfilerMark(int id, string name)
        {
            if (id >= _profilerMarkers.Length) Array.Resize(ref _profilerMarkers, _profilerMarkers.Length << 1);
            _profilerMarkers[id] = new ProfilerMarker(ProfilerCategory.Scripts, name);
        }

    }
}
