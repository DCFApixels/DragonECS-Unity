using DCFApixels.DragonECS.Unity.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Color = UnityEngine.Color;
using UnityObject = UnityEngine.Object;

namespace DCFApixels.DragonECS.Unity.Editors
{
    internal unsafe static partial class EcsGUI
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct PtrRefUnion
        {
            [FieldOffset(0)]
            public byte* Ptr;
            [FieldOffset(0)]
            public object Ref;
            public PtrRefUnion(byte* ptr) : this()
            {
                Ptr = ptr;
            }
        }

        #region Value
        private readonly unsafe struct Value
        {
            public readonly byte* FieldPtr;
            public readonly byte* ValuePtr;
            public Value(byte* fieldPtr, byte* valuePtr)
            {
                FieldPtr = fieldPtr;
                ValuePtr = valuePtr;
            }
            public Value Read(in InspectorFieldInfo field)
            {
                return Read(in this, field.Offset, field.Flag == FieldFlag.Ref);
            }
            public Value Read(int fieldOffset, bool isRef)
            {
                return Read(in this, fieldOffset, isRef);
            }
            public static Value Read(in Value source, in InspectorFieldInfo field)
            {
                return Read(in source, field.Offset, field.Flag == FieldFlag.Ref);
            }
            public static Value Read(in Value source, int fieldOffset, bool isRef)
            {
                byte* fieldPtr = source.ValuePtr + fieldOffset;
                byte* valuePtr;
                if (isRef)
                {
                    IntPtr* refPtr = (IntPtr*)fieldPtr;
                    valuePtr = (byte*)(refPtr[0]);
                }
                else
                {
                    valuePtr = fieldPtr;
                }
                return new Value(fieldPtr, valuePtr);
            }
            public ref T AsValue<T>() where T : struct
            {
#if DEV_MODE
                if (FieldPtr != ValuePtr) { throw new Exception(); }
#endif
                return ref UnsafeUtility.AsRef<T>(FieldPtr);
            }
            public T AsRef<T>() where T : class
            {
#if DEV_MODE
                if (FieldPtr == ValuePtr) { throw new Exception(); }
#endif

                //ref IntPtr p = ref UnsafeUtility.AsRef<IntPtr>(ValuePtr);
                //byte* pp = (byte*)p;
                //
                //Union union = default;
                ////union.Ptr = ValuePtr; // ValuePtr это какраз и есть реф
                //union.Ptr = pp; // ValuePtr это какраз и есть реф
                ////object result = union.Ref;
                //
                //object result = UnsafeUtility.As<IntPtr, object>(ref p);
                //return (T)result;

                PtrRefUnion union = default;
                union.Ptr = ValuePtr; // ValuePtr это какраз и есть реф
                object result = union.Ref;
                return (T)result;
            }
            public object AsRef()
            {
                return AsRef<object>();
            }
        }
        #endregion

