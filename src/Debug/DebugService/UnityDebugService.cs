using System;
using Unity.Profiling;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity
{
    public class UnityDebugService : DebugService
    {
        public static void Init() => Set<UnityDebugService>();

        private ProfilerMarker[] _profilerMarkers = new ProfilerMarker[64];

        public override void Print(string tag, object v)
        {
            string log = $"[{tag}] {v}";
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
        }

        public override void ProfileMarkBegin(int id)
        {
            _profilerMarkers[id].Begin();
        }

        public override double ProfileMarkEnd(int id)
        {
            _profilerMarkers[id].End();
            return -1;
        }

        protected override void OnDelMark(int id)
        {
            _profilerMarkers[id] = default;
        }

        protected override void OnNewMark(int id, string name)
        {
            if (id >= _profilerMarkers.Length) Array.Resize(ref _profilerMarkers, _profilerMarkers.Length << 1);
            _profilerMarkers[id] = new ProfilerMarker(ProfilerCategory.Scripts, name);
        }

    }
}
