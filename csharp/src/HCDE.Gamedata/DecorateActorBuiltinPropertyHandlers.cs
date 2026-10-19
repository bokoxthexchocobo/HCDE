namespace HCDE.Gamedata;

/// <summary>Native <c>DEFINE_PROPERTY</c> handlers from <c>thingdef_properties.cpp</c> (scalar DECORATE properties).</summary>
public static class DecorateActorBuiltinPropertyHandlers
{
    private static readonly Dictionary<string, Func<DecorateActorDefinition, IReadOnlyList<DecoratePropertyValue>, string?>> Handlers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["tag"] = ApplyTag,
            ["defthreshold"] = ApplyDefThreshold,
            ["threshold"] = ApplyThreshold,
            ["scale"] = ApplyScale,
            ["floatbobphase"] = ApplyFloatBobPhase,
            ["floatbobstrength"] = ApplyFloatBobStrength,
            ["floatbobfactor"] = ApplyFloatBobFactor,
            ["args"] = ApplyArgs,
            ["renderstyle"] = ApplyRenderStyle,
            ["gravity"] = ApplyGravity,
            ["bouncetype"] = ApplyBounceType,
            ["bouncefactor"] = ApplyBounceFactor,
            ["wallbouncefactor"] = ApplyWallBounceFactor,
            ["damagetype"] = ApplyDamageType,
            ["paintype"] = ApplyPainType,
            ["deathtype"] = ApplyDeathType,
            ["damagefactor"] = ApplyDamageFactor,
            ["poisondamage"] = ApplyPoisonDamage,
            ["painchance"] = ApplyPainChanceTyped,
            ["dropitem"] = ApplyDropItem,
        };

    public static bool TryApply(string propertyName, DecorateActorDefinition definition, IReadOnlyList<DecoratePropertyValue> values, out string? error)
    {
        error = null;
        if (!Handlers.TryGetValue(propertyName, out var handler))
        {
            error = $"'{propertyName}' is not a builtin scalar property.";
            return false;
        }

        error = handler(definition, values);
        return error is null;
    }

    private static string? ApplyTag(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.Tag = values[0].StringValue ?? "";
        return null;
    }

    private static string? ApplyDefThreshold(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var id = values[0].IntValue;
        if (id < 0) return "DefThreshold cannot be negative.";
        d.DefThreshold = id;
        return null;
    }

    private static string? ApplyThreshold(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var id = values[0].IntValue;
        if (id < 0) return "Threshold cannot be negative.";
        d.Threshold = id;
        return null;
    }

    private static string? ApplyScale(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var scale = values[0].DoubleValue;
        d.Scale = scale;
        d.ScaleY = scale;
        return null;
    }

    private static string? ApplyFloatBobPhase(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var id = values[0].IntValue;
        if (id < -1 || id >= 64) return "FloatBobPhase must be in range [-1,63].";
        d.FloatBobPhase = id;
        return null;
    }

    private static string? ApplyFloatBobStrength(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.FloatBobStrength = values[0].DoubleValue;
        return null;
    }

    private static string? ApplyFloatBobFactor(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var factor = values[0].DoubleValue;
        if (factor <= 0) return "FloatBobFactor must be above 0.0.";
        d.FloatBobFactor = factor;
        return null;
    }

    private static string? ApplyArgs(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        for (var i = 0; i < Math.Min(5, values.Count); i++)
            d.Args[i] = values[i].IntValue;
        d.ArgsDefined = true;
        return null;
    }

    private static string? ApplyRenderStyle(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var str = values[0].StringValue ?? "Normal";
        if (str.StartsWith("style_", StringComparison.OrdinalIgnoreCase))
            str = str[6..];
        if (!DecorateRenderStyleCatalog.TryResolve(str, out var style))
            return $"Unknown render style '{str}'.";
        d.RenderStyle = style;
        return null;
    }

    private static string? ApplyGravity(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.Gravity = values[0].DoubleValue;
        return null;
    }

    private static string? ApplyBounceType(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var name = values[0].StringValue ?? "None";
        if (!DecorateBounceTypeCatalog.TryResolve(name, out var flags))
            return $"Unknown bouncetype {name}.";
        d.BounceType = flags;
        return null;
    }

    private static string? ApplyBounceFactor(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.BounceFactor = Clamp01(values[0].DoubleValue);
        return null;
    }

    private static string? ApplyWallBounceFactor(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.WallBounceFactor = Clamp01(values[0].DoubleValue);
        return null;
    }

    private static string? ApplyDamageType(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var str = values[0].StringValue ?? "Normal";
        d.DamageType = string.Equals(str, "Normal", StringComparison.OrdinalIgnoreCase) ? "" : str;
        return null;
    }

    private static string? ApplyPainType(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var str = values[0].StringValue ?? "Normal";
        d.PainType = string.Equals(str, "Normal", StringComparison.OrdinalIgnoreCase) ? "" : str;
        return null;
    }

    private static string? ApplyDeathType(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var str = values[0].StringValue ?? "Normal";
        d.DeathType = string.Equals(str, "Normal", StringComparison.OrdinalIgnoreCase) ? "" : str;
        return null;
    }

    private static string? ApplyDamageFactor(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        if (values.Count == 1)
        {
            d.DamageFactor = values[0].DoubleValue;
            return null;
        }

        var type = values[0].StringValue ?? "Normal";
        d.DamageFactorByType[string.Equals(type, "Normal", StringComparison.OrdinalIgnoreCase) ? "" : type] = values[1].DoubleValue;
        return null;
    }

    private static string? ApplyPoisonDamage(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        d.PoisonDamage = values[0].IntValue;
        if (values.Count == 1)
        {
            d.PoisonDuration = int.MinValue;
            return null;
        }

        d.PoisonDuration = values[1].IntValue;
        if (values.Count > 2)
            d.PoisonPeriod = values[2].IntValue;
        return null;
    }

    private static string? ApplyPainChanceTyped(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        if (values.Count == 1 || values[0].Kind == DecoratePropertyKind.Integer)
        {
            d.PainChance = values[^1].IntValue;
            return null;
        }

        var painType = values[0].StringValue ?? "Normal";
        var chance = values[1].IntValue;
        d.PainChanceByType[string.Equals(painType, "Normal", StringComparison.OrdinalIgnoreCase) ? "" : painType] = chance;
        return null;
    }

    private static string? ApplyDropItem(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values)
    {
        var entry = new DecorateDropItem
        {
            Name = values[0].StringValue ?? "",
            Probability = values.Count > 1 ? values[1].IntValue : 255,
            Amount = values.Count > 2 ? values[2].IntValue : -1,
        };
        d.DropItems.Insert(0, entry);
        return null;
    }

    private static double Clamp01(double value) => value < 0 ? 0 : value > 1 ? 1 : value;
}

