namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnMapThingBandsTests
{
    [Fact]
    public void Head_RejectsZeroEdNum() =>
        Assert.True(VmPMobjSpawnMapThingBands.InvalidEdNum(0));

    [Fact]
    public void Head_PolySpawnSkipped() =>
        Assert.True(VmPMobjSpawnMapThingBands.IsPolySpawnSpecial(VmPMobjSpawnMapThingBands.SmtPolySpawnHurt));

    [Fact]
    public void Mask_DeathmatchUsesDmFlag() =>
        Assert.Equal(VmPMobjSpawnMapThingBands.MtfDeathmatch,
            VmPMobjSpawnMapThingBands.ResolveThingMask(
                new VmPMobjSpawnMapThingBands.GameTypeMaskContext(true, false, false, false)));

    [Fact]
    public void Seq_Clamp255ToNegativeOne() =>
        Assert.Equal(-1, VmPMobjSpawnMapThingBands.NormalizeSeqOverrideArg(255));

    [Fact]
    public void Invasion_DelayMinimumTenSeconds() =>
        Assert.Equal(10, VmPMobjSpawnMapThingBands.InvasionDelaySeconds(3));

    [Fact]
    public void Tail_BlocksNotDmMatchInDeathmatch() =>
        Assert.True(VmPMobjSpawnMapThingBands.BlockDeathmatchThing(true, VmPMobjSpawnMapThingBands.MfNotdmatch));

    [Fact]
    public void Filter_AppliesWhenNoPlayerStart() =>
        Assert.True(VmPMobjSpawnMapThingBands.ApplyGeneralSpawnFilters(-1, 0));

    [Fact]
    public void Coop_FoldSpawnClassAnyPlayerClearsMask() =>
        Assert.Equal(-1, VmPMobjSpawnMapThingBands.FoldCoopSpawnClass(4, 0));

    [Fact]
    public void Coop_BlockDmOnlyThings() =>
        Assert.True(VmPMobjSpawnMapThingBands.BlockDmOnlyInCoop(
            VmPMobjSpawnMapThingBands.Df2NoCoopThingSpawn, VmPMobjSpawnMapThingBands.MtfDeathmatch));

    [Fact]
    public void Coop_BlockMonstersWhenNomonsters() =>
        Assert.True(VmPMobjSpawnMapThingBands.BlockMonsterSpawn(
            VmPMobjSpawnMapThingBands.Level2Nomonsters, 0, VmPMobjSpawnMapThingBands.Mf3Ismonster));
}
