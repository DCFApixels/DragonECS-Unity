using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal static class RepaireFileUtility
    {
        private const string REFLINE_PATTERN = "- rid:";
        public static void Replace(string[] fileLines, string oldTypeData, string newTypeData)
        {
            for (int i = 0; i < fileLines.Length; i++)
            {
                if (fileLines[i].Contains(REFLINE_PATTERN))
                {
                    fileLines[++i].Replace(oldTypeData, newTypeData);
                }
            }
        }
        public static int NextRefLine(string[] fileLines, int startIndex)
        {
            for (int i = startIndex; i < fileLines.Length; i++)
            {
                if (fileLines[i].Contains(REFLINE_PATTERN))
                {
                    return ++i;
                }
            }
            return -1;
        }
        public static string GenerateReplacedLine(TypeData typeData)
        {
            return $"type: {{class: {typeData.ClassName}, ns: {typeData.NamespaceName}, asm: {typeData.AssemblyName}}}";
        }
        public static string GenerateReplacedLine(ManagedReferenceMissingType typeData)
        {
            return $"type: {{class: {typeData.className}, ns: {typeData.namespaceName}, asm: {typeData.assemblyName}}}";
        }

        public struct FileScope : IDisposable
        {
            public readonly string FilePath;
            public readonly string LocalAssetPath;
            public string[] lines;
            public FileScope(string localAssetPath)
            {
                LocalAssetPath = localAssetPath;
                FilePath = $"{Application.dataPath.Replace("/Assets", "")}/{localAssetPath}";
                lines = File.ReadAllLines(localAssetPath);
            }

            public void Dispose()
            {
                File.WriteAllLines(FilePath, lines);
                AssetDatabase.ImportAsset(LocalAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }
    }


    internal class RepairerFile
    {
        private readonly Type _type;
        private readonly string[] _fileLines;

        private string _currentLine;
        private string _nextLine;

        private int _currentIndex;

        private readonly string _path;
        private readonly string _localAssetPath;

        public RepairerFile(Type type, string localAssetPath)
        {
            _type = type;
            _localAssetPath = localAssetPath;

            _path = $"{Application.dataPath.Replace("/Assets", "")}/{localAssetPath}";
            _fileLines = File.ReadAllLines(_path);
        }

        public delegate bool GetterMissingTypeData(out MissingTypeData missingType);

        public void Repair(Func<bool> callbackForNextLine)
        {
            for (int i = 0; i < _fileLines.Length - 1; ++i)
            {
                _currentIndex = i;

                _currentLine = _fileLines[i];
                _nextLine = _fileLines[i + 1];

                if (callbackForNextLine.Invoke())
                    break;
            }

            File.WriteAllLines(_path, _fileLines);

            AssetDatabase.ImportAsset(_localAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        public bool CheckNeedLineAndReplacedIt(ManagedReferenceMissingType missingType)
        {
            string rid = $"rid: {missingType.referenceId}";
            string oldTypeData = $"type: {{class: {missingType.className}, ns: {missingType.namespaceName}, asm: {missingType.assemblyName}}}";

            if (_currentLine.Contains(rid) && _nextLine.Contains(oldTypeData))
            {
                string newTypeData = $"type: {{class: {_type.Name}, ns: {_type.Namespace}, asm: {_type.Assembly.GetName().Name}}}";
                _fileLines[_currentIndex + 1] = _nextLine.Replace(oldTypeData, newTypeData);

                return true;
            }

            return false;
        }
    }
}
