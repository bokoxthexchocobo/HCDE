namespace HCDE.Client.Tests;

public class VmDDoomMainIwadIterationTests
{
    [Fact]
    public void SharewareBlocksPwads_WhenModsPresent() =>
        Assert.True(VmDDoomMainIwadIteration.SharewareBlocksPwads(true, 1));

    [Fact]
    public void SteamAppIdInvalidChar() =>
        Assert.True(VmDDoomMainIwadIteration.SteamAppIdInvalidChar('x'));

    [Fact]
    public void CancelWhenNoIwadSelected() =>
        Assert.True(VmDDoomMainIwadIteration.CancelWhenNoIwadSelected(true));

    [Fact]
    public void AbortLoopWhenInitGameFailed() =>
        Assert.True(VmDDoomMainIwadIteration.AbortLoopWhenInitGameFailed(1));

    [Fact]
    public void ValidateSteamAppIdString_RejectsLetters() =>
        Assert.False(VmDDoomMainIwadIteration.ValidateSteamAppIdString("12a", out var failed) && !failed);
}
