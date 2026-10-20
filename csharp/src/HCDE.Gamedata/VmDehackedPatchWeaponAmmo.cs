namespace HCDE.Gamedata;

public sealed class VmDehackedWeaponPatchTarget
{
    public int AmmoUse { get; set; }
    public int MinSelAmmo { get; set; }
    public int Kickback { get; set; } = 100;
    public int WeaponFlags { get; set; }
    public int ReadySound { get; set; }
    public string? AmmoTypeName { get; set; }
    public int AmmoGive { get; set; }
    public string? DecalName { get; set; }
    public bool NeedsPostProcess { get; set; }
}

/// <summary><c>PatchWeapon</c> ammo/decal/MBF21 tail from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchWeaponAmmo
{
    public static int ResolveAmmoGive(int defaultAmount) => defaultAmount * 2;

    public static void ApplyAmmoType(VmDehackedWeaponPatchTarget target, string ammoType, int defaultAmount)
    {
        target.AmmoTypeName = ammoType;
        target.AmmoGive = ResolveAmmoGive(defaultAmount);
        if (target.AmmoUse == 0) target.AmmoUse = 1;
    }

    public static void ApplyAmmoUse(VmDehackedWeaponPatchTarget target, int value)
    {
        target.AmmoUse = value;
        target.NeedsPostProcess = true;
    }

    public static void ApplyMbf21Bits(uint value, IReadOnlyList<(string Name, int FlagValue)> bits, VmDehackedWeaponPatchTarget target)
    {
        var kickback = 100;
        var flags = 0;
        for (var i = 0; i < bits.Count; i++)
        {
            if ((value & (1u << i)) == 0) continue;
            if (bits[i].FlagValue == -1) kickback = 0;
            else flags |= bits[i].FlagValue;
        }

        target.Kickback = kickback;
        target.WeaponFlags = flags;
    }

    public static void ApplyReadySound(VmDehackedWeaponPatchTarget target, bool readyIsChainsawIdle, int sawIdleSoundId)
    {
        target.ReadySound = readyIsChainsawIdle ? sawIdleSoundId : 0;
    }

    public static void FinalizeAmmo(VmDehackedWeaponPatchTarget target)
    {
        if (target.AmmoTypeName is null) target.AmmoUse = 0;
    }
}
