namespace HCDE.Client.Tests;

public class VmMenuOptionFilterTests
{
    [Fact]
    public void ShouldSkipBlock_MatchesNativeRules()
    {
        Assert.True(VmMenuOptionFilter.ShouldSkipBlock("ReadThis", drawReadThis: true, swapMenu: false));
        Assert.True(VmMenuOptionFilter.ShouldSkipBlock("swapmenu", drawReadThis: false, swapMenu: true));
        Assert.False(VmMenuOptionFilter.ShouldSkipBlock("Other", false, false));
    }
}
