namespace HCDE.Gamedata.Tests;

public class VmIntermissionScreenRuntimeTests
{
    [Fact]
    public void TickerAndOverlay_MatchNativeGuards()
    {
        Assert.Equal(-1, VmIntermissionScreenRuntime.AdvanceTicker(9, 10));
        Assert.False(VmIntermissionScreenRuntime.ShouldShowOverlay("Multiplayer", multiplayer: false, true, true));
        Assert.True(VmIntermissionScreenRuntime.ShouldShowOverlay("Multiplayer", multiplayer: true, false, false));
    }

    [Fact]
    public void TextScreenDurationAndFaderAlpha()
    {
        Assert.Equal(45, VmIntermissionScreenRuntime.ComputeTextScreenDuration(10, 5, 2, 15));
        Assert.Equal(0, VmIntermissionScreenRuntime.ComputeFaderOverlayAlpha(10, 10, VmIntermissionFadeType.In));
        Assert.True(VmIntermissionScreenRuntime.ComputeFaderOverlayAlpha(5, 10, VmIntermissionFadeType.Out) > 0);
    }
}
