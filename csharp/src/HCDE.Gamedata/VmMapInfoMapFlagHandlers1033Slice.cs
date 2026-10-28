namespace HCDE.Gamedata;

/// <summary>
/// `MapFlagHandlers[]` continuation and `ParseMapDefinition` flag dispatch head from <c>g_mapinfo.cpp</c> (**1977–2126**).
/// Chunk **1033**; follows **1032** (**1827–1976**). Compat tail **2055–2106** excluded (off-tally `VmMapInfoFlagHandlersCompat`).
/// </summary>
public static class VmMapInfoMapFlagHandlers1033Slice
{
    public const uint FallDmgHx = 1u << 2;
    public const uint FallDmgZd = 1u << 3;
    public const uint FallDmgBoth = FallDmgHx | FallDmgZd;
    public const uint FreelookYes = 1u;
    public const uint FreelookNo = 1u << 1;
    public const uint JumpNo = 1u << 7;
    public const uint Noinfighting = 1u << 2;
    public const uint TotalInfighting = 1u << 3;
    public const uint TeamplayOn = 1u << 8;
    public const uint TeamplayOff = 1u << 9;
    public const uint NoShadowMap = 1u;
    public const uint CompatNoPassMobj = 1u << 16;

    public static void ApplyFreelookScFlags(ref uint flags, bool allowFreelook)
    {
        if (allowFreelook)
            VmMapInfoFlagApply.ApplyScFlags(ref flags, FreelookYes, ~FreelookNo);
        else
            VmMapInfoFlagApply.ApplyScFlags(ref flags, FreelookNo, ~FreelookYes);
    }

    public static void ApplyJumpToggle(ref uint flags, bool allowJump)
    {
        if (allowJump)
            flags &= ~JumpNo;
        else
            flags |= JumpNo;
    }

    public static void ApplyFallingDamageHx(ref uint flags) =>
        VmMapInfoFlagApply.ApplyScFlags(ref flags, FallDmgHx, ~FallDmgZd);

    public static void ApplyFallingDamageZd(ref uint flags) =>
        VmMapInfoFlagApply.ApplyScFlags(ref flags, FallDmgZd, ~FallDmgHx);

    public static void ApplyStrifeFallingDamage(ref uint flags) => flags |= FallDmgBoth;

    public static void ClearFallingDamage(ref uint flags) =>
        VmMapInfoFlagApply.ApplyScFlags(ref flags, 0, ~FallDmgBoth);

    public static void ApplyInfightingNone(ref uint flags2) =>
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, Noinfighting, ~TotalInfighting);

    public static void ApplyInfightingTotal(ref uint flags2) =>
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, TotalInfighting, ~Noinfighting);

    public static void ApplyInfightingNormal(ref uint flags2) =>
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, 0, ~(Noinfighting | TotalInfighting));

    public static void ApplyTeamplayOn(ref uint flags2) =>
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, TeamplayOn, ~TeamplayOff);

    public static void ApplyTeamplayOff(ref uint flags2) =>
        VmMapInfoFlagApply.ApplyScFlags2(ref flags2, TeamplayOff, ~TeamplayOn);

    public static void ApplyDisableShadowMap(ref uint flags3) => flags3 |= NoShadowMap;

    public static void ApplyEnableShadowMap(ref uint flags3) => flags3 &= ~NoShadowMap;

    public static void ApplyCompatPassover(VmMapInfoLevelFlags level, bool passoverEnabled)
    {
        if (passoverEnabled)
            VmMapInfoFlagApply.ApplyClearCompat(level, CompatNoPassMobj, 0);
        else
            VmMapInfoFlagApply.ApplyCompat(level, CompatNoPassMobj, 0, set: true);
    }

    public static bool IsHandlersTableTerminator(string? name) => name is null;

    public static bool ShouldDispatchFlagHandler(int matchIndex) => matchIndex >= 0;

    public static bool RequiresEatNextAssignment(VmMapInfoFlagHandlerType type) =>
        type == VmMapInfoFlagHandlerType.EatNext;

    public static bool TryResolveCompatHandlerSeam(string name, out VmMapInfoFlagHandler handler) =>
        VmMapInfoFlagHandlersCompat.TryGetCompatHandler(name, out handler);

    public static bool ShouldIgnoreLegacyCompatRow(string keyword) =>
        keyword.Equals("compat_corpsegibs", StringComparison.OrdinalIgnoreCase)
        || keyword.Equals("nobotnodes", StringComparison.OrdinalIgnoreCase);
}
