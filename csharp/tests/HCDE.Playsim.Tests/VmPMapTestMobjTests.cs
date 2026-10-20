namespace HCDE.Playsim.Tests;

public class VmPMapTestMobjTests
{
    [Fact]
    public void ZOkWhenPositionOk() =>
        Assert.True(VmPMapTestMobj.ZOkWhenPositionOk(16, 0, 32, 64));

    [Fact]
    public void CorpseNeedsIce() =>
        Assert.True(VmPMapTestMobj.CorpseNeedsIce(VmPMapTestMobj.MfCorpse, 0));

    [Fact]
    public void FloatChaseTarget_WhenClose() =>
        Assert.True(VmPMapTestMobj.FloatChaseTarget(true, false, false, 10, 5));

    [Fact]
    public void WindowNotBlocking() =>
        Assert.True(VmPMapTestMobj.WindowNotBlocking(64, 0, 64, 0, 32, 16));

    [Fact]
    public void OverOrUnder() =>
        Assert.True(VmPMapTestMobj.OverOrUnder(40, 48, 32, 0));

    [Fact]
    public void PushWallActivates() =>
        Assert.True(VmPMapTestMobj.PushWallActivates(true));
}
