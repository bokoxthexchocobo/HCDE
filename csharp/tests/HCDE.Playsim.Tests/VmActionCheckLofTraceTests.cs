namespace HCDE.Playsim.Tests;

public class VmActionCheckLofTraceTests
{
    [Fact]
    public void EvaluateHitActor_StopsOnJumpEnemy()
    {
        var bad = false;
        var d = VmActionCheckLofTrace.EvaluateHitActor(
            true, new object(), new object(), VmActionCheckLofTrace.CloffJumpEnemy,
            selfHostileToActor: true, selfFriendToActor: false, actorIsMonster: true,
            actorFlags: VmActionCheckLofTrace.MfShootable, actorFlags2: 0, actorFlags3: VmActionCheckLofTrace.Mf3IsMonster,
            ref bad);
        Assert.Equal(VmActionCheckLofTrace.TraceDecision.Stop, d);
        Assert.False(bad);
    }

    [Fact]
    public void EvaluateHitActor_SkipsNonShootableWhenRequired()
    {
        var bad = false;
        var d = VmActionCheckLofTrace.EvaluateHitActor(
            true, new object(), new object(), VmActionCheckLofTrace.CloffMustBeShootable,
            false, false, false, 0, 0, 0, ref bad);
        Assert.Equal(VmActionCheckLofTrace.TraceDecision.Skip, d);
    }

    [Fact]
    public void EvaluateHitActor_AbortsAndSetsBadActor()
    {
        var bad = false;
        var d = VmActionCheckLofTrace.EvaluateHitActor(
            true, new object(), new object(), 0,
            false, false, false, VmActionCheckLofTrace.MfShootable, 0, 0, ref bad);
        Assert.Equal(VmActionCheckLofTrace.TraceDecision.Abort, d);
        Assert.True(bad);
    }
}
