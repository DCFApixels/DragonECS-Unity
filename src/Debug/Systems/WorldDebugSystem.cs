using DCFApixels.DragonECS.Unity.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    public class WorldDebugSystem : IEcsRunSystem 
    {
        private string _monitorName;
        private EcsWorld _ecsWorld;

        public WorldDebugSystem(EcsWorld ecsWorld, string monitorName = "World")
        {
            _monitorName = monitorName;
            _ecsWorld = ecsWorld;
            WorldDebugMonitor monitor = new GameObject(EcsConsts.DEBUG_PREFIX + _monitorName).AddComponent<WorldDebugMonitor>();
            WorldPoolsMonitor poolsmonitor = new GameObject(EcsConsts.DEBUG_PREFIX + _monitorName).AddComponent<WorldPoolsMonitor>();
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
            //   _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(800f));
            //   var pools = Target.world.GetAllPools().ToArray().Where(o => !(o is EcsNullPool)).OfType<IEcsPool>();
            //
            //   GUILayout.Label("", GUILayout.ExpandWidth(true));
            //   
            //   float width = GUILayoutUtility.GetLastRect().width;
            //
            //   Vector3 newPoolBlockSize = _poolBlockMinSize;
            //   int widthCount = Mathf.Max(1, Mathf.Min((Mathf.FloorToInt(width / _poolBlockMinSize.x)), pools.Count()));
            //   newPoolBlockSize.x = width / widthCount;
            //
            //   int x = -1, y = 0;
            //   foreach (var pool in pools)
            //   {
            //       if(++x >= widthCount)
            //       {
            //           x = 0;
            //           y++;
            //       }
            //
            //       DrawPoolBlock(pool, new Rect(newPoolBlockSize.x * x, newPoolBlockSize.y * y, newPoolBlockSize.x, newPoolBlockSize.y));
            //   }
            //   GUILayout.EndScrollView();
            }


          //  private void DrawPoolBlock(IEcsPool pool, Rect position)
          //  {
          //      Color defaultContentColor = GUI.contentColor;
          //      GUI.contentColor = Color.black * 0.925f;
          //
          //      position = AddMargin(position, 1f, 1f);
          //
          //      EditorGUI.DrawRect(position, Color.black* 0.16f);
          //
          //      Rect progressBar = new Rect(Vector2.zero, _poolProgressBasrSize);
          //      progressBar.width = position.width;
          //      progressBar.center = position.center - Vector2.up * _poolBlockMinSize.y * 0.09f;
          //
          //
          //      Color mainColor = new Color(0.3f, 1f, 0f, 1f);
          //      var debugColor = pool.ComponentType.GetCustomAttribute<DebugColorAttribute>();
          //      if (debugColor != null)
          //      {
          //          mainColor = debugColor.GetUnityColor();
          //      }
          //      Color backgroundColor = mainColor * 0.3f + Color.white * 0.2f;
          //
          //      EditorGUI.DrawRect(progressBar, backgroundColor);
          //
          //      progressBar.yMin = progressBar.yMax - ((float)pool.Count / pool.Capacity) * progressBar.height;
          //
          //      GUIStyle textStyle0 = EditorStyles.miniBoldLabel;
          //      textStyle0.alignment = TextAnchor.MiddleCenter;
          //
          //      Color foregroundColor = mainColor;
          //      EditorGUI.DrawRect(progressBar, foregroundColor);
          //      GUI.Label(progressBar, pool.Count.ToString(), textStyle0);
          //
          //      GUIStyle textStyle1 = EditorStyles.miniBoldLabel;
          //      textStyle1.alignment = TextAnchor.UpperCenter;
          //      GUI.Label(AddMargin(position, 3f, 3f), "Total\r\n"+ pool.Capacity, textStyle1);
          //
          //      GUI.contentColor = defaultContentColor;
          //      GUIStyle textStyle2 = EditorStyles.miniBoldLabel;
          //      textStyle2.alignment = TextAnchor.LowerCenter;
          //      GUI.Label(AddMargin(position, -10f, 3f), pool.ComponentType.Name, textStyle2);
          //
          //  }

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
