namespace HCDE.Playsim.Tests;

public class VmActionRaiseTests
{
    [Fact]
    public void ShouldRaiseMaster_WhenMasterSet() =>
        Assert.True(VmActionRaise.ShouldRaiseMaster(new object()));

    [Fact]
    public void IsChildOf_WhenMasterMatches()
    {
        var m = new object();
        Assert.True(VmActionRaise.IsChildOf(m, new object()));
    }

    [Fact]
    public void ShouldRaiseSibling_WhenSharedMaster()
    {
        var master = new object();
        Assert.True(VmActionRaise.ShouldRaiseSibling(master, master, new object(), new object()));
    }
}
