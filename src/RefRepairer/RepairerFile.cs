using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public static class FileRepaireUtility
    {
        private const string REFLINE_PATTERN = "- rid:";
        public static void Replace(string filePath)
        {

        }
    }
    public class RepairerFile
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
