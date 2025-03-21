using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Reflection;
using Unity.Profiling;
using UnityEditor;
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
            string msg = AutoConvertObjectToString(v);
            string indexedLink = UnityDebugServiceStorage.NewIndexedLink();
            bool hasTag = string.IsNullOrEmpty(tag) == false;
            if (hasTag)
            {
                string taglower = tag.ToLower();
                switch (taglower)
                {
                    case "pass":
                        Debug.Log(
                            $"[<color=#00ff00>{tag}</color>] {msg}{indexedLink}");
                        break;
                    case "warning":
                        Debug.LogWarning(
                            $"[<color=#ffff00>{tag}</color>] {msg}{indexedLink}");
                        break;
                    case "error":
                        Debug.LogError(
                            $"[<color=#ff4028>{tag}</color>] {msg}{indexedLink}");
                        break;
                    default:
                        Debug.Log(
                            $"[{tag}] {msg}{indexedLink}");
                        break;
                }
                return;
            }
            Debug.Log($"{msg}{indexedLink}");
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