public sealed class DecorateDropItem
{
    public string Name { get; set; } = "";
    public int Probability { get; set; } = 255;
    public int Amount { get; set; } = -1;
}

internal static class DecorateRenderStyleCatalog
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NONE"] = "None",
        ["NORMAL"] = "Normal",
        ["FUZZY"] = "Fuzzy",
        ["SOULTRANS"] = "SoulTrans",
        ["OPTFUZZY"] = "OptFuzzy",
        ["STENCIL"] = "Stencil",
        ["TRANSLUCENT"] = "Translucent",
        ["ADD"] = "Add",
        ["SHADED"] = "Shaded",
        ["SHADOW"] = "Shadow",
        ["SUBTRACT"] = "Subtract",
        ["ADDSTENCIL"] = "AddStencil",
        ["ADDSHADED"] = "AddShaded",
        ["COLORBLEND"] = "ColorBlend",
        ["COLORADD"] = "ColorAdd",
        ["MULTIPLY"] = "Multiply",
    };

    public static bool TryResolve(string name, out string style) => Map.TryGetValue(name, out style!);
}

internal static class DecorateBounceTypeCatalog
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["None"] = "None",
        ["Doom"] = "Doom",
        ["Heretic"] = "Heretic",
        ["Hexen"] = "Hexen",
        ["DoomCompat"] = "DoomCompat",
        ["HereticCompat"] = "HereticCompat",
        ["HexenCompat"] = "HexenCompat",
        ["Grenade"] = "Grenade",
        ["Classic"] = "Classic",
    };

    public static bool TryResolve(string name, out string flags) => Map.TryGetValue(name, out flags!);
}
