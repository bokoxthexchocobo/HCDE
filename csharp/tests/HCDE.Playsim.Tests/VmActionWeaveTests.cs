namespace HCDE.Playsim.Tests;

public class VmActionWeaveTests
{
    [Fact]
    public void WeaveIndexXY_MasksToSixBits() =>
        Assert.Equal(3, VmActionWeave.WeaveIndexXY(67));

    [Fact]
    public void ShouldWeaveXY_WhenDistAndSpeedNonZero() =>
        Assert.True(VmActionWeave.ShouldWeaveXY(1, 2));

    [Fact]
    public void AdvanceWeaveIndex_WrapsAt64() =>
        Assert.Equal(0, VmActionWeave.AdvanceWeaveIndex(63, 1));

    [Fact]
    public void UseTryMoveWhenInteractive_WithoutNointeraction() =>
        Assert.True(VmActionWeave.UseTryMoveWhenInteractive(0));
}
