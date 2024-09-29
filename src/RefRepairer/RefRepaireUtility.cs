using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public static class UnityObjectExtensions
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

    internal class RefRepaireUtility
    {
        private readonly Dictionary<TypeData, ContainerMissingTypes> _missingTypes = new Dictionary<TypeData, ContainerMissingTypes>();


        #region Collect
        public List<ContainerMissingTypes> Collect()
        {
            _missingTypes.Clear();

            CollectByPrefabs();
            CollectByScriptableObjects();
            CollectByScenes();

            List<ContainerMissingTypes> result = new List<ContainerMissingTypes>(_missingTypes.Select((typeAndContainer) => typeAndContainer.Value));
            _missingTypes.Clear();
            return result;
        }
        private void CollectByPrefabs()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();

            foreach (var pathToPrefab in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".prefab")))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);
                PrefabUtility.LoadPrefabContentsIntoPreviewScene(pathToPrefab, previewScene);
                var copyPrefab = previewScene.GetRootGameObjects()[0];

                var componentsPrefab = prefab.GetComponentsInChildren<MonoBehaviour>();
                var componentsCopyPrefab = copyPrefab.GetComponentsInChildren<MonoBehaviour>();

                for (int i = 0; i < componentsCopyPrefab.Length; ++i)
                {
                    var monoBehaviour = componentsCopyPrefab[i];
                    if (SerializationUtility.HasManagedReferencesWithMissingTypes(monoBehaviour) == false)
                    {
                        continue;
                    }

                    foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(monoBehaviour))
                    {
                        var prefabObject = new UnityObjectData(componentsPrefab[i].gameObject, pathToPrefab);

                        AddMissingType(missingType, prefabObject);
                    }
                }

                UnityObject.DestroyImmediate(copyPrefab);
            }

            EditorSceneManager.ClosePreviewScene(previewScene);
        }
        private void CollectByScriptableObjects()
        {
            foreach (var pathToPrefab in AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && path.EndsWith(".asset")))
            {
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(pathToPrefab);

                if (SerializationUtility.HasManagedReferencesWithMissingTypes(scriptableObject) == false)
                {
                    continue;
                }

                foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(scriptableObject))
                {
                    var prefabObject = new UnityObjectData(scriptableObject, pathToPrefab);
                    AddMissingType(missingType, prefabObject);
                }
            }
        }
        private void CollectByScenes()
        {
            try
            {
                foreach (var scene in GetAllScenesInAssets())
                {
                    var gameObjects = scene.GetRootGameObjects();

                    foreach (var objectOnScene in gameObjects)
                    {
                        foreach (var monoBehaviour in objectOnScene.GetComponentsInChildren<MonoBehaviour>())
                        {
                            if (SerializationUtility.HasManagedReferencesWithMissingTypes(monoBehaviour) == false)
                            {
                                continue;
                            }

                            foreach (var missingType in SerializationUtility.GetManagedReferencesWithMissingTypes(monoBehaviour))
                            {
                                var sceneObject = new SceneObjectData(scene);
                                AddMissingType(missingType, sceneObject);
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {

                Debug.LogException(e);
            }



            // SerializationUtility.
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

        private void AddMissingType(ManagedReferenceMissingType missingType, BaseUnityObjectData unityObject)
        {
            var typeData = new TypeData(missingType);
            var missingTypeData = new MissingTypeData(missingType, unityObject);
            if (_missingTypes.TryGetValue(typeData, out var containerMissingTypes) == false)
            {
                containerMissingTypes = new ContainerMissingTypes(typeData);
                _missingTypes.Add(typeData, containerMissingTypes);
            }

            containerMissingTypes.Add(missingTypeData);
        }
        #endregion

    }





    public class ContainerMissingTypes
    {
        public readonly TypeData TypeData;

        private readonly List<MissingTypeData> _managedReferencesMissingTypeDatas = new List<MissingTypeData>();

        public IReadOnlyCollection<MissingTypeData> ManagedReferencesMissingTypeDatas => _managedReferencesMissingTypeDatas;

        public ContainerMissingTypes(TypeData typeData)
        {
            TypeData = typeData;
        }

        public void Add(MissingTypeData missingTypeData) => _managedReferencesMissingTypeDatas.Add(missingTypeData);

        public void Remove(MissingTypeData missingTypeData) => _managedReferencesMissingTypeDatas.Remove(missingTypeData);

        public void RemoveAt(int index) => _managedReferencesMissingTypeDatas.RemoveAt(index);
    }
    public struct MissingTypeData
    {
        public readonly ManagedReferenceMissingType Data;
        public readonly BaseUnityObjectData UnityObject;

        public MissingTypeData(ManagedReferenceMissingType missingType, BaseUnityObjectData unityObject)
        {
            Data = missingType;
            UnityObject = unityObject;
        }
    }
    public struct TypeData
    {
        public readonly string AssemblyName;
        public readonly string NamespaceName;
        public readonly string ClassName;

        public TypeData(ManagedReferenceMissingType missingType)
        {
            AssemblyName = missingType.assemblyName;
            NamespaceName = missingType.namespaceName;
            ClassName = missingType.className;
        }
    }
    public abstract class BaseUnityObjectData
    {
        public string LocalAssetPath => AssetDatabase.GUIDToAssetPath(AssetGuid);
        public abstract GUID AssetGuid { get; }
    }
    public class SceneObjectData : BaseUnityObjectData
    {
        public readonly string SceneName;

        public override GUID AssetGuid { get; }

        public SceneObjectData(Scene scene)
        {
            SceneName = scene.name;
            AssetGuid = AssetDatabase.GUIDFromAssetPath(scene.path);
        }
    }
    public class UnityObjectData : BaseUnityObjectData
    {
        public readonly UnityObject UnityObject;
        public override GUID AssetGuid { get; }

        public UnityObjectData(UnityObject unityObject, string pathToPrefab)
        {
            UnityObject = unityObject;
            AssetGuid = AssetDatabase.GUIDFromAssetPath(pathToPrefab);
        }
    }
}