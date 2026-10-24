namespace HCDE.Playsim.Tests;

public class VmPMapRailDamageTalkTests
{
    [Fact]
    public void RailHit_BleedsWhenAllowed() =>
        Assert.True(VmPMapRailDamageTalk.EvaluateRailHitPuff(
            new VmPMapRailDamageTalk.RailHitPuffContext(0, 0, 0, true, false)).Bleed);

    [Fact]
    public void RailHit_SkipsBleedWhenInvuln() =>
        Assert.False(VmPMapRailDamageTalk.EvaluateRailHitPuff(
            new VmPMapRailDamageTalk.RailHitPuffContext(0, VmPMapRailDamageTalk.Mf2Invulnerable, 0, true, false)).Bleed);

    [Fact]
    public void BuildDamageFlags_IncludesRailgun() =>
        Assert.NotEqual(0, VmPMapRailDamageTalk.BuildDamageFlags(false, false, false) & VmPMapRailDamageTalk.DmgRailgun);

    [Fact]
    public void CanTalk_BlocksInCombat() =>
        Assert.False(VmPMapRailDamageTalk.CanTalk(true, 1, VmPMapRailDamageTalk.Mf4Incombat));

    [Fact]
    public void DeepSplash_EitherWaterCrossing() =>
        Assert.True(VmPMapRailDamageTalk.DeepSplash(false, true));
}
