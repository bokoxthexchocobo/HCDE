namespace HCDE.Playsim.Tests;

public class VmPMapBounceReflectTests
{
    [Fact]
    public void ThrureflectPass_WhenReflective() =>
        Assert.True(VmPMapBounceReflect.ThrureflectPass(true, true));

    [Fact]
    public void RipperSkipsShootable_WhenRip() =>
        Assert.True(VmPMapBounceReflect.RipperSkipsShootable(true, true, true, false, false));

    [Fact]
    public void ReflectRequiresReflective() =>
        Assert.True(VmPMapBounceReflect.ReflectRequiresReflective(1));

    [Fact]
    public void EvaluateBounceActorEarly_HonorsSpecialBounceBlock()
    {
        var ctx = new VmPMapBounceReflect.BounceActorContext(
            true, 2, 0, false, false, false, false, false, false, false, false, 0, 0, false);
        Assert.False(VmPMapBounceReflect.EvaluateBounceActorEarly(ctx));
    }

    [Fact]
    public void BounceStateLookupCount_MatchesCreatureGate()
    {
        Assert.Equal(3, VmPMapBounceReflect.BounceStateLookupCount(true, false));
        Assert.Equal(2, VmPMapBounceReflect.BounceStateLookupCount(true, true));
    }
}
