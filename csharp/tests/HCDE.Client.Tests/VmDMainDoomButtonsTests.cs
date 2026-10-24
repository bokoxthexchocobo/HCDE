namespace HCDE.Client.Tests;

public class VmDMainDoomButtonsTests
{
    [Fact]
    public void ButtonCount_MatchesNativeTable()
    {
        Assert.Equal(32, VmDMainDoomButtons.ButtonCount);
        Assert.Equal("attack", VmDMainDoomButtons.ResolveButton(11));
    }
}
