namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapFlagHandlers1033SliceTests
{
    [Fact]
    public void ApplyFreelookScFlags_TogglesMutually()
    {
        uint flags = 0;
        VmMapInfoMapFlagHandlers1033Slice.ApplyFreelookScFlags(ref flags, allowFreelook: false);
        Assert.Equal(VmMapInfoMapFlagHandlers1033Slice.FreelookNo, flags);
    }

    [Fact]
    public void ApplyFallingDamageModes_ExclusiveHxAndZd()
    {
        uint flags = 0;
        VmMapInfoMapFlagHandlers1033Slice.ApplyFallingDamageHx(ref flags);
        Assert.Equal(VmMapInfoMapFlagHandlers1033Slice.FallDmgHx, flags);
        VmMapInfoMapFlagHandlers1033Slice.ApplyFallingDamageZd(ref flags);
        Assert.Equal(VmMapInfoMapFlagHandlers1033Slice.FallDmgZd, flags);
    }

    [Fact]
    public void ApplyInfightingNormal_ClearsBothBits()
    {
        uint flags2 = VmMapInfoMapFlagHandlers1033Slice.Noinfighting | VmMapInfoMapFlagHandlers1033Slice.TotalInfighting;
        VmMapInfoMapFlagHandlers1033Slice.ApplyInfightingNormal(ref flags2);
        Assert.Equal(0u, flags2 & (VmMapInfoMapFlagHandlers1033Slice.Noinfighting | VmMapInfoMapFlagHandlers1033Slice.TotalInfighting));
    }

    [Fact]
    public void ApplyTeamplayOn_SetsOnMask()
    {
        uint flags2 = 0;
        VmMapInfoMapFlagHandlers1033Slice.ApplyTeamplayOn(ref flags2);
        Assert.Equal(VmMapInfoMapFlagHandlers1033Slice.TeamplayOn, flags2);
    }

    [Fact]
    public void ApplyShadowMapToggle_FlipNoShadowBit()
    {
        uint flags3 = VmMapInfoMapFlagHandlers1033Slice.NoShadowMap;
        VmMapInfoMapFlagHandlers1033Slice.ApplyEnableShadowMap(ref flags3);
        Assert.Equal(0u, flags3);
    }

    [Fact]
    public void ShouldDispatchFlagHandler_RequiresNonNegativeIndex()
    {
        Assert.True(VmMapInfoMapFlagHandlers1033Slice.ShouldDispatchFlagHandler(0));
        Assert.False(VmMapInfoMapFlagHandlers1033Slice.ShouldDispatchFlagHandler(-1));
    }

    [Fact]
    public void TryResolveCompatHandlerSeam_DelegatesOffTallyCatalog()
    {
        Assert.True(VmMapInfoMapFlagHandlers1033Slice.TryResolveCompatHandlerSeam(
            "compat_noid24", out var handler));
        Assert.Equal("compat_noid24", handler.Name);
    }

    [Fact]
    public void RequiresEatNextAssignment_OnlyForEatNextType()
    {
        Assert.True(VmMapInfoMapFlagHandlers1033Slice.RequiresEatNextAssignment(
            VmMapInfoFlagHandlerType.EatNext));
        Assert.False(VmMapInfoMapFlagHandlers1033Slice.RequiresEatNextAssignment(
            VmMapInfoFlagHandlerType.SetFlag));
    }

    [Fact]
    public void ShouldIgnoreLegacyCompatRow_ForNobotnodesAndCorpseGibs()
    {
        Assert.True(VmMapInfoMapFlagHandlers1033Slice.ShouldIgnoreLegacyCompatRow("nobotnodes"));
    }
}
