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

            for (int i = 0; i < container.collectedMissingTypesBufferCount; i++)
            {
                ref var missing = ref container.collectedMissingTypesBuffer[i];
                if (missing.IsNull) { continue; }

                var unityObjectData = missing.UnityObject;
                using (var file = new FileScope(unityObjectData.GetLocalAssetPath()))
                {
                    // тут итерируюсь по блоку missingsResolvingDatas с одинаковым юнити объектом, так как такие идеут подр€т
                    do
                    {
                        int lineIndex = NextRefLine(file.lines, 0);
                        while (lineIndex > 0)
                        {
                            var line = file.lines[lineIndex];

                            //  ак сказанно в документации к методу Replace
                            // A string that is equivalent to this instance except that all instances of oldChar are replaced with newChar.
                            // If oldChar is not found in the current instance, the method returns the current instance unchanged.
                            // ј конкретно строчки "returns the current instance unchanged", можно сделать упрощенную проверку через ReferenceEquals 
                            line = line.Replace(missing.ResolvingData.OldSerializedInfoLine, missing.ResolvingData.NewSerializedInfoLine);
                            bool isChanged = !ReferenceEquals(file.lines[lineIndex], line);


                            if (isChanged)
                            {
                                file.lines[lineIndex] = line;
                                break;
                            }
                            lineIndex = NextRefLine(file.lines, lineIndex);
                        }

                        missing = ref container.collectedMissingTypesBuffer[i++];
                    } while (unityObjectData == missing.UnityObject);
                    i--;//чтобы итераци€ не поломалась
                }
            }
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
                File.WriteAllLines(FilePath, lines);
                AssetDatabase.ImportAsset(LocalAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif