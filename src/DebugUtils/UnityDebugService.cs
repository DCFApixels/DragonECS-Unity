using System;
using Unity.Profiling;
using UnityEngine;

#region [InitializeOnLoad]
#if UNITY_EDITOR
namespace DCFApixels.DragonECS
{
    using UnityEditor;
    [InitializeOnLoad]
    public partial class UnityDebugService { }
}
#endif
#endregion

namespace DCFApixels.DragonECS
{
    // Методы юнитевского Debug и ProfilerMarker потоко безопасны
    public partial class UnityDebugService : DebugService
    {
        private ProfilerMarker[] _profilerMarkers = new ProfilerMarker[64];

        static UnityDebugService()
        {
            Activate();
        }
        public static void Activate()
        {
            if (Instance.GetType() == typeof(UnityDebugService)) { return; }
            Set<UnityDebugService>();
        }

        protected override DebugService CreateThreadInstance()
        {
            return new UnityDebugService();
        }
        public override void Print(string tag, object v)
        {
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
                        Debug.Log(
                            $"[<color=#00ff00>{tag}</color>] {msg}");
                        break;
                    case "warning":
                        Debug.LogWarning(
                            $"[<color=#ffff00>{tag}</color>] {msg}");
                        break;
                    case "error":
                        Debug.LogError(
                            $"[<color=#ff4028>{tag}</color>] {msg}");
                        break;
                    default:
                        Debug.Log(
                            $"[{tag}] {msg}");
                        break;
                }
                return;
            }
            Debug.Log(msg);
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