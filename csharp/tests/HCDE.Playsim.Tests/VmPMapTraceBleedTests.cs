namespace HCDE.Playsim.Tests;

public class VmPMapTraceBleedTests
{
    [Fact]
    public void SkipBleed_WhenNoBloodFlag() =>
        Assert.True(VmPMapTraceBleed.SkipBleedActor(VmPMapTraceBleed.MfNoblood, 0, 0, false));

    [Fact]
    public void PlanBleed_LowDamageCanSkip() =>
        Assert.Equal(0, VmPMapTraceBleed.SelectBleedSprayNative(8, 100).Count);

    [Fact]
    public void PlanBleed_HighDamageSmearChance() =>
        Assert.Equal("BloodSmear", VmPMapTraceBleed.SelectBleedSprayNative(40, 0).BloodType);

    [Fact]
    public void DefaultBleedDistance_Fallback172() =>
        Assert.Equal(172, VmPMapTraceBleed.DefaultBleedDistance(0));

    [Fact]
    public void MissileBleed_SkipsBloodless() =>
        Assert.True(VmPMapTraceBleed.MissileBleedSkip(false, VmPMapTraceBleed.Mf3Bloodlessimpact));

    [Fact]
    public void LineTargetBleed_SkipsNullTarget() =>
        Assert.True(VmPMapTraceBleed.LineTargetBleedSkip(true, 0));

    [Fact]
    public void RailHit_SkipsThruActors() =>
        Assert.Equal(VmPMapTraceBleed.RailTraceStatus.Skip,
            VmPMapTraceBleed.EvaluateRailActorHit(SampleRail(thruActors: true)));

    [Fact]
    public void RailHit_StopsAtInvuln() =>
        Assert.Equal(VmPMapTraceBleed.RailTraceStatus.Stop,
            VmPMapTraceBleed.EvaluateRailActorHit(SampleRail(stopAtInvul: true, actorFlags2: VmPMapTraceBleed.Mf2Invulnerable)));

    [Fact]
    public void RailShootZ_AddsAttackOffsetWhenNotCenterZ() =>
        Assert.True(VmPMapTraceBleed.RailShootZ(32, 0, 0, 0, 0, 4) > 32);

    [Fact]
    public void BuildRailData_ClearsThruWhenNoAllowFlag() =>
        Assert.False(VmPMapTraceBleed.BuildRailDataFlags(
            new VmPMapTraceBleed.RailDataInitContext(0, 0, 0, 0, 0, 0, 0, true)).ThruActors);

    [Fact]
    public void ProcessRailHitPolicy_StopsOnWall() =>
        Assert.Equal(VmPMapTraceBleed.RailTraceStatus.Stop,
            VmPMapTraceBleed.ProcessRailHitPolicy(0, SampleRail()));

    [Fact]
    public void TryPrepareWallDecal_DimsColor() =>
        Assert.True(VmPMapTraceBleed.TryPrepareWallDecal(
            new VmPMapTraceBleed.WallDecalBleedInput(3, 200, 100, 50, true),
            out var color) && color.R == 100);

    [Fact]
    public void RunMissileTargetBleed_ComputesPitch() =>
        Assert.True(VmPMapTraceBleed.RunMissileTargetBleed(false, 0, -10, 5, out var pitch) && pitch != 0);

    private static VmPMapTraceBleed.RailHitContext SampleRail(
        bool thruActors = false,
        bool stopAtInvul = false,
        int actorFlags2 = 0) =>
        new(thruActors, false, 0, 0, false, false, false, false, false, false, false, false,
            stopAtInvul, actorFlags2, false, false, 0, 0, 0);
}
