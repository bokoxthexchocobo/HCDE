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
        foreach (var raw in line2.Split(new[] { ',', '|', '+', ' ', '\t', '\f', '\r' },
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

    public enum WeaponPatchLineKind
    {
        Unknown,
        Frame,
        AmmoType,
        Decal,
        AmmoUse,
        MinAmmo,
        Mbf21Bits,
    }

    public static WeaponPatchLineKind ClassifyWeaponPatchLine(string line1)
    {
        if (LineEndsWithFrameSuffix(line1))
            return WeaponPatchLineKind.Frame;
        if (IsAmmoTypeKey(line1))
            return WeaponPatchLineKind.AmmoType;
        if (IsDecalKey(line1))
            return WeaponPatchLineKind.Decal;
        if (IsAmmoUseKey(line1))
            return WeaponPatchLineKind.AmmoUse;
        if (IsMinAmmoKey(line1))
            return WeaponPatchLineKind.MinAmmo;
        if (IsMbf21BitsKey(line1))
            return WeaponPatchLineKind.Mbf21Bits;
        return WeaponPatchLineKind.Unknown;
    }

    public readonly record struct Mbf21WeaponBitEntry(int WeaponFlagValue);

    public static (int Kickback, int WeaponFlags, bool Changed) DecodeMbf21WeaponBits(
        uint value,
        IReadOnlyList<Mbf21WeaponBitEntry> bits)
    {
        if (value == 0)
            return (100, 0, false);
        var kickback = 100;
        var flags = 0;
        for (var i = 0; i < bits.Count; i++)
        {
            if ((value & (1u << i)) == 0)
                continue;
            var bitVal = bits[i].WeaponFlagValue;
            if (bitVal < 0)
                kickback = 0;
            else
                flags |= bitVal;
        }

        return (kickback, flags, true);
    }

    public sealed class WeaponPatchSession
    {
        public bool PatchedStates { get; set; }
        public int? ReadyStateNum { get; set; }
        public int? AmmoTypeIndex { get; set; }
        public int AmmoUse { get; set; }
        public int MinSelAmmo { get; set; }
        public int Kickback { get; set; } = 100;
        public int WeaponFlags { get; set; }
        public string? DecalName { get; set; }
    }

    public static bool ShouldInstallWeaponStates(bool typeResolved, bool patchedStates) =>
        typeResolved && patchedStates;

    public static int DefaultAmmoUseWhenMissing(int currentAmmoUse) =>
        currentAmmoUse == 0 ? 1 : currentAmmoUse;

    public static void ApplyAmmoTypeLine(WeaponPatchSession session, int normalizedIndex, bool ammoTypeResolved)
    {
        session.AmmoTypeIndex = normalizedIndex;
        if (ammoTypeResolved)
            session.AmmoUse = DefaultAmmoUseWhenMissing(session.AmmoUse);
    }

    public static void ApplyAmmoUseLine(WeaponPatchSession session, int val) => session.AmmoUse = val;

    public static void ApplyMinAmmoLine(WeaponPatchSession session, int val) => session.MinSelAmmo = val;

    public static void ApplyDecalLine(WeaponPatchSession session, string? decalName)
    {
        if (DecalLookupSucceeded(decalName))
            session.DecalName = decalName;
    }

    public static void ApplyMbf21BitsLine(
        WeaponPatchSession session,
        string line2,
        IReadOnlyList<string> bitNames,
        IReadOnlyList<Mbf21WeaponBitEntry> bitEntries)
    {
        var value = ParseMbf21Line(line2, bitNames);
        var decoded = DecodeMbf21WeaponBits(value, bitEntries);
        if (!decoded.Changed)
            return;
        session.Kickback = decoded.Kickback;
        session.WeaponFlags = decoded.WeaponFlags;
    }

    public static bool ProcessWeaponPatchLine(
        string line1,
        string line2,
        int intVal,
        bool typeResolved,
        WeaponPatchSession session,
        IDictionary<string, int> labels,
        IReadOnlyList<string> mbf21BitNames,
        IReadOnlyList<Mbf21WeaponBitEntry> mbf21BitEntries,
        out bool unknownKey)
    {
        unknownKey = false;
        var kind = ClassifyWeaponPatchLine(line1);
        switch (kind)
        {
            case WeaponPatchLineKind.Frame:
                if (typeResolved && !session.PatchedStates)
                    session.PatchedStates = true;
                if (TryApplyWeaponFrameLine(new WeaponFrameLine(line1, intVal), labels, out var ready))
                {
                    if (ready.HasValue)
                        session.ReadyStateNum = ready;
                }
                return true;
            case WeaponPatchLineKind.AmmoType:
                if (typeResolved)
                    ApplyAmmoTypeLine(session, NormalizeAmmoTypeIndex(intVal, MaxAmmoTypeIndex), true);
                return true;
            case WeaponPatchLineKind.Decal:
                ApplyDecalLine(session, line2.Trim());
                return true;
            case WeaponPatchLineKind.AmmoUse:
                if (typeResolved)
                    ApplyAmmoUseLine(session, intVal);
                return true;
            case WeaponPatchLineKind.MinAmmo:
                if (typeResolved)
                    ApplyMinAmmoLine(session, intVal);
                return true;
            case WeaponPatchLineKind.Mbf21Bits:
                ApplyMbf21BitsLine(session, line2, mbf21BitNames, mbf21BitEntries);
                return true;
            default:
                unknownKey = true;
                return false;
        }
    }

    public readonly record struct PatchWeaponFinalizeInput(
        int ReadyStateNum,
        int ChainsawStateNum,
        int SawIdleSoundId,
        bool AmmoTypeSet,
        int CurrentAmmoUse);

    public static (int ReadySound, int AmmoUse, bool InstallStates) FinalizePatchWeapon(
        WeaponPatchSession session,
        bool typeResolved,
        PatchWeaponFinalizeInput input)
    {
        var readySound = 0;
        if (HasReadyState(session.ReadyStateNum))
            readySound = ResolveReadySound(session.ReadyStateNum!.Value, input.ChainsawStateNum, input.SawIdleSoundId);

        var ammoUse = session.AmmoUse;
        if (ClearAmmoUseWhenNoType(input.AmmoTypeSet))
            ammoUse = 0;
        else if (ammoUse == 0)
            ammoUse = DefaultAmmoUseWhenMissing(ammoUse);

        var install = ShouldInstallWeaponStates(typeResolved, session.PatchedStates);
        return (readySound, ammoUse, install);
    }
}
