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
}
