using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Debugger state pointer from <c>StatePointerNode.cpp</c> (1-281).
/// Chunk 1350. Child names are appended. Owner lookup and the state-name
/// string stay injected. The header child map is never read by this file.
/// </summary>
public sealed class VmStatePointer1350 : Node1350
{
    public const ushort StfSlow = 1;
    public const ushort StfFast = 2;
    public const ushort StfFullbright = 4;
    public const ushort StfNoDelay = 8;
    public const ushort StfSameFrame = 16;
    public const ushort StfCanRaise = 32;
    public const ushort StfDehacked = 64;
    public const ushort StfConsumeAmmo = 128;

    public const byte SdfNext = 0;
    public const byte SdfState = 1;
    public const byte SdfStop = 2;
    public const byte SdfWait = 3;
    public const byte SdfLabel = 4;
    public const byte SdfIndex = 5;
    public const byte SdfMask = 7;

    public const string TypeSpriteId = "TypeSpriteID";
    public const string TypeSInt16 = "TypeSInt16";
    public const string TypeUInt16 = "TypeUInt16";
    public const string TypeUInt8 = "TypeUInt8";
    public const string TypeSInt32 = "TypeSInt32";
    public const string NoneName = "None";

    public VmRef1350 Value;
    public object? OwningType;
    public StateQueries1350 Queries;

    public VmStatePointer1350(string name, VmRef1350 value, object? owningType, StateQueries1350 queries)
    {
        Name = name;
        EvalName = name;
        Value = value;
        OwningType = owningType;
        Queries = queries;
    }

    public int GetId() => Id;

    public static bool IsValid(VmRef1350? val) => val != null && val.Address != null && val.Low != 0;

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

    public bool Serialize(Var1350 variable)
    {
        variable.VariablesReference = IsValid(Value) ? GetId() : 0;
        variable.NamedVariables = 0;
        var names = new List<string>();
        GetChildNames(names);
        variable.NamedVariables = names.Count;
        variable.Name = Name;
        if (EvalName != Name)
            variable.EvaluateName = EvalName;
        variable.Type = "StatePointer";
        if (!IsValid(Value))
        {
            variable.Value = "<NULL>";
        }
        else
        {
            variable.Value = Describe((State1350)Value.Address!);
        }
        return true;
    }

    public bool GetChildNames(List<string> names)
    {
        names.Add("NextState");
        names.Add("sprite");
        names.Add("Tics");
        names.Add("TicRange");
        names.Add("Light");
        names.Add("StateFlags");
        names.Add("Frame");
        names.Add("UseFlags");
        names.Add("DefineFlags");
        names.Add("Misc1");
        names.Add("Misc2");
        names.Add("DehIndex");
        return true;
    }

    public bool GetChildNode(string name, ref Node1350? node)
    {
        if (!IsValid(Value))
            return false;
        State1350? state = Value.Address;
        if (state == null)
            return false;
        if (SameName(name, "NextState"))
        {
            node = new VmStatePointer1350("NextState", VmRef1350.FromState(state.Next), OwningType, Queries);
            return true;
        }
        if (SameName(name, "sprite"))
        {
            node = new ValueNode1350("sprite", state.Sprite, TypeSpriteId);
            return true;
        }
        else if (SameName(name, "Tics"))
        {
            node = new ValueNode1350("Tics", state.Tics, TypeSInt16);
            return true;
        }
        else if (SameName(name, "TicRange"))
        {
            node = new ValueNode1350("TicRange", state.TicRange, TypeUInt16);
            return true;
        }
        else if (SameName(name, "Light"))
        {
            node = new ValueNode1350("Light", state.Light, TypeSInt16);
            return true;
        }
        else if (SameName(name, "StateFlags"))
        {
            node = new DummyNode1350("StateFlags", FormatStateFlags(state.StateFlags), "StateFlags");
            return true;
        }
        else if (SameName(name, "Frame"))
        {
            node = new ValueNode1350("Frame", state.Frame, TypeUInt8);
            return true;
        }
        else if (SameName(name, "UseFlags"))
        {
            // The native join of SUF_* names is built and then discarded.
            // The value node stores DefineFlags with TypeUInt8.
            node = new ValueNode1350("UseFlags", state.DefineFlags, TypeUInt8);
            return true;
        }
        else if (SameName(name, "DefineFlags"))
        {
            node = new DummyNode1350("DefineFlags", FormatDefineFlags(state.DefineFlags), "DefineFlags");
            return true;
        }
        else if (SameName(name, "Misc1"))
        {
            node = new ValueNode1350("Misc1", state.Misc1, TypeSInt32);
            return true;
        }
        else if (SameName(name, "Misc2"))
        {
            node = new ValueNode1350("Misc2", state.Misc2, TypeSInt32);
            return true;
        }
        else if (SameName(name, "DehIndex"))
        {
            node = new ValueNode1350("DehIndex", state.DehIndex, TypeSInt32);
            return true;
        }
        return true;
    }

