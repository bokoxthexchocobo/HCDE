namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppLumpGateTests
{
    [Fact]
    public void TryOpenDehsupp_RejectsUserWad()
    {
        Assert.False(VmDehackedDehSuppLumpGate.TryOpenDehsupp(0, 1, out var warning));
        Assert.NotNull(warning);
    }

    [Fact]
    public void TryOpenDehsupp_AcceptsCoreLump()
    {
        Assert.True(VmDehackedDehSuppLumpGate.TryOpenDehsupp(0, 0, out _));
    }
}
