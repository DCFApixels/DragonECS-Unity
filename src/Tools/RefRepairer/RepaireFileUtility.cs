#if UNITY_EDITOR
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
        public static void RepaieAsset(MissingRefContainer container)
        {
            if (container.IsEmpty) { return; }

            UnityObjectDataBase unityObjectData = null;
            FileScope fileScope = default;
            for (int i = 0; i < container.collectedMissingTypesBufferCount; i++)
            {
                ref var missing = ref container.collectedMissingTypesBuffer[i];
                if (unityObjectData != missing.UnityObject)
                {
                    unityObjectData = missing.UnityObject;
                    fileScope.Dispose();
                    fileScope = new FileScope(unityObjectData.GetLocalAssetPath());
                }

                int lineIndex = NextRefLine(fileScope.lines, 0);
                while (lineIndex > 0)
                {
                    var line = fileScope.lines[lineIndex];

                    // ��� �������� � ������������ � ������ Replace
                    // A string that is equivalent to this instance except that all instances of oldChar are replaced with newChar.
                    // If oldChar is not found in the current instance, the method returns the current instance unchanged.
                    // � ��������� ������� "returns the current instance unchanged", ����� ������� ���������� �������� ����� ReferenceEquals 
                    line = line.Replace(missing.ResolvingData.OldSerializedInfoLine, missing.ResolvingData.NewSerializedInfoLine);
                    bool isChanged = !ReferenceEquals(fileScope.lines[lineIndex], line);

                    if (isChanged)
                    {
                        fileScope.lines[lineIndex] = line;
                        break;
                    }
                    lineIndex = NextRefLine(fileScope.lines, lineIndex);
                }
            }
            fileScope.Dispose();

            container.RemoveResolved();
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
                if (string.IsNullOrEmpty(FilePath)) { return; }
                File.WriteAllLines(FilePath, lines);
                AssetDatabase.ImportAsset(LocalAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif