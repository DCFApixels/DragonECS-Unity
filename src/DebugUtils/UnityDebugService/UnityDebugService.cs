using DCFApixels.DragonECS.Unity.Internal;
using System;
using Unity.Profiling;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    // Методы юнитевского Debug и ProfilerMarker потоко безопасны
    public partial class UnityDebugService : DebugService
    {
        private ProfilerMarker[] _profilerMarkers = new ProfilerMarker[64];

        public static void Activate()
        {
            if (Instance.GetType() == typeof(UnityDebugService)) { return; }
            Set<UnityDebugService>();
        }

        protected override DebugService CreateThreadInstance()
        {
            return new UnityDebugService();
        }
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public override void Print(string tag, object v)
        {
            if (v is Exception e)
            {
                Debug.LogException(e);
                return;
            }
            bool hasTag = string.IsNullOrEmpty(tag) == false;
            string msg = AutoConvertObjectToString(v);

#if DRAGONECS_ENABLE_UNITY_CONSOLE_SHORTCUT_LINKS
            string indexedLink = UnityDebugServiceStorage.NewIndexedLink();
            if (hasTag)
            {
                string taglower = tag.ToLower();
                switch (taglower)
                {
                    case "pass":
                        Debug.Log(
                            $"{indexedLink}[<color=#00ff00>{tag}</color>] {msg}");
                        break;
                    case "warning":
                        Debug.LogWarning(
                            $"{indexedLink}[<color=#ffff00>{tag}</color>] {msg}");
                        break;
                    case "error":
                        Debug.LogError(
                            $"{indexedLink}[<color=#ff4028>{tag}</color>] {msg}");
                        break;
                    default:
                        Debug.Log(
                            $"{indexedLink}[{tag}] {msg}");
                        break;
                }
                return;
            }
            Debug.Log($"{indexedLink}{msg}");
#else
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
            Debug.Log($"{msg}");
#endif
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