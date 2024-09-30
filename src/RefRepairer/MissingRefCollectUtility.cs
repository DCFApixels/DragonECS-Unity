#if UNITY_EDITOR
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

    internal class ContainerMissingTypes
    {
        public readonly GUID AssetGUID;
        public readonly List<ContainerMissingRefs> Recirds = new List<ContainerMissingRefs>();
        public ContainerMissingTypes(GUID assetGUID)
        {
            AssetGUID = assetGUID;
        }
    }

    internal class MissingRefCollectUtility
    {
        //private readonly Dictionary<TypeData, ContainerMissingRefs> _manualRepairedMissingRefs = new Dictionary<TypeData, ContainerMissingRefs>();
        //private readonly List<ContainerMissingTypes> _autoRepariedMissingTypes = new List<ContainerMissingTypes>();

        private readonly List<Record> _collectedMissingTypes = new List<Record>();

        #region Collect
        public List<Record> Collect()
        {
            //_manualRepairedMissingRefs.Clear();
            
            _collectedMissingTypes.Clear();

            CollectByPrefabs(_collectedMissingTypes);
            CollectByScriptableObjects(_collectedMissingTypes);
            CollectByScenes(_collectedMissingTypes);

            //ContainerMissingRefs[] result = _manualRepairedMissingRefs.Values.ToArray();
            //_manualRepairedMissingRefs.Clear();

            return _collectedMissingTypes;
        }
        public readonly struct Record
        {
            public readonly UnityObjectDataBase UnityObject;
            public readonly ManagedReferenceMissingType[] missingTypes;
            public Record(UnityObjectDataBase unityObject, ManagedReferenceMissingType[] missingTypes)
            {
                UnityObject = unityObject;
                this.missingTypes = missingTypes;
            }
        }
        private void CollectByPrefabs(List<Record> list)
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
                    list.Add(new Record(unityObjectData, missings));
                }

                UnityObject.DestroyImmediate(prefabLoaded);
            }
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
        private void CollectByScriptableObjects(List<Record> list)
        {
            foreach (var pathToPrefab in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".asset")))
            {
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(pathToPrefab);
                var unityObjectData = new UnityObjectData(scriptableObject, pathToPrefab);

                if (SerializationUtility.HasManagedReferencesWithMissingTypes(scriptableObject) == false) { continue; }

                var missings = SerializationUtility.GetManagedReferencesWithMissingTypes(scriptableObject);
                list.Add(new Record(unityObjectData, missings));
            }
        }
        private void CollectByScenes(List<Record> list)
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
                            list.Add(new Record(unityObjectData, missings));
                        }
                    }
                }
            }
            catch (System.Exception e)
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
}
#endif