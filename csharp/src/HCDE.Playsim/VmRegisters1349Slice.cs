namespace HCDE.Playsim;

/// <summary>
/// Debugger register scope from <c>RegistersScopeStateNode.cpp</c> (1-283).
/// Chunk 1349. Child names are appended. The index parse is <c>std::stoi</c>
/// on the suffix after the prefix length. Memory decoding stays injected.
/// </summary>
public class VmRegisters1349Slice
{
    public const string Params = "Params";
    public const string Ints = "Ints";
    public const string Floats = "Floats";
    public const string Strings = "Strings";
    public const string Pointers = "Pointers";
    public const string SpecialSetup = "SpecialInits";
    public const string TypeVoidPtr = "void*";
    public const string TypeString = "string";
    public const string TypeFloat64 = "float64";
    public const string TypeSInt32 = "sint32";

    public Frame1349? Frame;
    public int Id;

    public VmRegisters1349Slice(Frame1349? frame) => Frame = frame;

    public int GetId() => Id;

    public bool SerializeScope(Scope1349 scope)
    {
        scope.Name = "Registers";
        scope.Expensive = false;
        scope.PresentationHint = "registers";
        scope.VariablesReference = GetId();
        var names = new List<string>();
        GetChildNames(names);
        scope.NamedVariables = names.Count;
        scope.IndexedVariables = 0;
        return true;
    }

    public bool GetChildNames(List<string> names)
    {
        names.Add(Params);
        names.Add(Ints);
        names.Add(Floats);
        names.Add(Strings);
        names.Add(Pointers);
        if (ScriptFunction(Frame!.Func) != null)
            names.Add(SpecialSetup);
        return true;
    }

    public bool GetChildNode(string name, ref Node1349? node)
    {
        if (SameName(name, Params))
        {
            node = new ParamsNode1349(name, Frame!);
            return true;
        }
        if (SameName(name, Ints))
        {
            node = new IntNode1349(name, Frame!);
            return true;
        }
        if (SameName(name, Floats))
        {
            node = new FloatNode1349(name, Frame!);
            return true;
        }
        if (SameName(name, Strings))
        {
            node = new StringNode1349(name, Frame!);
            return true;
        }
        if (SameName(name, Pointers))
        {
            node = new PointerNode1349(name, Frame!);
            return true;
        }
        if (SameName(name, SpecialSetup))
        {
            node = new SpecialNode1349(name, Frame!);
            return true;
        }
        return false;
    }

    public static Script1349? ScriptFunction(Script1349? func)
    {
        if (func == null || func.Native)
            return null;
        return func;
    }

    public static bool SameName(string left, string right)
    {
        if (left.Length != right.Length)
            return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (Fold(left[i]) != Fold(right[i]))
                return false;
        }
        return true;
    }

    public static int Stoi(string text)
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
        long value = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            value = value * 10 + (text[i] - '0');
            if (sign > 0 && value > int.MaxValue)
                throw new OverflowException("stoi");
            if (sign < 0 && value > (long)int.MaxValue + 1)
                throw new OverflowException("stoi");
            i++;
        }
        if (sign < 0)
            return (int)-value;
        return (int)value;
    }

    static int Fold(char c)
    {
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 'a';
        return c;
    }

    static bool IsCSpace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
}

public class Node1349
{
    public int Id;
    public int GetId() => Id;
}

public sealed class Scope1349
{
    public string Name = "";
    public bool Expensive;
    public string PresentationHint = "";
    public int VariablesReference;
    public int NamedVariables;
    public int IndexedVariables;
}

public sealed class Var1349
{
    public string Name = "";
    public string Type = "";
    public string Value = "";
    public int NamedVariables;
    public int IndexedVariables;
    public int VariablesReference;
}

public struct VmVal1349
{
    public string Kind;
    public long Bits;
    public double Float;
    public object? Ref;
    public string TypeName;
}

public sealed class Slot1349
{
    public int Index;
    public string Text = "";
}

public sealed class Script1349
{
    public bool Native;
    public List<(string Type, int Offset)> SpecialInits = new();
}

public sealed class Frame1349
{
    public Script1349? Func;
    public int NumRegD;
    public int NumRegF;
    public int NumRegS;
    public int NumRegA;
    public int MaxParam;
    public int NumParam;
    public int[] Ints = System.Array.Empty<int>();
    public double[] Floats = System.Array.Empty<double>();
    public Slot1349[] Strings = System.Array.Empty<Slot1349>();
    public object?[] Pointers = System.Array.Empty<object?>();
    public VmVal1349[] Params = System.Array.Empty<VmVal1349>();
    public long ExtraBase;
    public Func<string, VmVal1349, string, Node1349?>? Create;
    public Func<long, string, VmVal1349>? ReadMemory;
}

public abstract class RegNode1349 : Node1349
{
    public Frame1349 Frame;
    public string Name;

    public RegNode1349(string name, Frame1349 frame)
    {
        Name = name;
        Frame = frame;
    }

    public abstract string GetPrefix();

    public abstract int GetNumberOfRegisters();

    public abstract VmVal1349 GetRegisterValue(int index);

    public abstract string GetRegisterType(int index);

