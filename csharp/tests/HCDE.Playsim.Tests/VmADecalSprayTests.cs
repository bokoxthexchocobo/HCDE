namespace HCDE.Playsim.Tests;

public class VmADecalSprayTests
{
    [Fact]
    public void SprayDecal_NullShooter_NoOp()
    {
        var called = false;
        VmADecalSpray.SprayDecal(
            null,
            "foo",
            64,
            0,
            0,
            0,
            1,
            0,
            0,
            false,
            0,
            0,
            _ => 0,
            (_, _) => 0,
            _ => (0, 0, 0),
            _ => 1,
            _ => 0,
            _ => 1,
            _ => 0,
            _ => 0,
            _ => 0,
            _ => _,
            (_, _, _, _, _, _) => (0, (0, 0, 0), null, 0, null),
            1,
            _ => null,
            _ => null,
            (_, _, _, _) => new VmADecalImpact.StaticCreateContext());
        Assert.False(called);
    }
}
