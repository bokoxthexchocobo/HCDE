using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Debugger array node from <c>ArrayStateNode.cpp</c> (1-259), through end of file.
/// Chunk 1355. <c>CacheChildren</c> is declared and not defined. Pointer unwrap
/// keeps the original array address. <c>NewPointer</c>, <c>GetVMValue</c>, and
/// <c>CreateNodeForVariable</c> stay injected.
/// </summary>
public sealed class VmArrayStateNode1355 : Node1355
{
    public string Name = "";
    public string EvalName = "";
    public VmWord1355 Value = new();
    public Type1355 Type = new();
    public Type1355? Element;
    public ArrayServices1355 Services = new();
    readonly Dictionary<string, Node1355?> _children = new();

    public VmArrayStateNode1355(string name, VmWord1355 value, Type1355 type, ArrayServices1355 services)
    {
        Name = name;
        EvalName = name;
        Value = value;
        Type = type;
        Services = services;
        Type1355 walked = type;
        if (walked.IsPointer)
        {
            walked = walked.Pointed ?? throw new NullReferenceException("PointedType");
        }
        if (walked.IsArray)
        {
            Element = walked.ElementType;
        }
        else if (walked.IsDynArray)
        {
            Element = walked.ElementType;
        }
    }

    public int GetId()
    {
        return Id;
    }

    public static bool IsValid(VmWord1355? val)
    {
        if (val == null || val.Bits == 0)
            return false;
        return (uint)val.Bits != 0;
    }

    public static Type1355? GetElementType(Type1355 type)
    {
        Type1355 walked = type;
        if (type.IsPointer)
        {
            walked = walked.Pointed ?? throw new NullReferenceException("PointedType");
        }
        if (walked.IsArray)
        {
            return walked.ElementType;
        }
        if (walked.IsDynArray)
        {
            return walked.ElementType;
        }
        return null;
    }

    public static long GetElementCount(VmWord1355 value, Type1355 type)
    {
        Type1355 walked = type;
        if (walked.IsPointer)
        {
            walked = walked.Pointed ?? throw new NullReferenceException("PointedType");
        }
        if (walked.IsDynArray || walked.IsStaticArray)
        {
            if (!IsValid(value))
                return 0;
            HeapArray1355 header = value.Array ?? throw new NullReferenceException("array");
            if (header.Count == uint.MaxValue)
                return -1;
            return header.Count;
        }
        if (walked.IsArray)
        {
            if (walked.ElementCount == uint.MaxValue)
                return -1;
            return walked.ElementCount;
        }
        return 0;
    }

