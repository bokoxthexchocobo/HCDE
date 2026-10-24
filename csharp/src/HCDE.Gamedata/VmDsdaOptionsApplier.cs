namespace HCDE.Gamedata;

public sealed class VmWadLevelInfoPatch
{
    public uint Flags2 { get; set; }
    public uint Flags3 { get; set; }
    public uint CompatFlags { get; set; }
    public uint CompatFlags2 { get; set; }
    public uint CompatMask { get; set; }
    public uint CompatMask2 { get; set; }
}

/// <summary><c>parseOptions</c> level flag application from <c>options.cpp</c>.</summary>
public static class VmDsdaOptionsApplier
{
    private const uint Level2NoInfighting = 0x00004000;
    private const uint Level3AvoidMelee = 0x00020000;
    private const uint Level3NoJumpDown = 0x00040000;
    private const uint CompatDropoff = 1u << 20;
    private const uint CompatVileGhosts = 1u << 25;
    private const uint CompatMbfMonsterMove = 1u << 24;
    private const uint CompatLimitPain = 1u << 2;
    private const uint CompatNoDoorLight = 1u << 9;
    private const uint CompatLight = 1u << 29;
    private const uint CompatShortTex = 1u << 0;
    private const uint CompatStairIndex = 1u << 1;
    private const uint CompatCrossDropoff = 1u << 20;
    private const uint Compat2AvoidHazards = 1u << 12;
    private const uint Compat2FloorMove = 1u << 1;
    private const uint Compat2StayOnLift = 1u << 13;
    private const uint Compat2NoFriendlySpawn = 1u << 21;
    private const uint Compat2ReservedLineFlag = 1u << 20;

    public static void ApplyToLevel(VmWadLevelInfoPatch level, VmDsdaOptions opt)
    {
        var flags2 = level.Flags2;
        var flags3 = level.Flags3;
        var compat = level.CompatFlags;
        var compat2 = level.CompatFlags2;
        var compatMask = level.CompatMask;
        var compatMask2 = level.CompatMask2;

        SetFlag(ref flags2, Level2NoInfighting, opt.MonsterInfighting);
        SetFlag(ref flags3, Level3AvoidMelee, opt.MonsterBacking);
        SetMaskedFlag(ref compat2, ref compatMask2, Compat2AvoidHazards, opt.MonsterAvoidHazards);
        SetMaskedFlag(ref compat, ref compatMask, CompatMbfMonsterMove, opt.MonsterFriction);
        SetMaskedFlag(ref compat, ref compatMask, CompatDropoff, opt.CompDropoff);
        SetMaskedFlag(ref compat, ref compatMask, CompatVileGhosts, opt.CompVile);
        SetFlag(ref flags3, Level3NoJumpDown, opt.DogJumping < 0 ? -1 : opt.DogJumping == 0 ? 1 : 0);
        SetMaskedFlag(ref compat, ref compatMask, CompatLimitPain, opt.CompPain);
        SetMaskedFlag(ref compat, ref compatMask, CompatNoDoorLight, opt.CompDoorlight);
        SetMaskedFlag(ref compat, ref compatMask, CompatLight | CompatShortTex, opt.CompModel);
        SetMaskedFlag(ref compat2, ref compatMask2, Compat2FloorMove, opt.CompFloors);
        SetMaskedFlag(ref compat2, ref compatMask2, Compat2StayOnLift, opt.CompStaylift);
        SetMaskedFlag(ref compat, ref compatMask, CompatStairIndex, opt.CompStairs);
        SetMaskedFlag(ref compat, ref compatMask, CompatCrossDropoff, opt.CompLedgeblock);
        SetMaskedFlag(ref compat2, ref compatMask2, Compat2NoFriendlySpawn,
            opt.CompFriendlyspawn < 0 ? -1 : opt.CompFriendlyspawn == 0 ? 1 : 0);
        SetMaskedFlag(ref compat2, ref compatMask2, Compat2ReservedLineFlag, opt.CompReservedLineflag);

        level.Flags2 = flags2;
        level.Flags3 = flags3;
        level.CompatFlags = compat;
        level.CompatFlags2 = compat2;
        level.CompatMask = compatMask;
        level.CompatMask2 = compatMask2;
    }

    private static void SetFlag(ref uint flag, uint mask, int set)
    {
        if (set < 0) return;
        if (set != 0) flag |= mask;
        else flag &= ~mask;
    }

    private static void SetMaskedFlag(ref uint flag, ref uint flagMask, uint mask, int set)
    {
        if (set < 0) return;
        if (set != 0) flag |= mask;
        else flag &= ~mask;
        flagMask |= mask;
    }
}