    public virtual bool Serialize(Var1349 variable)
    {
        variable.Name = Name;
        variable.Type = Name + " Registers";
        int max = GetNumberOfRegisters();
        variable.Value = Name + "[" + max.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
        variable.NamedVariables = max;
        variable.VariablesReference = GetId();
        return true;
    }

    public virtual bool GetChildNames(List<string> names)
    {
        string prefix = GetPrefix();
        int count = GetNumberOfRegisters();
        for (int i = 0; i < count; i++)
            names.Add(prefix + i.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return true;
    }

    public virtual bool GetChildNode(string name, ref Node1349? node) => ParseRegister(name, ref node);

    protected bool ParseRegister(string name, ref Node1349? node)
    {
        string prefix = GetPrefix();
        string rest = name.Substring(prefix.Length);
        int index = VmRegisters1349Slice.Stoi(rest);
        if (index < 0 || index >= GetNumberOfRegisters())
            return false;
        VmVal1349 val = GetRegisterValue(index);
        if (Frame.Create == null)
            throw new InvalidOperationException("CreateNodeForVariable");
        node = Frame.Create(name, val, GetRegisterType(index));
        return true;
    }
}

public sealed class PointerNode1349 : RegNode1349
{
    public PointerNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "a";
    public override int GetNumberOfRegisters() => Frame.NumRegA;
    public override VmVal1349 GetRegisterValue(int index) => new() { Kind = "ptr", Ref = Frame.Pointers[index] };
    public override string GetRegisterType(int index) => VmRegisters1349Slice.TypeVoidPtr;
    public override bool GetChildNode(string name, ref Node1349? node) => ParseRegister(name, ref node);
}

public sealed class StringNode1349 : RegNode1349
{
    public StringNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "s";
    public override int GetNumberOfRegisters() => Frame.NumRegS;
    public override VmVal1349 GetRegisterValue(int index) => new() { Kind = "string-addr", Ref = Frame.Strings[index], Bits = index };
    public override string GetRegisterType(int index) => VmRegisters1349Slice.TypeString;
}

public sealed class FloatNode1349 : RegNode1349
{
    public FloatNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "f";
    public override int GetNumberOfRegisters() => Frame.NumRegF;
    public override VmVal1349 GetRegisterValue(int index) => new() { Kind = "float", Float = Frame.Floats[index] };
    public override string GetRegisterType(int index) => VmRegisters1349Slice.TypeFloat64;
}

public sealed class IntNode1349 : RegNode1349
{
    public IntNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "d";
    public override int GetNumberOfRegisters() => Frame.NumRegD;
    public override VmVal1349 GetRegisterValue(int index) => new() { Kind = "int", Bits = Frame.Ints[index] };
    public override string GetRegisterType(int index) => VmRegisters1349Slice.TypeSInt32;
}

public sealed class ParamsNode1349 : RegNode1349
{
    public ParamsNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "";
    public override int GetNumberOfRegisters() => Frame.MaxParam;
    public override VmVal1349 GetRegisterValue(int index) => Frame.Params[index];
    public override string GetRegisterType(int index) => VmRegisters1349Slice.TypeVoidPtr;

    public override bool Serialize(Var1349 variable)
    {
        variable.Name = VmRegisters1349Slice.Params;
        variable.Type = "Parameter Registers";
        int max = GetNumberOfRegisters();
        variable.Value = "Params - Max: " + max.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + ", In Use: " + Frame.NumParam.ToString(System.Globalization.CultureInfo.InvariantCulture);
        variable.IndexedVariables = max;
        variable.VariablesReference = GetId();
        return true;
    }
}

public sealed class SpecialNode1349 : RegNode1349
{
    public SpecialNode1349(string name, Frame1349 frame) : base(name, frame) => Name = name;
    public override string GetPrefix() => "";

    public override int GetNumberOfRegisters()
    {
        Script1349 fun = VmRegisters1349Slice.ScriptFunction(Frame.Func) ?? throw new NullReferenceException("script function");
        return fun.SpecialInits.Count;
    }

    public override VmVal1349 GetRegisterValue(int index)
    {
        Script1349 fun = VmRegisters1349Slice.ScriptFunction(Frame.Func) ?? throw new NullReferenceException("script function");
        var tao = fun.SpecialInits[index];
        if (Frame.ReadMemory == null)
            throw new InvalidOperationException("GetVMValue");
        return Frame.ReadMemory(Frame.ExtraBase + tao.Offset, tao.Type);
    }

    public override string GetRegisterType(int index)
    {
        Script1349 fun = VmRegisters1349Slice.ScriptFunction(Frame.Func) ?? throw new NullReferenceException("script function");
        return fun.SpecialInits[index].Type;
    }

    public override bool Serialize(Var1349 variable)
    {
        variable.Name = VmRegisters1349Slice.SpecialSetup;
        variable.Type = "Special Setup Registers";
        int max = GetNumberOfRegisters();
        variable.Value = "Special Setup - Max: " + max.ToString(System.Globalization.CultureInfo.InvariantCulture);
        variable.IndexedVariables = max;
        variable.VariablesReference = GetId();
        return true;
    }
}
