#if UNITY_EDITOR
using DCFApixels.DragonECS;
using DCFApixels.DragonECS.PoolsCore;
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

    public static TypeScan SkanTypeStructure(Type type)
    {
        return TypeScan.Scan(type);
    }
    private static bool IsSerializableField(FieldInfo fieldInfo)
    {
        return fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null || fieldInfo.GetCustomAttribute<SerializeReference>() != null;
    }
    private static bool IsCanUnsafeOverwrite(FieldInfo fieldInfo, bool isLeaf)
    {
        if (isLeaf)
        {
            return true;
        }
        if (fieldInfo.FieldType.IsValueType)
        {
            return true;
        }

        return false;
    }
    private static bool IsLeafField(FieldInfo fieldInfo)
    {
        if (fieldInfo.FieldType.IsEnum)
        {
            return true;
        }
        if (fieldInfo.FieldType.IsPrimitive)
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

    public class TypeScan
    {
        public static readonly TypeScan NoSerializableScan = new TypeScan();

        public readonly bool IsSerializable;
        public readonly bool IsCanUnsafeOverwrite;

        public readonly bool IsClass;
        public readonly bool IsStruct;

        public readonly FieldScan[] Fields = Array.Empty<FieldScan>();
        public readonly Type SerializableType;

        //public readonly string[] EnumFlagNames; 

        private TypeScan() { }
        private TypeScan(Type type)
        {
            IsSerializable = true;
            SerializableType = type;



            IsCanUnsafeOverwrite = true;
            ReadOnlySpan<FieldScan> fields = FieldScan.GetFieldScans(type);
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.IsCanUnsafeOverwrite == false)
                {
                    IsCanUnsafeOverwrite = false;
                    break;
                }
            }

            Fields = fields.ToArray();
        }
        public static TypeScan Scan(Type type)
        {
            if (type.GetCustomAttribute<System.SerializableAttribute>() == null)
            {
                return NoSerializableScan;
            }
            return new TypeScan(type);
        }
    }

    public class FieldScan
    {
        public static readonly FieldScan NoSerializableScan = new FieldScan();

        public readonly bool IsSerializable;

        public readonly bool IsSerializeField;
        public readonly bool IsSerializeReference;
        public readonly bool IsUnityObject;

        public readonly bool IsLeaf;
        public readonly bool IsCanUnsafeOverwrite;
        public readonly TypeScan ValueTypeScan;
        public readonly FieldInfo SerializableFieldInfo;

        //public readonly 

        private FieldScan() { }
        private FieldScan(FieldInfo field)
        {
            IsSerializable = true;
            SerializableFieldInfo = field;
            IsLeaf = IsLeafField(field);
            IsCanUnsafeOverwrite = IsCanUnsafeOverwrite(field, IsLeaf);

            if (field.FieldType.IsValueType)
            {
                ValueTypeScan = TypeScan.Scan(field.FieldType);
            }
        }


        private static FieldScan[] FieldsBuffer = new FieldScan[64];
        private static int FieldsBufferCount = 0;
        public static ReadOnlySpan<FieldScan> GetFieldScans(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (FieldsBuffer == null || FieldsBuffer.Length < fields.Length)
            {
                var newsize = DCFApixels.DragonECS.Unity.Internal.ArrayUtility.NormalizeSizeToPowerOfTwo(fields.Length);
                FieldsBuffer = new FieldScan[newsize];
            }
            for (int i = 0; i < fields.Length; i++)
            {
                FieldsBuffer[i] = Scan(fields[i]);
                FieldsBufferCount++;
            }
            return new ReadOnlySpan<FieldScan>(FieldsBuffer, 0, FieldsBufferCount);
        }
        public static void CleanGetFieldScansBuffer()
        {
            for (int i = 0; i < FieldsBufferCount; i++)
            {
                FieldsBuffer[i] = null;
            }
        }
        public static FieldScan Scan(FieldInfo field)
        {
            if (IsSerializableField(field) == false)
            {
                return NoSerializableScan;
            }

            return new FieldScan(field);
        }
    }

    //public class AttributeScan { } // TODO
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
