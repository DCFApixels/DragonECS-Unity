#if UNITY_EDITOR
using DCFApixels.DragonECS;
using DCFApixels.DragonECS.PoolsCore;
using DCFApixels.DragonECS.Unity;
using DCFApixels.DragonECS.Unity.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
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














public abstract class GeneratedComponentTemplateBase : ComponentTemplateBase
{
    [Serializable]
    protected struct TypeInfo
    {
        public string asm;
        public string ns;
        public string name;
        public TypeInfo(string asm, string ns, string name)
        {
            this.asm = asm;
            this.ns = ns;
            this.name = name;
        }
    }
}
public abstract class GeneratedComponentTemplateBase<TStencil> : GeneratedComponentTemplateBase
{
    private static Type _componentType;
    private static Type _componentInterfaceType;

    private static ConverterWrapperBase<TStencil> _converter;

    [SerializeField]
    private TStencil _component; // Stencil
    [SerializeField]
    private bool _offset;
    public override Type Type
    {
        get { return _componentType; }
    }
    public override void Apply(short worldID, int entityID)
    {
        _converter.Apply(ref _component, worldID, entityID);
    }
    public override object GetRaw()
    {
        return _converter.GetRaw(ref _component);
    }
    public override void SetRaw(object raw)
    {
        _converter.SetRaw(ref _component, raw);
    }

    public static void InitStatic(string componentTypeAssemblyQualifiedName, string poolTypeAssemblyQualifiedName)
    {

    }
}

public static class GeneratorUtility
{
    public delegate void ApplyHandler<TComponent>(ref TComponent data);
    public delegate void ApplyHandler<TComponent, TPool>(ref TComponent data, TPool pool);

    public static bool SkanTypeStructure(Type type)
    {

    }
    private static bool IsSerializableField(FieldInfo fieldInfo)
    {
        return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null || fieldInfo.GetCustomAttribute<SerializeReference>() != null;
    }
    private static bool IsCanUnsafeOverride(FieldInfo fieldInfo)
    {
        if (fieldInfo.FieldType.IsValueType)
        {
            return true;
        }
        if (fieldInfo.FieldType == typeof(string))
        {
            return true;
        }
        if (typeof(UnityEngine.Object).IsAssignableFrom(fieldInfo.FieldType))
        {
            return true;
        }
        if (fieldInfo.GetCustomAttribute<SerializeReference>() != null)
        {
            return true;
        }

        return false;
    }

    //public interface IUnityCompilatorInfo // defines hack
    //{
    //    /// <summary> VMT size </summary>
    //    public int ObjectVirtualDataSize { get; }
    //    /// <summary> can rewrite VMT </summary>
    //    public bool IsSupportRewriteObjectVirtualData { get; }
    //}
    //private class UnityCompilatorInfo : IUnityCompilatorInfo 
    //{
    //    public int ObjectVirtualDataSize
    //    {
    //        get
    //        {
    //            return 8;
    //        }
    //    }
    //    public bool IsSupportRewriteObjectVirtualData
    //    {
    //        get
    //        {
    //            return true;
    //        }
    //    }
    //}
}
public abstract class ConverterWrapperBase<TStencil>
{
    public abstract void Apply(ref TStencil component, short worldID, int entityID);
    public abstract object GetRaw(ref TStencil stencilComponent);
    public abstract void SetRaw(ref TStencil stencilComponent, object raw);
}
public class ConverterWrapper<TStencil, TComponent, TPool> : ConverterWrapperBase<TStencil> where TPool : IEcsPoolImplementation, new()
{
    private F.DoHandler<TComponent> _apply;
    private F.DoHandler<TComponent, TPool> _apply2;
    public override void Apply(ref TStencil stencilComponent, short worldID, int entityID)
    {
        ref var component = ref UnsafeUtility.As<TStencil, TComponent>(ref stencilComponent);
        _apply(ref component);


        EcsWorld w = null;
        var pool = w.GetPoolInstance<TPool>();
        _apply2(ref component, pool);


        //EcsWorld.GetPoolInstance<EcsPool<T>>(worldID).TryAddOrGet(entityID) = component;
    }
    public override object GetRaw(ref TStencil stencilComponent)
    {
        ref var component = ref UnsafeUtility.As<TStencil, TComponent>(ref stencilComponent);
        return component;
    }
    public override void SetRaw(ref TStencil stencilComponent, object raw)
    {
        TComponent component = (TComponent)raw;
        stencilComponent = UnsafeUtility.As<TComponent, TStencil>(ref component);
    }
}


public struct TTT : IEcsComponent { }
public class F
{
    public void Do1<T>(ref T data) where T : struct, IEcsComponent { }
    public void Do2<T>(ref T data, EcsPool<T> pool) where T : struct, IEcsComponent { }

    public delegate void DoHandler<T>(ref T data);
    public delegate void DoHandler<T, TPool>(ref T data, TPool pool);

    public void Do()
    {
        DoHandler<TTT> dodo1 = Do1;
        DoHandler<TTT, EcsPool<TTT>> dodo2 = Do2;
    }
}



public class Template_GUID : GeneratedComponentTemplateBase<Component_GUID>
{
    static Template_GUID() { InitStatic("AssemblyQualifiedName", "AssemblyQualifiedName"); }
}
[System.Serializable]
public struct Component_GUID
{
    // data...
}
