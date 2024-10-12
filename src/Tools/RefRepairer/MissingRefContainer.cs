#if UNITY_EDITOR
using DCFApixels.DragonECS.Unity.Editors;
using DCFApixels.DragonECS.Unity.RefRepairer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.RefRepairer.Editors
{
    internal class MissingRefContainer
    {
        public CollectedAssetMissingRecord[] collectedMissingTypesBuffer = null;
        public int collectedMissingTypesBufferCount = 0;
        public readonly Dictionary<TypeData, MissingsResolvingData> MissingsResolvingDatas = new Dictionary<TypeData, MissingsResolvingData>();
        public bool IsEmpty
        {
            get { return collectedMissingTypesBufferCount == 0; }
        }

        #region Clear/RemoveResolved
        public void Clear()
        {
            for (int i = 0; i < collectedMissingTypesBufferCount; i++)
            {
                collectedMissingTypesBuffer[i] = default;
            }
            collectedMissingTypesBufferCount = 0;
            MissingsResolvingDatas.Clear();
        }
        public void RemoveResolved()
        {
            int offset = 0;
            int i = 0;
            int newLength = collectedMissingTypesBufferCount;
            bool isReturn = true;
            for (; i < newLength; i++)
            {
                ref var collectedMissingType = ref collectedMissingTypesBuffer[i];
                if (collectedMissingType.IsResolvedOrNull)
                {
                    if (collectedMissingType.ResolvingData != null)
                    {
                        MissingsResolvingDatas.Remove(collectedMissingType.ResolvingData.OldTypeData);
                    }
                    offset = 1;
                    newLength--;
                    isReturn = false;
                    break;
                }
            }
            if (isReturn) { return; }

            int nextI = i + offset;
            for (; nextI < newLength; nextI++)
            {
                ref var collectedMissingType = ref collectedMissingTypesBuffer[i];
                if (collectedMissingType.IsResolvedOrNull)
                {
                    if (collectedMissingType.ResolvingData != null)
                    {
                        MissingsResolvingDatas.Remove(collectedMissingType.ResolvingData.OldTypeData);
                    }
                    offset++;
                    newLength--;
                }
                else
                {
                    collectedMissingTypesBuffer[i] = collectedMissingTypesBuffer[nextI];
                    i++;
                }
            }

            for (i = newLength; i < collectedMissingTypesBufferCount; i++)
            {
                collectedMissingTypesBuffer[i] = default;
            }

            collectedMissingTypesBufferCount = newLength;
        }
        #endregion

        #region Collect
        public void Collect()
        {
            int oldCollectedMissingTypesBufferCount = collectedMissingTypesBufferCount;
            if (collectedMissingTypesBuffer == null)
            {
                collectedMissingTypesBuffer = new CollectedAssetMissingRecord[256];
            }
            collectedMissingTypesBufferCount = 0;
            MissingsResolvingDatas.Clear();

            CollectByPrefabs();
            CollectByScriptableObjects();
            CollectByScenes();

            for (int i = collectedMissingTypesBufferCount; i < oldCollectedMissingTypesBufferCount; i++)
            {
                collectedMissingTypesBuffer[i] = default;
            }
        }
        private void Add(UnityObjectDataBase unityObjectData, ref ManagedReferenceMissingType missing)
        {
            var oldTypeData = new TypeData(missing);
            if (MissingsResolvingDatas.TryGetValue(oldTypeData, out var resolvingData) == false)
            {
                resolvingData = new MissingsResolvingData(oldTypeData);
                MissingsResolvingDatas.Add(oldTypeData, resolvingData);

                if (MetaIDRegistry.instance.TryGetMetaID(oldTypeData, out string metaID))
                {
                    if (UnityEditorUtility.TryGetTypeForMetaID(metaID, out Type type))
                    {
                        resolvingData.NewTypeData = new TypeData(type);
                    }
                }
            }

            if (collectedMissingTypesBufferCount >= collectedMissingTypesBuffer.Length)
            {
                Array.Resize(ref collectedMissingTypesBuffer, collectedMissingTypesBuffer.Length << 1);
            }
            collectedMissingTypesBuffer[collectedMissingTypesBufferCount++] = new CollectedAssetMissingRecord(unityObjectData, missing, resolvingData);
        }
        private void CollectByPrefabs()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            foreach (var pathToPrefab in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".prefab")))
            {
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);
                var unityObjectData = new UnityObjectData(prefabAsset, pathToPrefab);

                PrefabUtility.LoadPrefabContentsIntoPreviewScene(pathToPrefab, previewScene);
                var prefabLoaded = previewScene.GetRootGameObjects()[0];

                foreach (var component in prefabLoaded.GetComponentsInChildren<MonoBehaviour>())
                {
                    if (SerializationUtility.HasManagedReferencesWithMissingTypes(component) == false) { continue; }

                    var missings = SerializationUtility.GetManagedReferencesWithMissingTypes(component);
                    for (int i = 0; i < missings.Length; i++)
                    {
                        Add(unityObjectData, ref missings[i]);
                    }
                }

                UnityObject.DestroyImmediate(prefabLoaded);
            }
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
        private void CollectByScriptableObjects()
        {
            foreach (var pathToPrefab in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".asset")))
            {
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(pathToPrefab);
                var unityObjectData = new UnityObjectData(scriptableObject, pathToPrefab);

                if (SerializationUtility.HasManagedReferencesWithMissingTypes(scriptableObject) == false) { continue; }

                var missings = SerializationUtility.GetManagedReferencesWithMissingTypes(scriptableObject);
                for (int i = 0; i < missings.Length; i++)
                {
                    Add(unityObjectData, ref missings[i]);
                }
            }
        }
        private void CollectByScenes()
        {
            try
            {
                foreach (var scene in GetAllScenesInAssets())
                {
                    var unityObjectData = new SceneObjectData(scene);

                    var gameObjects = scene.GetRootGameObjects();

                    foreach (var objectOnScene in gameObjects)
                    {
                        foreach (var monoBehaviour in objectOnScene.GetComponentsInChildren<MonoBehaviour>())
                        {
                            if (SerializationUtility.HasManagedReferencesWithMissingTypes(monoBehaviour) == false) { continue; }

                            var missings = SerializationUtility.GetManagedReferencesWithMissingTypes(monoBehaviour);
                            for (int i = 0; i < missings.Length; i++)
                            {
                                Add(unityObjectData, ref missings[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion

        #region Utils
        private static IEnumerable<Scene> GetAllScenesInAssets()
        {
            var oldScenesSetup = EditorSceneManager.GetSceneManagerSetup();

            (bool isHasSelected, string scenePath, int identifierInFile) oldSelectedObject = default;
            GameObject activeGameObject = Selection.activeGameObject;
            if (activeGameObject != null)
            {
                oldSelectedObject.isHasSelected = true;
                oldSelectedObject.scenePath = activeGameObject.scene.path;
                oldSelectedObject.identifierInFile = activeGameObject.GetLocalIdentifierInFile();
            }

            foreach (var pathToScene in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".unity")))
            {
                Scene scene = EditorSceneManager.OpenScene(pathToScene, OpenSceneMode.Single);
                yield return scene;
            }

            EditorSceneManager.RestoreSceneManagerSetup(oldScenesSetup);

            if (oldSelectedObject.isHasSelected)
            {
                Selection.activeGameObject = SceneManager.GetSceneByPath(oldSelectedObject.scenePath)
                    .GetRootGameObjects()
                    .FirstOrDefault(gameObject => gameObject.GetLocalIdentifierInFile() == oldSelectedObject.identifierInFile);
            }
            else
            {
                Selection.activeGameObject = null;
            }
        }

        //private void AddMissingType(ManagedReferenceMissingType missingType, UnityObjectDataBase unityObject)
        //{
        //    var typeData = new TypeData(missingType);
        //    var missingTypeData = new MissingTypeData(missingType, unityObject);
        //    if (_manualRepairedMissingRefs.TryGetValue(typeData, out var containerMissingTypes) == false)
        //    {
        //        containerMissingTypes = new ContainerMissingRefs(typeData);
        //        _manualRepairedMissingRefs.Add(typeData, containerMissingTypes);
        //    }
        //
        //    containerMissingTypes.ManagedReferencesMissingTypeDatas.Add(missingTypeData);
        //}
        #endregion

    }

    internal static class UnityObjectExtensions
    {
        public static int GetLocalIdentifierInFile(this UnityObject unityObject)
        {
            PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            SerializedObject serializedObject = new SerializedObject(unityObject);
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
            return localIdProp.intValue;
        }
    }


    //internal class ContainerMissingTypes
    //{
    //    public readonly GUID AssetGUID;
    //    public readonly List<ContainerMissingRefs> Recirds = new List<ContainerMissingRefs>();
    //    public ContainerMissingTypes(GUID assetGUID)
    //    {
    //        AssetGUID = assetGUID;
    //    }
    //}
}
#endif