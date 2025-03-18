#if UNITY_EDITOR
using DCFApixels.DragonECS;
using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal static class Generator
    {
        private const string PATH = "Assets/Generated/" + EcsUnityConsts.UNITY_PACKAGE_NAME;


        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            CompilationPipeline.compilationStarted -= CompilationPipeline_compilationStarted;
            CompilationPipeline.compilationStarted += CompilationPipeline_compilationStarted;
            CompilationPipeline.compilationFinished -= CompilationPipeline_compilationFinished;
            CompilationPipeline.compilationFinished += CompilationPipeline_compilationFinished;
        }

        private static void CompilationPipeline_compilationStarted(object obj)
        {
            Debug.Log("compilationStarted");
        }
        private static void CompilationPipeline_compilationFinished(object obj)
        {
            Debug.Log("compilationFinished");
        }



        private static void OnCompilationFinished(object obj)
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                CleanupFail();
            }


            var componentMetas = UnityEditorUtility._serializableTypeWithMetaIDs.Where(o => o.IsComponent);
        }

        private static void CleanupFail()
        {
            var guids = FindGeneratedAssets();
        }
        private static string[] FindGeneratedAssets()
        {
            string[] guids = AssetDatabase.FindAssets($" t:MonoScript", new[] { PATH });
            return guids;
        }





        private static void Generate(IEnumerable<TypeMeta> types)
        {

        }
        


    }
}
#endif