#if UNITY_EDITOR
using DCFApixels.DragonECS.UncheckedCore;
using DCFApixels.DragonECS.Unity.Internal;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    [CustomEditor(typeof(WorldMonitor))]
    internal class WorldMonitorEditor : ExtendedEditor<WorldMonitor>
    {
        private GUIStyle _headerStyle;

        private void CopyToClipboard()
        {
            const char SEPARATOR = '\t';

            var world = Target.World;
            EntitiesMatrix mtrx = new EntitiesMatrix(world);

            var allpools = world.AllPools.Slice(0, world.PoolsCount);
            StringBuilder sb = new StringBuilder();
            int i = -1;

            int entitiesCount = world.Entities.Count;

            //numbers
            sb.Append($"{SEPARATOR}{SEPARATOR}№");
            i = -1;
            foreach (var pool in allpools)
            {
                i++;
                sb.Append($"{SEPARATOR}{i}");
            }
            sb.Append("\r\n");
            //numbers end

            //chunks
            sb.Append($"{SEPARATOR}{SEPARATOR}Chunks");
            i = -1;
            foreach (var pool in allpools)
            {
                i++;
                sb.Append($"{SEPARATOR}{i >> 5}");
            }
            sb.Append("\r\n");
            //chunks end


            //header
            sb.Append($"Entity{SEPARATOR}Gen{SEPARATOR}Count");

            //pools
            foreach (var pool in allpools)
            {
                sb.Append($"{SEPARATOR}");
                if (pool.IsNullOrDummy() == false)
                {
                    sb.Append(pool.ComponentType.ToMeta().TypeName);
                }
                else
                {
                    sb.Append("NULL");
                }
            }
            sb.Append("\r\n");
            //header end


            //content
            for (i = 0; i < mtrx.EntitesCount; i++)
            {
                if (mtrx.IsEntityUsed(i))
                {
                    sb.Append($"{i}{SEPARATOR}{mtrx.GetEntityGen(i)}{SEPARATOR}{mtrx.GetEntityComponentsCount(i)}");
                    for (int j = 0; j < mtrx.PoolsCount; j++)
                    {
                        if (mtrx[i, j])
                        {
                            sb.Append($"{SEPARATOR}+");
                        }
                        else
                        {
                            sb.Append($"{SEPARATOR}");
                        }
                    }
                    sb.Append("\r\n");
                    entitiesCount--;
                }

                if (entitiesCount <= 0)
                {
                    break;
                }
            }
            //end

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        protected override void DrawCustom()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.fontSize = 28;
            }

            using (EcsGUI.Layout.BeginHorizontal())
            {
                GUILayout.Label("[World]", _headerStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Copy to Clipboard", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true)))
                {
                    CopyToClipboard();
                }
            }


            EcsGUI.Layout.DrawWorldBaseInfo(Target.World);

            EcsGUI.Layout.DrawWorldComponents(Target.World);
        }
    }
}
#endif