namespace HCDE.Playsim.Tests;

public class VmGGameSession1001SliceTests
{
    [Fact]
    public void PlayerPopAndScreenshot()
    {
        var mob = new object();
        Assert.True(VmGGameSession1001Slice.ShouldRevertSpyCameraToSelf(
            viewerInGame: true,
            viewerCamera: mob,
            departingPlayerMob: mob));
        Assert.True(VmGGameSession1001Slice.ShouldAttachConsoleStatusBar(0, 0));
        Assert.True(VmGGameSession1001Slice.ShouldQueueScreenshot(
            VmGGameSession1001Slice.GameActionNothing,
            filename: "shot.png"));
    }

    [Fact]
    public void LoadGameAndWadChecks()
    {
        var req = VmGGameSession1001Slice.TryCreateLoadGameRequest("save1", hideConsole: true);
        Assert.NotNull(req);
        Assert.Equal(VmGGameSession1001Slice.GameActionLoadGameHideConsole, req!.Value.GameAction);
        Assert.True(VmGGameSession1001Slice.IsResourceWadLoaded(0));
        Assert.True(VmGGameSession1001Slice.ShouldSkipMapWadValidation("a.wad", "a.wad"));
        var merged = VmGGameSession1001Slice.MergePrintRequiresOnMissingWad(
            currentPrintRequires: false,
            printWarnings: true,
            wadMissing: true);
        Assert.True(merged);
        Assert.True(VmGGameSession1001Slice.ResolveSaveGameWadsComplete(
            gameWadLoaded: true,
            mapWadLoadedOrSkipped: true));
    }

    [Fact]
    public void SaveCvarFilter()
    {
        Assert.True(VmGGameSession1001Slice.ShouldSerializeCvarForSave(
            cvarFlags: 0x1,
            filterMask: 0x1,
            noSaveFlag: 0x2,
            ignoreFlag: 0x4,
            configOnlyFlag: 0x8));
        Assert.False(VmGGameSession1001Slice.ShouldSerializeCvarForSave(
            cvarFlags: 0x3,
            filterMask: 0x1,
            noSaveFlag: 0x2,
            ignoreFlag: 0x4,
            configOnlyFlag: 0x8));
    }
}
