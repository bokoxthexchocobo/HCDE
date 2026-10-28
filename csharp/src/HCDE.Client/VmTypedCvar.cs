namespace HCDE.Client;

/// <summary>Managed <c>FBaseCVar</c> subset with latch support.</summary>
public abstract class VmTypedCvar
{
    protected VmTypedCvar(string name, uint flags)
    {
        Name = name;
        Flags = flags;
    }

    public string Name { get; }
    public uint Flags { get; set; }

    public abstract VmCvarValueType RealType { get; }

    public abstract VmCvarValue GetFavoriteRep();
    public abstract VmCvarValue GetFavoriteRepDefault();
    public abstract void ForceSet(VmCvarValue value, VmCvarValueType type);

    public void SetGenericRep(VmCvarValue value, VmCvarValueType type, VmCvarLatchBuffer latch, Func<bool>? mustLatch = null)
    {
        if ((Flags & VmCvarFlags.Latch) != 0 && mustLatch?.Invoke() == true)
        {
            latch.Push(this, value, type, (Flags & VmCvarFlags.UnsafeContext) != 0);
            Flags &= ~VmCvarFlags.UnsafeContext;
            return;
        }

        ForceSet(value, type);
        Flags &= ~(VmCvarFlags.IsDefault | VmCvarFlags.UnsafeContext);
    }
}

public sealed class VmBoolCvar : VmTypedCvar
{
    public bool Value { get; private set; }
    public bool DefaultValue { get; private set; }

    public VmBoolCvar(string name, bool def, uint flags) : base(name, flags)
    {
        DefaultValue = def;
        Value = (flags & VmCvarFlags.IsDefault) != 0 ? def : false;
    }

    public override VmCvarValueType RealType => VmCvarValueType.Bool;
    public override VmCvarValue GetFavoriteRep() => VmCvarValue.FromBool(Value);
    public override VmCvarValue GetFavoriteRepDefault() => VmCvarValue.FromBool(DefaultValue);

    public override void ForceSet(VmCvarValue value, VmCvarValueType type)
    {
        Value = type switch
        {
            VmCvarValueType.Bool => value.Bool,
            VmCvarValueType.Int => value.Int != 0,
            VmCvarValueType.String => bool.TryParse(value.String, out var b) && b,
            _ => Value,
        };
    }
}

public sealed class VmIntCvar : VmTypedCvar
{
    public int Value { get; private set; }
    public int DefaultValue { get; private set; }

    public VmIntCvar(string name, int def, uint flags) : base(name, flags)
    {
        DefaultValue = def;
        Value = (flags & VmCvarFlags.IsDefault) != 0 ? def : 0;
    }

    public override VmCvarValueType RealType => VmCvarValueType.Int;
    public override VmCvarValue GetFavoriteRep() => VmCvarValue.FromInt(Value);
    public override VmCvarValue GetFavoriteRepDefault() => VmCvarValue.FromInt(DefaultValue);

    public override void ForceSet(VmCvarValue value, VmCvarValueType type)
    {
        Value = type switch
        {
            VmCvarValueType.Int => value.Int,
            VmCvarValueType.Bool => value.Bool ? 1 : 0,
            VmCvarValueType.String => int.TryParse(value.String, out var n) ? n : Value,
            _ => Value,
        };
    }
}

public sealed class VmFloatCvar : VmTypedCvar
{
    public double Value { get; private set; }
    public double DefaultValue { get; private set; }

    public VmFloatCvar(string name, double def, uint flags) : base(name, flags)
    {
        DefaultValue = def;
        Value = (flags & VmCvarFlags.IsDefault) != 0 ? def : 0;
    }

    public override VmCvarValueType RealType => VmCvarValueType.Float;
    public override VmCvarValue GetFavoriteRep() => VmCvarValue.FromFloat(Value);
    public override VmCvarValue GetFavoriteRepDefault() => VmCvarValue.FromFloat(DefaultValue);

    public override void ForceSet(VmCvarValue value, VmCvarValueType type)
    {
        Value = type switch
        {
            VmCvarValueType.Float => value.Float,
            VmCvarValueType.Int => value.Int,
            VmCvarValueType.String => double.TryParse(value.String, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : Value,
            _ => Value,
        };
    }
}

public sealed class VmStringCvar : VmTypedCvar
{
    public string Value { get; private set; }
    public string DefaultValue { get; private set; }

    public VmStringCvar(string name, string? def, uint flags) : base(name, flags)
    {
        DefaultValue = def ?? "";
        Value = (flags & VmCvarFlags.IsDefault) != 0 ? DefaultValue : "";
    }

    public override VmCvarValueType RealType => VmCvarValueType.String;
    public override VmCvarValue GetFavoriteRep() => VmCvarValue.FromString(Value);
    public override VmCvarValue GetFavoriteRepDefault() => VmCvarValue.FromString(DefaultValue);

    public override void ForceSet(VmCvarValue value, VmCvarValueType type)
    {
        Value = type switch
        {
            VmCvarValueType.String => value.String ?? "",
            VmCvarValueType.Int => value.Int.ToString(),
            VmCvarValueType.Float => value.Float.ToString(System.Globalization.CultureInfo.InvariantCulture),
            VmCvarValueType.Bool => value.Bool ? "true" : "false",
            _ => Value,
        };
    }
}
