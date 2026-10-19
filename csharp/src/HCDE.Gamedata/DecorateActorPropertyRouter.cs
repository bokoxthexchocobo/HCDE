namespace HCDE.Gamedata;

/// <summary>Native <c>ParseActorProperty</c> name routing without ZScript symbols.</summary>
public static class DecorateActorPropertyRouter
{
    private static readonly string[] StateLabels =
    [
        "Spawn", "See", "Melee", "Missile", "Pain", "Death", "XDeath", "Burn",
        "Ice", "Raise", "Crash", "Crush", "Wound", "Disintegrate", "Heal",
    ];

    private static readonly Dictionary<string, DecorateBuiltinProperty> Builtin = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Health"] = new("I", ApplyHealth),
        ["Radius"] = new("F", ApplyRadius),
        ["Height"] = new("F", ApplyHeight),
        ["Speed"] = new("F", ApplySpeed),
        ["PainChance"] = new("I", ApplyPainChance),
        ["Mass"] = new("I", ApplyMass),
        ["Damage"] = new("I", ApplyDamage),
        ["Scale"] = new("F", ApplyScale),
        ["Gravity"] = new("F", ApplyGravity),
        ["Alpha"] = new("F", ApplyAlpha),
        ["RenderStyle"] = new("S", ApplyRenderStyle),
        ["Monster"] = new("0", (_, _) => { }),
        ["+SOLID"] = new("0", (d, _) => ModifyFlag(d, "SOLID", true)),
        ["-SOLID"] = new("0", (d, _) => ModifyFlag(d, "SOLID", false)),
        ["+SHOOTABLE"] = new("0", (d, _) => ModifyFlag(d, "SHOOTABLE", true)),
        ["-SHOOTABLE"] = new("0", (d, _) => ModifyFlag(d, "SHOOTABLE", false)),
        ["+COUNTITEM"] = new("0", (d, _) => ModifyFlag(d, "COUNTITEM", true)),
        ["-COUNTITEM"] = new("0", (d, _) => ModifyFlag(d, "COUNTITEM", false)),
    };

    public static bool TryApplyProperty(
        string propertyName,
        string actorClass,
        IReadOnlyList<string> tokens,
        DecorateActorDefinition definition,
        out string? error)
    {
        error = null;
        if (TryMatchStateLabel(propertyName, out var label))
        {
            definition.StateLabels[label] = tokens.Count == 0 ? "0" : string.Join(' ', tokens);
            return true;
        }

        if (string.Equals(propertyName, "translation", StringComparison.OrdinalIgnoreCase)
            && DecorateTranslationPropertyHandler.TryParsePropertyTokens(tokens, out var translationValues, out error))
        {
            return DecorateTranslationPropertyHandler.TryApply(translationValues, definition, out error);
        }

        if (DecorateActorBuiltinPropertyParamSpecs.TryGetSpec(propertyName, out var extendedSpec)
            && DecoratePropertyParamParser.TryParse(extendedSpec, tokens, out var extendedParsed, out error))
        {
            return DecorateActorBuiltinPropertyHandlers.TryApply(propertyName, definition, extendedParsed!.Values, out error);
        }

        if (!Builtin.TryGetValue(propertyName, out var builtin))
        {
            error = $"'{propertyName}' is an unknown actor property.";
            return false;
        }

        if (!string.IsNullOrEmpty(builtin.ParamSpec) && builtin.ParamSpec != "0")
        {
            if (!DecoratePropertyParamParser.TryParse(builtin.ParamSpec, tokens, out var parsed, out error))
                return false;
            builtin.Apply(definition, parsed!.Values);
            return true;
        }

        builtin.Apply(definition, []);
        return true;
    }

    private static bool TryMatchStateLabel(string name, out string label)
    {
        foreach (var state in StateLabels)
        {
            if (name.Equals(state, StringComparison.OrdinalIgnoreCase))
            {
                label = state;
                return true;
            }
        }

        label = "";
        return false;
    }

    private static void ModifyFlag(DecorateActorDefinition definition, string flag, bool set)
    {
        if (!DecorateActorFlagModifier.TryModify(definition.Flags, flag, set, out _))
            return;
    }

    private static void ApplyHealth(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Flags.Health = values[0].IntValue;

    private static void ApplyRadius(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Radius = values[0].DoubleValue;

    private static void ApplyHeight(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Height = values[0].DoubleValue;

    private static void ApplySpeed(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Speed = values[0].DoubleValue;

    private static void ApplyPainChance(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.PainChance = values[0].IntValue;

    private static void ApplyMass(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Mass = values[0].IntValue;

    private static void ApplyDamage(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Damage = values[0].IntValue;

    private static void ApplyScale(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Scale = values[0].DoubleValue;

    private static void ApplyGravity(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Gravity = values[0].DoubleValue;

    private static void ApplyAlpha(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.Alpha = values[0].DoubleValue;

    private static void ApplyRenderStyle(DecorateActorDefinition d, IReadOnlyList<DecoratePropertyValue> values) =>
        d.RenderStyle = values[0].StringValue ?? "Normal";

    private sealed record DecorateBuiltinProperty(string ParamSpec, Action<DecorateActorDefinition, IReadOnlyList<DecoratePropertyValue>> Apply);
}

public sealed class DecorateActorDefinition
{
    public string ActorClass { get; set; } = "Actor";
    public DecorateActorFlagState Flags { get; } = new();
    public Dictionary<string, string> StateLabels { get; } = new(StringComparer.OrdinalIgnoreCase);
    public double Radius { get; set; } = 20;
    public double Height { get; set; } = 16;
    public double Speed { get; set; }
    public int PainChance { get; set; } = 256;
    public int Mass { get; set; } = 100;
    public int Damage { get; set; }
    public double Scale { get; set; } = 1;
    public double Gravity { get; set; } = 1;
    public double Alpha { get; set; } = 1;
    public string RenderStyle { get; set; } = "Normal";
    public string Tag { get; set; } = "";
    public int DefThreshold { get; set; }
    public int Threshold { get; set; }
    public double ScaleY { get; set; } = 1;
    public int FloatBobPhase { get; set; }
    public double FloatBobStrength { get; set; }
    public double FloatBobFactor { get; set; } = 1;
    public int[] Args { get; } = new int[5];
    public bool ArgsDefined { get; set; }
    public string BounceType { get; set; } = "None";
    public double BounceFactor { get; set; }
    public double WallBounceFactor { get; set; }
    public string DamageType { get; set; } = "";
    public string PainType { get; set; } = "";
    public string DeathType { get; set; } = "";
    public double DamageFactor { get; set; } = 1;
    public Dictionary<string, double> DamageFactorByType { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> PainChanceByType { get; } = new(StringComparer.OrdinalIgnoreCase);
    public int PoisonDamage { get; set; }
    public int PoisonDuration { get; set; }
    public int PoisonPeriod { get; set; }
    public List<DecorateDropItem> DropItems { get; } = new();
    public int TranslationId { get; set; }
    public bool TranslationUsesCustomRanges { get; set; }
    public List<string> TranslationCustomRanges { get; } = new();
}

internal static class DecorateActorBuiltinPropertyParamSpecs
{
    private static readonly Dictionary<string, string> Specs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tag"] = "S",
        ["defthreshold"] = "I",
        ["threshold"] = "I",
        ["scale"] = "F",
        ["floatbobphase"] = "I",
        ["floatbobstrength"] = "F",
        ["floatbobfactor"] = "F",
        ["args"] = "Iiiii",
        ["renderstyle"] = "S",
        ["gravity"] = "F",
        ["bouncetype"] = "S",
        ["bouncefactor"] = "F",
        ["wallbouncefactor"] = "F",
        ["damagetype"] = "S",
        ["paintype"] = "S",
        ["deathtype"] = "S",
        ["damagefactor"] = "ZF",
        ["poisondamage"] = "Iii",
        ["painchance"] = "ZI",
        ["dropitem"] = "S_i_i",
    };

    public static bool TryGetSpec(string name, out string spec) => Specs.TryGetValue(name, out spec!);
}
