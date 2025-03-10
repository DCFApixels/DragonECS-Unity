#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Docs.Editors
{
    [FilePath(EcsUnityConsts.LOCAL_CACHE_FOLDER + "/" + nameof(DragonDocsPrefs) + ".prefs", FilePathAttribute.Location.ProjectFolder)]
    internal class DragonDocsPrefs : ScriptableSingleton<DragonDocsPrefs>
    {
        [SerializeField] private DragonDocs m_docs;
        [SerializeField] private bool[] m_isExpands;

        [NonSerialized] private bool _isInitInfos = false;
        [NonSerialized] private MetaGroupInfo[] _infos = null;

        public DragonDocs Docs
        {
            get { return m_docs; }
        }
        public Span<bool> IsExpands
        {
            get { return new Span<bool>(m_isExpands, 0, m_docs.Metas.Length); }
        }
        public ReadOnlySpan<MetaGroupInfo> Infos
        {
            get
            {
                InitInfos();
                return new ReadOnlySpan<MetaGroupInfo>(_infos);
            }
        }

        private void InitInfos()
        {
            if (_isInitInfos) { return; }
            ReadOnlySpan<DragonDocsMeta> metas;
            if (m_docs == null || (metas = m_docs.Metas).IsEmpty)
            {
                _infos = Array.Empty<MetaGroupInfo>();
                _isInitInfos = true;
                return;
            }

            string groupPath = metas[0]._group;
            int startIndex = 0;
            List<MetaGroupInfo> groups = new List<MetaGroupInfo>(128);
            for (int i = 1; i < metas.Length; i++)
            {
                var meta = metas[i];
                if (groupPath != meta._group)
                {
                    if (string.IsNullOrEmpty(groupPath))
                    {
                        groups.Add(new MetaGroupInfo("", "<OTHER>", startIndex, i - startIndex, 0));
                    }
                    else
                    {
                        AddInfo(groups, groupPath, startIndex, i - startIndex);
                    }
                    groupPath = meta._group;
                    startIndex = i;
                }
            }
            AddInfo(groups, groupPath, startIndex, metas.Length - startIndex);

            _infos = groups.ToArray();

            _isInitInfos = true;
        }

        private void AddInfo(List<MetaGroupInfo> infos, string path, int startIndex, int length)
        {
            MetaGroupInfo lastInfo;
            if (infos.Count == 0)
            {
                lastInfo = new MetaGroupInfo(string.Empty, string.Empty, 0, 0, 0);
            }
            else
            {
                lastInfo = infos[infos.Count - 1];
            }
            //if (lastInfo.Depth == 0) { lastInfo = new MetaGroupInfo("", "", 0, 0, 0); }
            int depth = 0;
            int lastSeparatorIndex = 0;
            int i = 0;
            int nameLength = 0;
            //if(lastInfo.Path.Length <= path.Length)
            {
                for (int j = 0, jMax = lastInfo.Path.Length; j < jMax; j++)
                {
                    char lastChr = lastInfo.Path[j];
                    char chr = path[j];
                    if (lastChr == chr)
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                    if (chr == '/') // || j == jMax - 1
                    {
                        depth++;
                        lastSeparatorIndex = j + 1;
                        nameLength = 0;
                    }
                    else
                    {
                        nameLength++;
                    }
                }
            }
            for (int iMax = path.Length; i < iMax; i++)
            {
                char chr = path[i];
                if (chr == '/')
                {
                    infos.Add(new MetaGroupInfo(path.Substring(0, lastSeparatorIndex + nameLength + 1), path.Substring(lastSeparatorIndex, nameLength), startIndex, i == iMax - 1 ? length : 0, depth));
                    depth++;
                    lastSeparatorIndex = i + 1;
                    nameLength = 0;
                }
                else
                {
                    nameLength++;
                }
            }
        }

        public void Save(DragonDocs docs)
        {
            m_docs = docs;
            if (m_isExpands == null || m_isExpands.Length != docs.Metas.Length)
            {
                int size = docs.Metas.Length;
                Array.Resize(ref m_isExpands, size <= 0 ? 1 : size);
                m_isExpands[0] = true;
            }
            _isInitInfos = false;
            _infos = null;
            Save(true);
        }

    }

    internal struct MetaGroupInfo
    {
        public readonly string Path;
        public readonly string Name;
        public readonly int StartIndex;
        public readonly int Length;
        public readonly int Depth;
        public MetaGroupInfo(string path, string name, int startIndex, int length, int depth)
        {
            Path = path;
            Name = name;
            StartIndex = startIndex;
            Length = length;
            Depth = depth;
        }
    }
}
#endif