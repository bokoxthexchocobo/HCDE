namespace HCDE.Playsim.Tests;

public class VmPMobjFileHead960SliceTests
{
    [Fact]
    public void SanitizeSvGravity_ReplacesNaNWithDefault()
    {
        Assert.Equal(
            VmPMobjFileHead960Slice.DefaultSvGravity,
            VmPMobjFileHead960Slice.SanitizeSvGravity(float.NaN));
    }

    [Fact]
    public void SanitizeSvGravity_PreservesFiniteValue()
    {
        Assert.Equal(650f, VmPMobjFileHead960Slice.SanitizeSvGravity(650f));
    }

    [Fact]
    public void ApplySvGravityToAllLevels_UpdatesEachLevel()
    {
        var touched = new List<float>();
        VmPMobjFileHead960Slice.ApplySvGravityToAllLevels(
            [new object(), new object()],
            (_, g) => touched.Add(g),
            900f);
        Assert.Equal([900f, 900f], touched);
    }

    [Fact]
    public void AdvanceWaterSink_UsesSmallFactorWhenRequested()
    {
        var z = VmPMobjFileHead960Slice.AdvanceWaterSink(10, smallActor: true);
        var expected = 10 - (VmPMobjFileHead960Slice.WaterSinkSmallFactor * VmPMobjFileHead960Slice.WaterSinkSpeed);
        Assert.Equal(expected, z);
    }

    [Fact]
    public void WaterJumpVelocity_ScalesMultiplier()
    {
        Assert.Equal(VmPMobjFileHead960Slice.WaterJumpSpeed * 2, VmPMobjFileHead960Slice.WaterJumpVelocity(2));
    }

    [Fact]
    public void RegistersAActorGcPointer_MatchesImplementPointerBand()
    {
        Assert.True(VmPMobjFileHead960Slice.RegistersAActorGcPointer("LastLookActor"));
        Assert.False(VmPMobjFileHead960Slice.RegistersAActorGcPointer("Inventory"));
    }

    [Fact]
    public void GcPointerRegistrationOrder_MatchesNativeSequence()
    {
        var order = VmPMobjFileHead960Slice.AActorGcPointerRegistrationOrderThrough150();
        Assert.Equal(VmPMobjFileHead960Slice.AActorGcPointerBandCountThrough150(), order.Count);
        Assert.Equal("target", order[0]);
        Assert.Equal("LastLookActor", order[^1]);
    }

    [Fact]
    public void ResolveSvGravityCvar_SanitizesAndApplies()
    {
        var levels = new List<object> { new(), new() };
        var applied = new List<float>();
        var result = VmPMobjFileHead960Slice.ResolveSvGravityCvar(
            float.PositiveInfinity,
            levels,
            (_, g) => applied.Add(g));
        Assert.Equal(VmPMobjFileHead960Slice.DefaultSvGravity, result);
        Assert.Equal([800f, 800f], applied);
    }

    [Fact]
    public void ClampsBelowZUnderflow_DetectsUnderflowConstant()
    {
        Assert.True(VmPMobjFileHead960Slice.ClampsBelowZUnderflow(VmPMobjFileHead960Slice.ZUnderflow - 1));
    }
}