        #region FieldValueProcessor
        private abstract class FieldValueProcessor
        {
            private static readonly Dictionary<Type, FieldValueProcessor> _processors = new Dictionary<Type, FieldValueProcessor>();
            private static readonly StructFieldValueProcessor _defaultProcessor = new StructFieldValueProcessor();
            static FieldValueProcessor()
            {
                _processors.Clear();
                _defaultProcessor._valueInfo = InspectorTypeInfo.Get(typeof(object));
                _processors.Add(typeof(object), _defaultProcessor);
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var processorType in assembly.GetTypes())
                    {
                        if (processorType.IsGenericType == false &&
                            processorType.IsAbstract == false &&
                            typeof(FieldValueProcessor).IsAssignableFrom(processorType) &&
                            processorType != typeof(StructFieldValueProcessor))
                        {
                            var processor = (FieldValueProcessor)Activator.CreateInstance(processorType);
                            processor._valueInfo = InspectorTypeInfo.Get(processor.ProcessedType);
                            _processors.Add(processor.ProcessedType, processor);
                        }
                    }
                }
            }
            public static FieldValueProcessor GetProcessor(Type type)
            {
                if(type == null)
                {
                    throw new ArgumentNullException("type");
                }

                if (_processors == null)
                {
                    throw new Exception("_processors == null");
                }
                if (_processors.TryGetValue(type, out FieldValueProcessor processor) == false)
                {
                    FieldValueProcessor otherProcessor;
                    if (type.IsValueType)
                    {
                        otherProcessor = _defaultProcessor;
                    }
                    else
                    {
                        otherProcessor = GetProcessor(type.BaseType);
                    }

                    processor = (FieldValueProcessor)otherProcessor.MemberwiseClone();
                    processor._valueInfo = InspectorTypeInfo.Get(type);
                    _processors.Add(type, processor);
                }
                return processor;
            }


            private InspectorTypeInfo _valueInfo;
            public InspectorTypeInfo ValueInfo
            {
                get { return _valueInfo; }
            }
            public bool IsDefault
            {
                get { return this == _defaultProcessor; }
            }
            public abstract Type ProcessedType { get; }
            public abstract float GetHeight(in InspectorFieldInfo field, in Value value);
            public abstract void Draw(Rect rect, in InspectorFieldInfo field, in Value value);

            public override string ToString()
            {
                if(_valueInfo == null)
                {
                    return "ERROR";
                }
                return _valueInfo.Type.Name + " " + GetType().Name;
            }
        }
        private class StructFieldValueProcessor : FieldValueProcessor
        {
            public override Type ProcessedType { get { return typeof(object); } }
            public override float GetHeight(in InspectorFieldInfo field, in Value value)
            {
                float result = GetPropertyHeight(field, ValueInfo, value);
                //Debug.Log("GetHeight: " + result + " : " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                return result;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value value)
            {
                //Debug.Log("Draw: " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                DrawProperty(rect, field, ValueInfo, value);
            }
        }
        private class ValueFieldValueProcessor<T> : StructFieldValueProcessor where T : struct
        {
            public sealed override Type ProcessedType { get { return typeof(T); } }
            public sealed override float GetHeight(in InspectorFieldInfo field, in Value value)
            {
                float result = GetHeight(field, value, ref value.AsValue<T>());
                //Debug.Log("GetHeight: " + result + " : " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                return result;
            }
            public sealed override void Draw(Rect rect, in InspectorFieldInfo field, in Value value)
            {
                //Debug.Log("Draw: " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                Draw(rect, field, value, ref value.AsValue<T>());
            }
            public virtual float GetHeight(in InspectorFieldInfo field, in Value raw, ref T value)
            {
                return base.GetHeight(field, raw);
            }
            public virtual void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref T value)
            {
                base.Draw(rect, in field, in raw);
            }
        }
        private class RefFieldValueProcessor<T> : StructFieldValueProcessor where T : class
        {
            public sealed override Type ProcessedType { get { return typeof(T); } }
            public sealed override float GetHeight(in InspectorFieldInfo field, in Value value)
            {
                float result = GetHeight(field, value, value.AsRef<T>());
                //Debug.Log("GetHeight: " + result + " : " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                return result;
            }
            public sealed override void Draw(Rect rect, in InspectorFieldInfo field, in Value value)
            {
                //Debug.Log("Draw: " + ValueInfo.Type + " : " + ValueInfo.Fields + " : " + ValueInfo.IsNull);
                Draw(rect, field, value, value.AsRef<T>());
            }
            public virtual float GetHeight(in InspectorFieldInfo field, in Value raw, T value)
            {
                return base.GetHeight(field, raw);
            }
            public virtual void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, T value)
            {
                base.Draw(rect, in field, in raw);
            }
        }
        private unsafe class ArrayFieldValueProcessor : RefFieldValueProcessor<Array>
        {
            private static readonly int _mappingDataOffset;
            static ArrayFieldValueProcessor()
            {
                Array array = new int[] { 1, 2, 3 };
                ulong handle;

                byte* ptrObject = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(array, out handle);
                UnsafeUtility.ReleaseGCObject(handle);
                byte* ptrData = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out handle);
                UnsafeUtility.ReleaseGCObject(handle);

                _mappingDataOffset = (int)(ptrData - ptrObject);
            }


            private readonly ReorderableList _listDragCache; // сюда попадает _list если было вызвано событие драг енд дропа, а изначальный _listDragCache перемещается в _list
            private readonly ReorderableList _list;
            public ArrayFieldValueProcessor()
            {
                //_list.list
            }
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, Array value)
            {
                //var elemType = value.GetType().GetElementType();
                //int length = value.Length;
                //byte* ptr = raw.ValuePtr + _mappingDataOffset;
                //
                //
                //_list.heigh
                //
                //ReorderableList l = null;
                //l.onMouseDragCallback
                //
                //float result = 0;
                //var processor = GetProcessor(elemType);
                //for (int i = 0; i < length; i++)
                //{
                //
                //}

                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, Array value)
            {
                //var elemType = value.GetType().GetElementType();
                //int length = value.Length;
                //byte* ptr = raw.ValuePtr + _mappingDataOffset;
                //

                EditorGUI.LabelField(rect, UnityEditorUtility.GetLabel(field.Name), "Array Unsupport");
            }
        }
        private class SByteFieldValueProcessor : ValueFieldValueProcessor<sbyte>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref sbyte value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref sbyte value)
            {
                value = (sbyte)EditorGUI.IntField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class ByteFieldValueProcessor : ValueFieldValueProcessor<byte>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref byte value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref byte value)
            {
                value = (byte)EditorGUI.IntField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class ShortFieldValueProcessor : ValueFieldValueProcessor<short>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref short value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref short value)
            {
                value = (short)EditorGUI.IntField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class UShortFieldValueProcessor : ValueFieldValueProcessor<ushort>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref ushort value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref ushort value)
            {
                value = (ushort)EditorGUI.IntField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class IntFieldValueProcessor : ValueFieldValueProcessor<int>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref int value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref int value)
            {
                value = EditorGUI.IntField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class UIntFieldValueProcessor : ValueFieldValueProcessor<uint>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref uint value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref uint value)
            {
                value = (uint)EditorGUI.LongField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class LongFieldValueProcessor : ValueFieldValueProcessor<long>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref long value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref long value)
            {
                value = EditorGUI.LongField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class ULongFieldValueProcessor : ValueFieldValueProcessor<ulong>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref ulong value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref ulong value)
            {
                value = (ulong)EditorGUI.LongField(rect, UnityEditorUtility.GetLabel(field.Name), (long)value);
            }
        }
        private class FloatFieldValueProcessor : ValueFieldValueProcessor<float>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref float value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref float value)
            {
                value = EditorGUI.FloatField(rect, UnityEditorUtility.GetLabel(field.Name), (long)value);
            }
        }
        private class DoubleFieldValueProcessor : ValueFieldValueProcessor<double>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref double value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref double value)
            {
                value = EditorGUI.DoubleField(rect, UnityEditorUtility.GetLabel(field.Name), (long)value);
            }
        }
        private class CharFieldValueProcessor : ValueFieldValueProcessor<char>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref char value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref char value)
            {
                var result = EditorGUI.TextField(rect, UnityEditorUtility.GetLabel(field.Name), value.ToString());
                if(result.Length > 0)
                {
                    value = result[0];
                }
                value = default;
            }
        }
        private class ColorFieldValueProcessor : ValueFieldValueProcessor<Color>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, ref Color value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, ref Color value)
            {
                value = EditorGUI.ColorField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class StringFieldValueProcessor : RefFieldValueProcessor<string>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, string value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, string value)
            {
                value = EditorGUI.TextField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class GradientFieldValueProcessor : RefFieldValueProcessor<Gradient>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, Gradient value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, Gradient value)
            {
                value = EditorGUI.GradientField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class CurveFieldValueProcessor : RefFieldValueProcessor<AnimationCurve>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, AnimationCurve value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, AnimationCurve value)
            {
                value = EditorGUI.CurveField(rect, UnityEditorUtility.GetLabel(field.Name), value);
            }
        }
        private class UnityObjectFieldValueProcessor : RefFieldValueProcessor<UnityObject>
        {
            public override float GetHeight(in InspectorFieldInfo field, in Value raw, UnityObject value)
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            public override void Draw(Rect rect, in InspectorFieldInfo field, in Value raw, UnityObject value)
            {
                value = EditorGUI.ObjectField(rect, UnityEditorUtility.GetLabel(field.Name), value, ValueInfo.Type, true);
            }
        }
        #endregion

        #region InspectorTypeInfo
        private class InspectorTypeInfo
        {
            #region cahce
            private static Dictionary<Type, InspectorTypeInfo> _typeInfosCache = new Dictionary<Type, InspectorTypeInfo>();
            static InspectorTypeInfo()
            {
                _typeInfosCache.Clear();
            }
            public static InspectorTypeInfo Get(Type type)
            {
                if (_typeInfosCache.TryGetValue(type, out InspectorTypeInfo info) == false)
                {
                    info = new InspectorTypeInfo(type);
                    _typeInfosCache.Add(type, info);
                }

                return info;
            }
            #endregion   
            private struct FakeNull { }
            public static readonly InspectorTypeInfo PtrTypeInfo = new InspectorTypeInfo(typeof(IntPtr));
            public static readonly InspectorTypeInfo NullTypeInfo = new InspectorTypeInfo(typeof(FakeNull));

            public readonly Type Type;
            //public readonly int Size;
            public readonly InspectorFieldInfo[] Fields;
            //public readonly float DefaultInspectorHegiht;

            public readonly bool IsVector;
            public readonly bool IsColor;

            public bool IsNull
            {
                get { return this == NullTypeInfo; }
            }
            public bool IsPrt
            {
                get { return this == PtrTypeInfo; }
            }

            #region Constructors
            //private static StructList<InspectorFieldInfo> CnstrBuffer = new StructList<InspectorFieldInfo>(32);
            private static readonly char[] VectorFields = new char[] { 'x', 'y', 'z', 'w' };
            private static readonly char[] ColorFields = new char[] { 'r', 'g', 'b', 'a' };
            public InspectorTypeInfo(Type type)
            {
                StructList<InspectorFieldInfo> CnstrBuffer = new StructList<InspectorFieldInfo>(32);
                //if (CnstrBuffer.IsNull)
                //{
                //    CnstrBuffer = new StructList<InspectorFieldInfo>(32);
                //}
                CnstrBuffer.FastClear();
                Type = type;
                //Size = UnsafeUtility.SizeOf(type);
                var fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var fieldInfo in fieldInfos)
                {
                    InspectorFieldInfo.Union infoUniton = default;
                    ref InspectorFieldInfo.Constructor infocstr = ref infoUniton.Constructor;

                    var fieldType = fieldInfo.FieldType;

                    infocstr.FieldInfo = fieldInfo;
                    infocstr.Offset = UnsafeUtility.GetFieldOffset(fieldInfo);
                    infocstr.Name = UnityEditorUtility.TransformFieldName(fieldInfo.Name);
                    infocstr.Flag = FieldFlagUtitlity.GetFieldFlag(fieldType);

                    if (infocstr.Flag == FieldFlag.StructValue)
                    {
                        infocstr.PreDefinedType = Get(fieldType);
                    }
                    else if (infocstr.Flag == FieldFlag.Ref)
                    {
                        infocstr.PreDefinedType = PtrTypeInfo;
                    }
                    else
                    {
                        if (type == infocstr.FieldInfo.FieldType)
                        {
                            infocstr.PreDefinedType = NullTypeInfo;
                        }
                        else
                        {
                            infocstr.PreDefinedType = Get(fieldType);
                        }
                    }
                    

                    CnstrBuffer.Add(infoUniton.Result);
                }

                Fields = CnstrBuffer.Enumerable.ToArray();
                Array.Sort(Fields);

                int length = Fields.Length;
                if (length >= 2 && length <= 3)
                {
                    bool isCheck = true;
                    for (int i = 0; i < length; i++)
                    {
                        ref var field = ref Fields[i];
                        
                        if (field.Flag == FieldFlag.LeafValue &&
                            field.PreDefinedType.Type.IsPrimitive &&
                            char.ToLower(field.Name[0]) != VectorFields[i])
                        {
                            isCheck = false;
                            break;
                        }
                    }
                    if (isCheck)
                    {
                        IsVector = true;
                    }
                    else
                    {
                        isCheck = true;
                        for (int i = 0; i < length; i++)
                        {
                            ref var field = ref Fields[i];
                            if (field.Flag == FieldFlag.LeafValue &&
                                field.PreDefinedType.Type.IsPrimitive && 
                                char.ToLower(field.Name[0]) != ColorFields[i])
                            {
                                isCheck = false;
                                break;
                            }
                        }
                        if (isCheck)
                        {
                            IsColor = true;
                        }
                    }
                }
            }
            #endregion

            public override string ToString()
            {
                return Type.Name;
            }
        }
        #endregion

        #region InspectorFieldInfo
        [StructLayout(LayoutKind.Sequential)]
        private unsafe readonly struct InspectorFieldInfo : IEquatable<InspectorFieldInfo>, IComparable<InspectorFieldInfo>
        {
            public readonly FieldInfo FieldInfo;
            public readonly InspectorTypeInfo PreDefinedType;
            public readonly string Name;
            public readonly int Offset;
            public readonly FieldFlag Flag;

            public int CompareTo(InspectorFieldInfo other)
            {
                return Offset - other.Offset;
            }
            public bool Equals(InspectorFieldInfo other)
            {
                return EqualityComparer<InspectorFieldInfo>.Default.Equals(this, other);
            }
            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct Constructor
            {
                public FieldInfo FieldInfo;
                public InspectorTypeInfo PreDefinedType;
                public string Name;
                public int Offset;
                public FieldFlag Flag;
                public override string ToString() { return Name; }
            }
            [StructLayout(LayoutKind.Explicit)]
            public struct Union
            {
                [FieldOffset(0)]
                public Constructor Constructor;
                [FieldOffset(0)]
                public InspectorFieldInfo Result;
            }
            public override string ToString() { return Name; }
        }
        private static class FieldFlagUtitlity
        {
            private static readonly HashSet<Type> _cantDisplayedTypes = new HashSet<Type>()
                {
                    typeof(IntPtr),
                    typeof(UIntPtr),
                };
            private static readonly HashSet<Type> _leafTypes = new HashSet<Type>()
                {
                    typeof(string),
                    typeof(AnimationCurve),
                    typeof(Gradient),
                    typeof(LayerMask),
                    typeof(Color),
                };
            public static FieldFlag GetFieldFlag(Type fieldType)
            {
                if (fieldType.IsPointer || _cantDisplayedTypes.Contains(fieldType))
                {
                    return FieldFlag.CantDisplayed;
                }
                if (fieldType.IsClass || fieldType.IsInterface)
                {
                    return FieldFlag.Ref;
                }
                if (fieldType.IsPrimitive || fieldType.IsEnum || _leafTypes.Contains(fieldType))
                {
                    return FieldFlag.LeafValue;
                }
                return FieldFlag.StructValue;
            }
            public static bool IsValueField(FieldFlag flag)
            {
                return flag == FieldFlag.LeafValue || flag == FieldFlag.StructValue;
            }
            public static bool IsCanDisplayed(FieldFlag flag)
            {
                return flag != FieldFlag.None && flag != FieldFlag.CantDisplayed;
            }
        }
        private enum FieldFlag : byte
        {
            None = 0,
            CantDisplayed,
            LeafValue,
            StructValue,
            Ref,
        }
        #endregion

        private static void DrawProperty(Rect rect, in InspectorFieldInfo field, InspectorTypeInfo valueInfo, in Value value)
        {
            //if (field.Flag == FieldFlag.LeafValue)
            //{
            //    EditorGUI.LabelField(rect, UnityEditorUtility.GetLabel(field.Name));
            //    return;
            //}
            float y = rect.y;
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect subRect = rect;
            subRect.y = y;
            subRect.height = height - EditorGUIUtility.standardVerticalSpacing;
            y += height;

            EditorGUI.LabelField(subRect, field.Name);

            using (UpIndentLevel())
            {
                foreach (var subField in valueInfo.Fields)
                {
                    if (FieldFlagUtitlity.IsCanDisplayed(subField.Flag) == false) { continue; }

                    var subValue = value.Read(subField);
                    Type subValueType = null; 

                    switch (subField.Flag)
                    {
                        case FieldFlag.LeafValue:
                        case FieldFlag.StructValue:
                            subValueType = subField.PreDefinedType.Type;
                            break;
                        case FieldFlag.Ref:
                            var obj = subValue.AsRef();
                            subValueType = obj.GetType();
                            break;
                    }

                    if (subValueType == null) { continue; }

                    var processor = FieldValueProcessor.GetProcessor(subValueType);
                    height = processor.GetHeight(subField, in subValue);
                    subRect = rect;
                    subRect.y = y;
                    subRect.height = height - EditorGUIUtility.standardVerticalSpacing;
                    y += height;

                    if (subField.Flag == FieldFlag.StructValue && processor.IsDefault)
                    {
                        if (subField.PreDefinedType.IsVector)
                        {
                            float defLabelWidth = EditorGUIUtility.labelWidth;
                            EditorGUIUtility.labelWidth = 14f;
                            subRect.xMin += EditorGUIUtility.labelWidth;
                            var vectorFields = subField.PreDefinedType.Fields;
                            var widthOne = subRect.width / vectorFields.Length;
                            foreach (var vectorField in vectorFields)
                            {
                                var vectorFieldProcessor = FieldValueProcessor.GetProcessor(vectorField.PreDefinedType.Type);
                                vectorFieldProcessor.Draw(subRect, in vectorField, subValue.Read(vectorField));
                            }
                            EditorGUIUtility.labelWidth = defLabelWidth;
                        }
                        //else if (subField.PreDefinedType.IsColor)
                        //{
                        //    EditorGUI.ColorField(subRect, );
                        //}
                    }

                    processor.Draw(subRect, in subField, in subValue);
                }
            }
        }
        private static float GetPropertyHeight(in InspectorFieldInfo field, InspectorTypeInfo valueInfo, in Value value)
        {
            float result = 0;
            //if (field.Flag == FieldFlag.LeafValue)
            //{
            //    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //}

            //Debug.Log(string.Join("\r\n", valueInfo.Fields));

            foreach (var subField in valueInfo.Fields)
            {
                if (FieldFlagUtitlity.IsCanDisplayed(subField.Flag) == false) { continue; }

                var subValue = value.Read(subField);
                Type subValueType = null;

                switch (subField.Flag)
                {
                    case FieldFlag.LeafValue:
                    case FieldFlag.StructValue:
                        subValueType = subField.PreDefinedType.Type;
                        break;
                    case FieldFlag.Ref:
                        var obj = subValue.AsRef();
                        subValueType = obj.GetType();
                        break;
                }

                if (subValueType == null) { continue; }

                var processor = FieldValueProcessor.GetProcessor(subValueType);
                if (subField.Flag == FieldFlag.StructValue && processor.IsDefault)
                {
                    if (subField.PreDefinedType.IsVector)
                    //if (subField.PreDefinedType.IsVector || subField.PreDefinedType.IsColor)
                    {
                        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                result += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                result += processor.GetHeight(subField, in subValue);
            }
            return result;
        }
        [StructLayout(LayoutKind.Explicit)]
        private ref struct RefPtrUnion
        {
            [FieldOffset(0)]
            public object Ref;
            [FieldOffset(0)]
            public IntPtr Ptr;
            public RefPtrUnion(object @ref) : this()
            {
                Ref = @ref;
            }
            public RefPtrUnion(IntPtr ptr) : this()
            {
                Ptr = ptr;
            }
        }
        public static unsafe partial class Layout
        {
            private static bool DrawProperty(object data, string name)
            {
                //Debug.LogWarning("--------------------------------------------");
                EditorGUI.BeginChangeCheck();
                Type type = data.GetType();
                //byte* ptr = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(data, out ulong gcHandle);
                RefPtrUnion union = new RefPtrUnion(data);
                byte* ptr = (byte*)union.Ptr;
                ptr += sizeof(IntPtr) * 2; //TODO тут надо просчитать что констатно ли значение смещения для упакованных данных
                InspectorTypeInfo inspectorTypeInfo = InspectorTypeInfo.Get(type);
                Value value = new Value(ptr, ptr);

                InspectorFieldInfo.Union f = default;
                f.Constructor.Name = name;
                float h = GetPropertyHeight(in f.Result, inspectorTypeInfo, in value);
                var r = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, h);

                EcsGUI.DrawProperty(r, in f.Result, inspectorTypeInfo, in value);


                return EditorGUI.EndChangeCheck();
            }
        }
    }
}
