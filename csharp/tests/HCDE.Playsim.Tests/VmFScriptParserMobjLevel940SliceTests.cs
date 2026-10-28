namespace HCDE.Playsim.Tests;

public class VmFScriptParserMobjLevel940SliceTests
{
    [Fact]
    public void MobjGeomAssignsWhenPresent_RequiresSecondArgAndActor() =>
        Assert.False(VmFScriptParserMobjLevel940Slice.MobjGeomAssignsWhenPresent(1, new object()));

    [Fact]
    public void ThingCountCountsMapActorsOnly_WhenLivingModeZero() =>
        Assert.True(VmFScriptParserMobjLevel940Slice.ThingCountCountsMapActorsOnly(2, 0, false));

    [Fact]
    public void ThingCountCountsLiving_WhenModeNonZeroAndNotInventory() =>
        Assert.True(VmFScriptParserMobjLevel940Slice.ThingCountCountsLiving(2, 1, false));

    [Fact]
    public void ThingCountActorQualifies_LivingBranchIgnoresMapActor() =>
        Assert.True(VmFScriptParserMobjLevel940Slice.ThingCountActorQualifies(false, false, 5));

    [Fact]
    public void BuildSetColorApply_RejectsMissingTagSector() =>
        Assert.Null(VmFScriptParserMobjLevel940Slice.BuildSetColorApply(2, 7, -1, 0x112233, 0, 0));

    [Fact]
    public void ComposePalEntryRgb_PacksChannels() =>
        Assert.Equal(0x00010203, VmFScriptParserMobjLevel940Slice.ComposePalEntryRgb(3, 2, 1));

    [Fact]
    public void SectorTagIteratorActive_MatchesNativeLoopGuard() =>
        Assert.True(VmFScriptParserMobjLevel940Slice.SectorTagIteratorActive(0));
}
