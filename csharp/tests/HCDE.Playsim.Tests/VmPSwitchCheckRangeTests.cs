namespace HCDE.Playsim.Tests;

public class VmPSwitchCheckRangeTests
{
    [Fact]
    public void CheckSwitchRange_SkipsWhenNoFlags()
    {
        Assert.True(VmPSwitchCheckRange.CheckSwitchRange(
            new VmPSwitchCheckRange.User(),
            new VmPSwitchCheckRange.Line(),
            0,
            new VmPSwitchCheckRange.CheckContext(),
            false));
    }

    [Fact]
    public void StartButton_RepressSetsTimer()
    {
        var side = new object();
        var list = new List<VmPSwitchCheckRange.ButtonThinker> { new() { Side = side, Timer = 5 } };
        Assert.False(VmPSwitchCheckRange.StartButton(list, side, _ => { }));
        Assert.Equal(1, list[0].Timer);
    }
}
