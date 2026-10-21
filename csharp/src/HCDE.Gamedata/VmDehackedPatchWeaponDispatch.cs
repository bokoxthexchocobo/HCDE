namespace HCDE.Gamedata;

/// <summary>Whole-function <c>PatchWeapon</c> dispatch from <c>d_dehacked.cpp</c> (2253–2437).</summary>
public static class VmDehackedPatchWeaponDispatch
{
    public const int MaxWeaponSlot = 9;
    public const int MaxAmmoTypeIndex = 12;
    public const int DefaultAmmoTypeIndex = 5;
    public const int ChainsawReadyStateNum = 67;

    public static bool IsWeaponSlotValid(int weaponNum, int weaponNameCount) =>
        weaponNum >= 0 && weaponNum < MaxWeaponSlot && weaponNum < weaponNameCount;

    public static bool ShouldMakeStateDefines(bool typeResolved, bool patchedStates) =>
        typeResolved && !patchedStates;

    public static bool LineEndsWithFrameSuffix(string line1) =>
        line1.Length > 6 && line1.EndsWith(" frame", StringComparison.OrdinalIgnoreCase);

    public static bool TryMapFrameLabel(string line1, int stateNum, IDictionary<string, int> labels)
    {
        if (!LineEndsWithFrameSuffix(line1)) return false;
        return VmDehackedPatchWeapon.TryMapWeaponFrameLabel(line1, stateNum, labels);
    }

    public static string? FrameLabelForPrefix(string line1)
    {
        if (!LineEndsWithFrameSuffix(line1)) return null;
        if (line1.StartsWith("Deselect", StringComparison.OrdinalIgnoreCase)) return "Select";
        if (line1.StartsWith("Select", StringComparison.OrdinalIgnoreCase)) return "Deselect";
        if (line1.StartsWith("Bobbing", StringComparison.OrdinalIgnoreCase)) return "Ready";
        if (line1.StartsWith("Shooting", StringComparison.OrdinalIgnoreCase)) return "Fire";
        if (line1.StartsWith("Firing", StringComparison.OrdinalIgnoreCase)) return "Flash";
        return null;
    }

    public static bool IsAmmoTypeKey(string line1) =>
        line1.Equals("Ammo type", StringComparison.OrdinalIgnoreCase);

    public static bool IsDecalKey(string line1) =>
        line1.Equals("Decal", StringComparison.OrdinalIgnoreCase);

    public static bool IsAmmoUseKey(string line1) =>
        line1.Equals("Ammo use", StringComparison.OrdinalIgnoreCase)
        || line1.Equals("Ammo per shot", StringComparison.OrdinalIgnoreCase);

    public static bool IsMinAmmoKey(string line1) =>
        line1.Equals("Min ammo", StringComparison.OrdinalIgnoreCase);

    public static bool IsMbf21BitsKey(string line1) =>
        line1.Equals("MBF21 Bits", StringComparison.OrdinalIgnoreCase);

    public static int NormalizeAmmoTypeIndex(int val, int ammoNameCount) =>
        VmDehackedPatchWeapon.ClampAmmoTypeIndex(val, ammoNameCount);

    public static bool DecalLookupSucceeded(string? decalName) => !string.IsNullOrWhiteSpace(decalName);

    public static uint ParseMbf21NumericToken(string token) =>
        uint.TryParse(token, out var v) ? v : 0u;

    public static bool TokenIsNumeric(string token) => uint.TryParse(token, out _);

    public static uint ApplyMbf21Token(uint acc, string token, IReadOnlyList<string> bitNames)
    {
        if (TokenIsNumeric(token)) return acc | ParseMbf21NumericToken(token);
        for (var i = 0; i < bitNames.Count; i++)
        {
            if (bitNames[i].Equals(token, StringComparison.OrdinalIgnoreCase))
                return acc | (1u << i);
        }

        return acc;
    }

    public static uint ParseMbf21Line(string line2, IReadOnlyList<string> bitNames)
    {
        var value = 0u;
        foreach (var raw in line2.Split([',', '|', '+', ' ', '\t', '\f', '\r'],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            value = ApplyMbf21Token(value, raw, bitNames);
        }

        return value;
    }

    public static bool ReadyStateIsChainsaw(int readyStateNum, int chainsawStateNum) =>
        readyStateNum == chainsawStateNum;

public static bool HasReadyState(int? readyStateNum) => readyStateNum.HasValue;

    public static int ResolveReadySound(int readyStateNum, int chainsawStateNum, int sawIdleSoundId) =>
        ReadyStateIsChainsaw(readyStateNum, chainsawStateNum) ? sawIdleSoundId : 0;

    public static bool LoopContinues(int getLineResult) => getLineResult == 1;

    public static bool WeaponTypeMissing(bool typeResolved) => !typeResolved;

    public static string FormatUnknownWeaponKey(string key, int weaponNum) => $"{key} (weapon {weaponNum})";

    public readonly record struct WeaponFrameLine(string Line1, int StateNum);

    public static bool TryApplyWeaponFrameLine(WeaponFrameLine ctx, IDictionary<string, int> labels, out int? readyStateNum)
    {
        readyStateNum = null;
        if (!TryMapFrameLabel(ctx.Line1, ctx.StateNum, labels)) return false;
        if (ctx.Line1.StartsWith("Bobbing", StringComparison.OrdinalIgnoreCase))
            readyStateNum = ctx.StateNum;
        return true;
    }
}