    public static void DumpStateHelper(StateLabelList1350 list, string prefix, StateQueries1350 queries)
    {
        for (int i = 0; i < list.NumLabels; i++)
        {
            StateLabel1350 slot = list.Items[i];
            State1350? state = slot.State;
            if (state != null)
            {
                StateOwner1350? owner = queries.StaticFindStateOwner(state);
                if (owner == null)
                {
                    if (state.DehIndex >= 0)
                        queries.Lines.Add(prefix + slot.Name + ": DehExtra " + state.DehIndex.ToString(CultureInfo.InvariantCulture) + "\n");
                    else
                        queries.Lines.Add(prefix + slot.Name + ": invalid\n");
                }
                else
                {
                    queries.Lines.Add(prefix + slot.Name + ": " + queries.StaticGetStateName(state) + "\n");
                }
            }
            if (slot.Children != null)
                DumpStateHelper(slot.Children, prefix + "." + slot.Name, queries);
        }
    }

    string Describe(State1350 state)
    {
        StateOwner1350? owner = Queries.StaticFindStateOwner(state);
        string label = NoneName;
        if (owner != null)
        {
            for (int i = 0; i < owner.GetStateLabels().NumLabels; i++)
            {
                StateLabel1350 slot = owner.GetStateLabels().Items[i];
                if (ReferenceEquals(slot.State, state))
                {
                    label = slot.Name;
                    break;
                }
            }
        }
        if (owner == null || label == NoneName)
            return Queries.StaticGetStateName(state);
        return owner.TypeName + "." + label;
    }

    static string FormatStateFlags(ushort flags)
    {
        var names = new List<string>();
        if ((flags & StfSlow) != 0)
            names.Add("STF_SLOW");
        if ((flags & StfFast) != 0)
            names.Add("STF_FAST");
        if ((flags & StfFullbright) != 0)
            names.Add("STF_FULLBRIGHT");
        if ((flags & StfNoDelay) != 0)
            names.Add("STF_NODELAY");
        if ((flags & StfSameFrame) != 0)
            names.Add("STF_SAMEFRAME");
        if ((flags & StfCanRaise) != 0)
            names.Add("STF_CANRAISE");
        if ((flags & StfDehacked) != 0)
            names.Add("STF_DEHACKED");
        if ((flags & StfConsumeAmmo) != 0)
            names.Add("STF_CONSUMEAMMO");
        return flags.ToString(CultureInfo.InvariantCulture) + " (" + StringJoin(names, " | ") + ")";
    }

    static string FormatDefineFlags(byte flags)
    {
        string value = flags.ToString(CultureInfo.InvariantCulture) + " ";
        value += flags switch
        {
            SdfNext => "SDF_NEXT",
            SdfState => "SDF_STATE",
            SdfStop => "SDF_STOP",
            SdfWait => "SDF_WAIT",
            SdfLabel => "SDF_LABEL",
            SdfIndex => "SDF_INDEX",
            SdfMask => "SDF_MASK",
            _ => "<INVALID>",
        };
        return value;
    }

    static string StringJoin(List<string> parts, string delim)
    {
        string imploded = "";
        foreach (string part in parts)
            imploded += part + delim;
        if (imploded.Length < delim.Length)
            return imploded;
        return imploded.Substring(0, imploded.Length - delim.Length);
    }

    static char Fold(char c) => c is >= 'A' and <= 'Z' ? (char)(c + 32) : c;
}

public class Node1350
{
    public string Name = "";
    public string EvalName = "";
    public int Id;
}

public sealed class VmRef1350
{
    public State1350? Address;
    public uint Low;

    public static VmRef1350 FromState(State1350? state)
    {
        if (state == null)
            return new VmRef1350();
        return new VmRef1350 { Address = state, Low = state.PointerLow };
    }
}

public sealed class State1350
{
    public State1350? Next;
    public uint PointerLow = 1;
    public int Sprite;
    public short Tics;
    public ushort TicRange;
    public short Light;
    public ushort StateFlags;
    public byte Frame;
    public byte UseFlags;
    public byte DefineFlags;
    public int Misc1;
    public int Misc2;
    public int DehIndex;
}

public sealed class ValueNode1350 : Node1350
{
    public int Bits;
    public string TypeToken;

    public ValueNode1350(string name, int bits, string typeToken)
    {
        Name = name;
        EvalName = name;
        Bits = bits;
        TypeToken = typeToken;
    }
}

public sealed class DummyNode1350 : Node1350
{
    public string Text;
    public string TypeToken;

    public DummyNode1350(string name, string text, string typeToken)
    {
        Name = name;
        EvalName = name;
        Text = text;
        TypeToken = typeToken;
    }
}

public sealed class Var1350
{
    public string Name = "";
    public string? EvaluateName;
    public string Type = "";
    public string Value = "";
    public int VariablesReference;
    public int NamedVariables;
    public int IndexedVariables;
}

public sealed class StateQueries1350
{
    public Func<State1350, StateOwner1350?>? FindOwner;
    public Func<State1350, string>? StateName;
    public List<string> Lines = new();

    public StateOwner1350? StaticFindStateOwner(State1350 state)
    {
        if (FindOwner == null)
            throw new InvalidOperationException("StaticFindStateOwner");
        return FindOwner(state);
    }

    public string StaticGetStateName(State1350 state)
    {
        if (StateName == null)
            throw new InvalidOperationException("StaticGetStateName");
        return StateName(state);
    }
}

public sealed class StateOwner1350
{
    public string TypeName = "";
    public StateLabelList1350 Labels = new();

    public StateLabelList1350 GetStateLabels() => Labels;
}

public sealed class StateLabelList1350
{
    public int NumLabels;
    public List<StateLabel1350> Items = new();
}

public sealed class StateLabel1350
{
    public string Name = "None";
    public State1350? State;
    public StateLabelList1350? Children;
}
