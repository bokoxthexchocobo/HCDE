namespace HCDE.Playsim.Tests;

public class VmPMapGetFrictionTests
{
    [Fact]
    public void FlyingPair_UsesFlyFriction() =>
        Assert.Equal(VmPMapGetFriction.FlyingFriction, VmPMapGetFriction.FlyingPair().Friction);

    [Fact]
    public void WaterFriction_HalvesMoveFactor() =>
        Assert.Equal(VmPMapGetFriction.OrigFrictionFactor * 0.5, VmPMapGetFriction.HalveMoveFactor(VmPMapGetFriction.OrigFrictionFactor));

    [Fact]
    public void PreferLowerFriction_WhenOrig() =>
        Assert.True(VmPMapGetFriction.PreferLowerFriction(0.5, VmPMapGetFriction.OrigFriction));

    [Fact]
    public void ApplyActorFrictionMultiplier_RecalculatesMoveFactor()
    {
        var (f, m) = VmPMapGetFriction.ApplyActorFrictionMultiplier(0.5, 0.1, 0.5);
        Assert.Equal(0.25, f, 3);
        Assert.Equal(VmPMapGetFriction.FrictionToMoveFactor(0.25), m, 6);
    }

    [Fact]
    public void BoostMoveFactorForVelocity_QuadruplesAtHighSpeed() =>
        Assert.Equal(8, VmPMapGetFriction.BoostMoveFactorForVelocity(1, VmPMapGetFriction.MoreFrictionVelocity * 5));

    [Fact]
    public void ResolveMoveFactor_IgnoresBoostOnDefaultFriction() =>
        Assert.Equal(0.2, VmPMapGetFriction.ResolveMoveFactor(VmPMapGetFriction.OrigFriction, 0.2, 100));

    [Fact]
    public void LineIsAbovePortalBlocked_ReturnsNegativeOne() =>
        Assert.Equal(-1, VmPMapGetFriction.LineIsAbovePortalBlocked(true));

    [Fact]
    public void LineIsBelowCompare_WhenActorBelowCeiling() =>
        Assert.Equal(1, VmPMapGetFriction.LineIsBelowCompare(32, 40));
}
