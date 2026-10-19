using System.Globalization;

namespace HCDE.Gamedata;

/// <summary>Subset of <c>ParseFlagExpressionString</c> from <c>thingdef_parse.cpp</c>.</summary>
public static class DecorateFlagExpression
{
    public static bool TryParse(string text, IReadOnlyDictionary<string, int> namedFlags, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmed = text.Trim();
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        var inner = trimmed;
        var wrapped = false;
        if (inner.StartsWith('(') && inner.EndsWith(')'))
        {
            wrapped = true;
            inner = inner[1..^1].Trim();
        }

        if (inner.Length == 0)
            return false;

        var parts = inner.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        var combined = 0;
        foreach (var part in parts)
        {
            if (!namedFlags.TryGetValue(part, out var bit))
                return false;
            combined |= bit;
        }

        value = combined;
        return wrapped || parts.Length >= 1;
    }

    public static IReadOnlyDictionary<string, int> MorphStyleFlags => MorphStyles;

    public static IReadOnlyDictionary<string, int> ThingActivationFlags => ThingActivations;

    private static readonly Dictionary<string, int> MorphStyles = new(StringComparer.Ordinal)
    {
        ["MRF_ADDSTAMINA"] = 1 << 0,
        ["MRF_FULLHEALTH"] = 1 << 1,
        ["MRF_UNDOBYTOMEOFPOWER"] = 1 << 2,
        ["MRF_UNDOBYCHAOSDEVICE"] = 1 << 3,
        ["MRF_FAILNOTELEFRAG"] = 1 << 4,
        ["MRF_FAILNOLAUGH"] = 1 << 5,
        ["MRF_WHENINVULNERABLE"] = 1 << 6,
        ["MRF_LOSEACTUALWEAPON"] = 1 << 7,
        ["MRF_NEWTIDBEHAVIOUR"] = 1 << 8,
        ["MRF_UNDOBYDEATH"] = 1 << 9,
        ["MRF_UNDOBYDEATHFORCED"] = 1 << 10,
        ["MRF_UNDOBYDEATHSAVES"] = 1 << 11,
        ["MRF_UNDOALWAYS"] = 1 << 12,
        ["MRF_TRANSFERTRANSLATION"] = 1 << 13,
    };

    private static readonly Dictionary<string, int> ThingActivations = new(StringComparer.Ordinal)
    {
        ["THINGSPEC_Default"] = 0,
        ["THINGSPEC_ThingActs"] = 1 << 0,
        ["THINGSPEC_ThingTargets"] = 1 << 1,
        ["THINGSPEC_TriggerTargets"] = 1 << 2,
        ["THINGSPEC_MonsterTrigger"] = 1 << 3,
        ["THINGSPEC_MissileTrigger"] = 1 << 4,
        ["THINGSPEC_ClearSpecial"] = 1 << 5,
        ["THINGSPEC_NoDeathSpecial"] = 1 << 6,
        ["THINGSPEC_TriggerActs"] = 1 << 7,
        ["THINGSPEC_Activate"] = 1 << 8,
        ["THINGSPEC_Deactivate"] = 1 << 9,
        ["THINGSPEC_Switch"] = 1 << 10,
    };
}
