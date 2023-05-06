using DCFApixels.DragonECS.Unity.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [DebugHide, DebugColor(DebugColor.Gray)]
    public class WorldDebugSystem : IEcsRunProcess 
    {
        private string _monitorName;
        private EcsWorld _ecsWorld;

        public WorldDebugSystem(EcsWorld ecsWorld, string monitorName = null)
        {
            _monitorName = monitorName;
            if (string.IsNullOrEmpty(_monitorName)) _monitorName = ecsWorld.GetType().Name;
            _ecsWorld = ecsWorld;
            WorldDebugMonitor monitor = new GameObject(EcsConsts.DEBUG_PREFIX + _monitorName).AddComponent<WorldDebugMonitor>();
            WorldPoolsMonitor poolsmonitor = new GameObject(EcsConsts.DEBUG_PREFIX + "Pools").AddComponent<WorldPoolsMonitor>();
            poolsmonitor.transform.SetParent(monitor.transform);

            monitor.source = this;
            monitor.world = _ecsWorld;
            monitor.monitorName = _monitorName;

            poolsmonitor.source = this;
            poolsmonitor.world = _ecsWorld;
            poolsmonitor.monitorName = "pools";
        }

        public void Run(EcsPipeline pipeline)
        {
        }
    }

    public class WorldDebugMonitor : DebugMonitorBase
    {
        internal WorldDebugSystem source;
        internal EcsWorld world;
    }

#if UNITY_EDITOR
    namespace Editors
    {
        using UnityEditor;

        [CustomEditor(typeof(WorldDebugMonitor))]
        public class WorldDebugMonitorEditor : Editor
        {
            private WorldDebugMonitor Target => (WorldDebugMonitor)target;

            public override void OnInspectorGUI()
            {
                GUILayout.Label($"Size: {Target.world.Capacity}");
                GUILayout.Label($"Total entities: {Target.world.Count}");
            }
        }
    }
#endif




    public class WorldPoolsMonitor : DebugMonitorBase
    {
        internal WorldDebugSystem source;
        internal EcsWorld world;
    }

#if UNITY_EDITOR
    namespace Editors
    {
        using System.Linq;
        using UnityEditor;
        using System.Reflection;

        [CustomEditor(typeof(WorldPoolsMonitor))]
        public class WorldPoolsMonitorEditor : Editor
        {
            private static Vector2 _poolBlockMinSize = new Vector2(80, 160);
            private static Vector2 _poolProgressBasrSize = _poolBlockMinSize * new Vector2(1f, 0.8f);

            private WorldPoolsMonitor Target => (WorldPoolsMonitor)target;

            private Vector2 _scroll;

            public override void OnInspectorGUI()
            {
               _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(800f));
               var pools = Target.world.AllPools.ToArray().Where(o => !o.IsNullOrDummy()).OfType<IEcsPool>();
            
               GUILayout.Label("", GUILayout.ExpandWidth(true));
               
               float width = GUILayoutUtility.GetLastRect().width;
            
               Vector3 newPoolBlockSize = _poolBlockMinSize;
               int widthCount = Mathf.Max(1, Mathf.Min((Mathf.FloorToInt(width / _poolBlockMinSize.x)), pools.Count()));
               newPoolBlockSize.x = width / widthCount;
            
               int x = -1, y = 0;
               foreach (var pool in pools)
               {
                   if(++x >= widthCount)
                   {
                       x = 0;
                       y++;
                   }
            
                   DrawPoolBlock(pool, new Rect(newPoolBlockSize.x * x, newPoolBlockSize.y * y, newPoolBlockSize.x, newPoolBlockSize.y));
               }
               GUILayout.EndScrollView();
            }


            private void DrawPoolBlock(IEcsPool pool, Rect position)
            {
                int count = pool.Count;
                int capacity = pool.Capacity < 0 ? count : pool.Capacity;

                Color defaultContentColor = GUI.contentColor;
                GUI.contentColor = Color.black * 0.925f;
          
                position = AddMargin(position, 1f, 1f);
          
                EditorGUI.DrawRect(position, Color.black* 0.16f);
          
                Rect progressBar = new Rect(Vector2.zero, _poolProgressBasrSize);
                progressBar.width = position.width;
                progressBar.center = position.center - Vector2.up * _poolBlockMinSize.y * 0.09f;
          
          
                Color mainColor = new Color(0.3f, 1f, 0f, 1f);
                var debugColor = pool.ComponentType.GetCustomAttribute<DebugColorAttribute>();
                if (debugColor != null)
                {
                    mainColor = debugColor.GetUnityColor();
                }
                Color backgroundColor = mainColor * 0.3f + Color.white * 0.2f;
          
                EditorGUI.DrawRect(progressBar, backgroundColor);
          
                progressBar.yMin = progressBar.yMax - ((float)count / capacity) * progressBar.height;
          
                GUIStyle textStyle0 = new GUIStyle(EditorStyles.miniBoldLabel);
                textStyle0.alignment = TextAnchor.MiddleCenter;
          
                Color foregroundColor = mainColor;
                EditorGUI.DrawRect(progressBar, foregroundColor);
                GUI.Label(progressBar, count.ToString(), textStyle0);
          
                GUIStyle textStyle1 = new GUIStyle(EditorStyles.miniBoldLabel);
                textStyle1.alignment = TextAnchor.UpperCenter;
                GUI.Label(AddMargin(position, 3f, 3f), "Total\r\n"+ capacity, textStyle1);
          
                GUI.contentColor = defaultContentColor;
                GUIStyle textStyle2 = new GUIStyle(EditorStyles.miniBoldLabel);
                textStyle2.wordWrap = true;
                textStyle2.alignment = TextAnchor.LowerCenter;
                string name = EcsEditor.GetGenericName(pool.ComponentType);
                GUIContent label = new GUIContent(name, $"t({name})");
                GUI.Label(AddMargin(position, -10f, 3f), label, textStyle2);
          
            }

            private Rect AddMargin(Rect rect, Vector2 value)
            {
                return AddMargin(rect, value.x, value.y);
            }
            private Rect AddMargin(Rect rect, float x, float y)
            {
                rect.yMax -= y;
                rect.yMin += y;
                rect.xMax -= x;
                rect.xMin += x;
                return rect;
            }
        }
    }
#endif
}
