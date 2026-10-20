namespace HCDE.Playsim.Tests;

public class VmPMapBounceActorTests
{
    [Fact]
    public void ThruReflect_PassesReflectiveActor() =>
        Assert.True(VmPMapBounceActor.ThruReflectPasses(true, true));

    [Fact]
    public void RipSkipsShootable_WhenNoBounceOnUnrips() =>
        Assert.True(VmPMapBounceActor.RipSkipsShootable(true, true, 0, true, false));

    [Fact]
    public void BounceActor_SpecialMissileHitStops() =>
        Assert.False(VmPMapBounceActor.EvaluateBounceActorEntry(
            new VmPMapBounceActor.BounceActorContext(true, true, 0, -1, false, 0, true, false, false, true, 1)));

    [Fact]
    public void Reflect_RequiresReflectiveFlag() =>
        Assert.False(VmPMapBounceActor.ReflectRequiresReflective(0));

    [Fact]
    public void HereticBounce_DeathWhenNotMbf() =>
        Assert.True(VmPMapBounceActor.HereticBounceDeath(VmPMapBounceActor.BounceHeretictype));

    [Fact]
    public void UseBounceState_FlagSet() =>
        Assert.True(VmPMapBounceActor.UseBounceState(VmPMapBounceActor.BounceUsebouncestate));
}
