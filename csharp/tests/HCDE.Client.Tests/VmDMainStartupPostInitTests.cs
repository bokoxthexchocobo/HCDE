namespace HCDE.Client.Tests;

public class VmDMainStartupPostInitTests
{
    [Fact]
    public void StartupKind_ResolvesHexen() =>
        Assert.Equal(
            VmDMainStartupPostInit.StartupScreenKind.Hexen,
            VmDMainStartupPostInit.ResolveStartupKind(
                VmDMainStartupPostInit.StartupScreenKind.Default,
                VmDMainStartupPostInit.GameHexen));

    [Fact]
    public void NoStartScreen_WhenBatchRun() =>
        Assert.True(VmDMainStartupPostInit.NoStartScreen(true, false, false, false, false, false, false, false));

    [Fact]
    public void TextureProgress_Headroom4096WhenZeroGuess() =>
        Assert.Equal(4096, VmDMainStartupPostInit.TextureProgressHeadroom(0));

    [Fact]
    public void DedicatedSkippedBanner_Pluralizes() =>
        Assert.Contains("commands", VmDMainStartupPostInit.DedicatedSkippedCommandsBanner(2));

    [Fact]
    public void MusInfoAfterMapInfo() =>
        Assert.True(VmDMainStartupPostInit.MusInfoAfterMapInfo(true));

    [Fact]
    public void EnableCvarCallbacksAfterTexMan() =>
        Assert.True(VmDMainStartupPostInit.EnableCvarCallbacksAfterTexMan(true));
}