    public static bool ParseInt(string text, out int value)
    {
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        if (i >= text.Length)
            throw new FormatException("stoi");
        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        if (i >= text.Length || text[i] < '0' || text[i] > '9')
            throw new FormatException("stoi");
        long acc = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            int digit = text[i] - '0';
            if (acc > (long.MaxValue - digit) / 10)
                throw new OverflowException("stoi");
            acc = acc * 10 + digit;
            i++;
        }
        if (sign < 0)
        {
            if (acc > 2147483648L)
                throw new OverflowException("stoi");
            value = unchecked((int)(-acc));
        }
        else
        {
            if (acc > 2147483647L)
                throw new OverflowException("stoi");
            value = (int)acc;
        }
        return true;
    }

    public void SetVariableName(Var1355 variable)
    {
        variable.Name = Name;
        if (EvalName != Name)
            variable.EvaluateName = EvalName;
    }

    public bool Serialize(Var1355 variable)
    {
        variable.VariablesReference = GetId();
        long count = GetElementCount(Value, Type);
        variable.IndexedVariables = count < 0 ? 0 : count;
        SetVariableName(variable);
        if (Element == null)
            throw new NullReferenceException("m_elementType");
        string elementTypeName = Element.Descriptive;
        variable.Type = Type.Descriptive;
        if (!IsValid(Value) || count < 0)
        {
            variable.Value = elementTypeName + "[<NONE>]";
        }
        else
        {
            long indexed = variable.IndexedVariables ?? 0L;
            variable.Value = elementTypeName + "[" + indexed.ToString(CultureInfo.InvariantCulture) + "]";
            if (Type.IsPointer)
                variable.Value += " (" + FormatPointer(Value.Bits) + ")";
        }
        return true;
    }

    public bool GetChildNames(List<string> names)
    {
        if (!IsValid(Value))
            return true;
        long count = GetElementCount(Value, Type);
        for (uint i = 0; i < count; i++)
            names.Add(i.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    public bool GetChildNode(string name, ref Node1355? node)
    {
        if (!IsValid(Value))
            return false;
        long count = GetElementCount(Value, Type);
        if (count <= 0)
            return false;
        if (!ParseInt(name, out int elementIndex))
            return false;
        if (elementIndex < 0 || (uint)elementIndex > count - 1)
            return false;
        string key = elementIndex.ToString(CultureInfo.InvariantCulture);
        if (_children.TryGetValue(key, out Node1355? cached))
        {
            node = cached;
            return true;
        }
        if (Value.Bits == 0)
            return false;
        Type1355 walked = Type;
        if (Type.IsPointer)
        {
            walked = Type.Pointed ?? throw new NullReferenceException("PointedType");
        }
        if (walked.IsDynArray || walked.IsStaticArray)
        {
            Type1355? elementType = GetElementType(Type);
            if (elementType == null)
                throw new NullReferenceException("ElementType");
            Type1355? returnType = elementType;
            if (elementType.Size > 8)
                returnType = CallNewPointer(elementType);
            VmWord1355 elementVal = ReadElement(Value.Array, elementType, elementIndex);
            _children[key] = CallCreate(key, elementVal, returnType);
        }
        else if (walked.IsArray)
        {
            if (Element == null)
                throw new NullReferenceException("m_elementType");
            ulong address = unchecked(Value.Bits + (ulong)(uint)Element.Size * (ulong)elementIndex);
            if (Services.GetVmValue == null)
                throw new InvalidOperationException("GetVMValue");
            VmWord1355 read = Services.GetVmValue(address, Element);
            _children[key] = CallCreate(key, read, Element);
        }
        if (!_children.ContainsKey(key))
            _children[key] = null;
        node = _children[key];
        return true;
    }

    Type1355? CallNewPointer(Type1355 elementType)
    {
        if (Services.NewPointer == null)
            throw new InvalidOperationException("NewPointer");
        return Services.NewPointer(elementType);
    }

    Node1355? CallCreate(string key, VmWord1355 word, Type1355? type)
    {
        if (Services.CreateNode == null)
            throw new InvalidOperationException("CreateNodeForVariable");
        return Services.CreateNode(key, word, type);
    }

    static VmWord1355 ReadElement(HeapArray1355? arr, Type1355 elementType, int index)
    {
        if (elementType.IsFloat)
        {
            if (elementType.Size == 8)
            {
                HeapArray1355 header = arr ?? throw new NullReferenceException("array");
                CheckIndex(header, index);
                long bits = (long)ReadLe(RequireData(header), index * 8, 8);
                return VmWord1355.FromDouble(BitConverter.Int64BitsToDouble(bits));
            }
            if (elementType.Size == 4)
            {
                HeapArray1355 header = arr ?? throw new NullReferenceException("array");
                CheckIndex(header, index);
                int bits = (int)ReadLe(RequireData(header), index * 4, 4);
                return VmWord1355.FromDouble(BitConverter.Int32BitsToSingle(bits));
            }
            return new VmWord1355();
        }
        HeapArray1355 got = arr ?? throw new NullReferenceException("array");
        CheckIndex(got, index);
        if (elementType.IsObjectPointer)
            return VmWord1355.FromPointer(ReadLe(RequireData(got), index * 8, 8));
        if (ReferenceEquals(elementType, Type1355.StringType))
            return VmWord1355.FromPointer(unchecked(got.DataAddress + (ulong)index * (uint)elementType.Size));
        if (elementType.IsPointer)
            return VmWord1355.FromPointer(ReadLe(RequireData(got), index * 8, 8));
        switch (elementType.Size)
        {
            case 8:
                return VmWord1355.FromPointer(ReadLe(RequireData(got), index * 8, 8));
            case 4:
                return VmWord1355.FromUInt((uint)ReadLe(RequireData(got), index * 4, 4));
            case 2:
                return VmWord1355.FromInt((int)ReadLe(RequireData(got), index * 2, 2));
            case 1:
                return VmWord1355.FromInt((int)ReadLe(RequireData(got), index, 1));
            default:
                return VmWord1355.FromPointer(unchecked(got.DataAddress + (ulong)index * (uint)elementType.Size));
        }
    }

    static void CheckIndex(HeapArray1355 arr, int index)
    {
        if ((uint)index > arr.Count)
            throw new InvalidOperationException("index");
    }

    static byte[] RequireData(HeapArray1355 arr)
    {
        if (arr.Data == null)
            throw new NullReferenceException("Array");
        return arr.Data;
    }

    static ulong ReadLe(byte[] bytes, int offset, int size)
    {
        if (offset < 0 || size < 0 || size > bytes.Length || offset > bytes.Length - size)
            throw new NullReferenceException("Array");
        ulong v = 0;
        for (int b = 0; b < size; b++)
            v |= (ulong)bytes[offset + b] << (8 * b);
        return v;
    }

    public static string FormatPointer(ulong bits)
    {
        if (bits == 0)
            return "(nil)";
        return "0x" + bits.ToString("x", CultureInfo.InvariantCulture);
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }
}

public class Node1355
{
    public int Id;
}

public sealed class VmWord1355
{
    public ulong Bits;
    public HeapArray1355? Array;

    public static VmWord1355 FromInt(int v) => new() { Bits = (uint)v };

    public static VmWord1355 FromUInt(uint v) => new() { Bits = v };

    public static VmWord1355 FromDouble(double v) => new() { Bits = (ulong)BitConverter.DoubleToInt64Bits(v) };

    public static VmWord1355 FromPointer(ulong v) => new() { Bits = v };
}

public sealed class HeapArray1355
{
    public byte[]? Data;
    public ulong DataAddress;
    public uint Count;
    public uint Most;
}

public sealed class Type1355
{
    public static readonly Type1355 StringType = new() { Descriptive = "String", Size = 8 };

    public bool IsPointer;
    public bool IsArray;
    public bool IsDynArray;
    public bool IsStaticArray;
    public bool IsFloat;
    public bool IsObjectPointer;
    public int Size;
    public uint ElementCount;
    public Type1355? ElementType;
    public Type1355? Pointed;
    public string Descriptive = "";
}

public sealed class Var1355
{
    public int VariablesReference;
    public long? IndexedVariables;
    public string? Name;
    public string? EvaluateName;
    public string? Type;
    public string? Value;
}

public sealed class ArrayServices1355
{
    public Func<Type1355, Type1355?>? NewPointer;
    public Func<string, VmWord1355, Type1355?, Node1355?>? CreateNode;
    public Func<ulong, Type1355, VmWord1355>? GetVmValue;
}
