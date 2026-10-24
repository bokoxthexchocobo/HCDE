namespace HCDE.Playsim.Tests;

public class VmPMobjWaterSpawnConstructTests
{
    [Fact]
    public void SplashCheck_BoomTransition() =>
        Assert.True(VmPMobjWaterSpawnConstruct.ShouldSplashOnBoomTransition(0, 2));

    [Fact]
    public void UpdateWaterLevel_RunsSplashWhenRequested() =>
        Assert.True(VmPMobjWaterSpawnConstruct.RunSplashCheck(
            new VmPMobjWaterSpawnConstruct.UpdateWaterLevelFlow(true, false, 0)));

    [Fact]
    public void InstantReaction_ZeroesMonsterReaction() =>
        Assert.Equal(0, VmPMobjWaterSpawnConstruct.MonsterReactionTime(
            VmPMobjWaterSpawnConstruct.InstantReactionApplies(true, 0, 0, VmPMobjWaterSpawnConstruct.Mf3Ismonster)));

    [Fact]
    public void ResolveSpawnZ_FloatRandzUsesSpace() =>
        Assert.True(VmPMobjWaterSpawnConstruct.ResolveSpawnZ(
            VmPMobjWaterSpawnConstruct.Floatrandz, 0, 100, 32, 128) > 0);

    [Fact]
    public void ConstructTail_StripsFriendlyOnNoAllies() =>
        Assert.Equal(0, VmPMobjWaterSpawnConstruct.EvaluateConstructTail(
            SampleContext(levelNoAllies: true)).Flags & VmPMobjWaterSpawnConstruct.MfFriendly);

    [Fact]
    public void ConstructTail_ForcesScrollerFlag() =>
        Assert.NotEqual(0, VmPMobjWaterSpawnConstruct.EvaluateConstructTail(SampleContext()).Flags8);

    private static VmPMobjWaterSpawnConstruct.ConstructActorContext SampleContext(bool levelNoAllies = false) =>
        new(false, false, 1, false, 0, 0, VmPMobjWaterSpawnConstruct.Mf3Ismonster,
            false, false, VmPMobjWaterSpawnConstruct.Onfloorz, 0, 64, 32, 10,
            0, levelNoAllies ? VmPMobjWaterSpawnConstruct.LevelNoallies : 0,
            true, VmPMobjWaterSpawnConstruct.MfFriendly, VmPMobjWaterSpawnConstruct.Mf5Countsecret);
}
