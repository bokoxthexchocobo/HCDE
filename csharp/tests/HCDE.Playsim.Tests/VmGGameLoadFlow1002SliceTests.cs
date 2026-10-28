namespace HCDE.Playsim.Tests;

public class VmGGameLoadFlow1002SliceTests
{
    [Fact]
    public void DeserializeCvarFilter()
    {
        Assert.True(VmGGameLoadFlow1002Slice.ShouldDeserializeCvarFromSave(
            cvarFlags: 0x1,
            filterMask: 0x1,
            noSaveFlag: 0x2,
            ignoreFlag: 0x4,
            configOnlyFlag: 0x8));
        Assert.True(VmGGameLoadFlow1002Slice.ShouldApplyLoadedCvarString("value"));
    }

    [Fact]
    public void DoLoadGameHeadGates()
    {
        Assert.True(VmGGameLoadFlow1002Slice.ShouldStopDemoPlaybackForLoad(
            gameAction: VmGGameLoadFlow1002Slice.GaLoadGameHideConsole));
        Assert.True(VmGGameLoadFlow1002Slice.ShouldHideConsoleAfterLoadMenu(
            VmGGameLoadFlow1002Slice.GaLoadGameHideConsole));
        Assert.True(VmGGameLoadFlow1002Slice.IsSaveArchiveEntryMissing(-1));
    }

    [Fact]
    public void EngineAndVersionChecks()
    {
        Assert.True(VmGGameLoadFlow1002Slice.IsEngineSignatureAllowed(
            engineSignature: "HCDE",
            gameSignature: "HCDE",
            allowLoadIn: []));
        Assert.Equal(
            VmGGameLoadFlow1002Slice.EngineCompatibilityReject.ForeignEngine,
            VmGGameLoadFlow1002Slice.ResolveEngineCompatibilityReject(
                engineSignature: "OTHER",
                engineAllowed: false));
        Assert.True(VmGGameLoadFlow1002Slice.IsSaveVersionSupported(
            saveVersion: VmGGameLoadFlow1002Slice.MinSaveVersion,
            minVersion: VmGGameLoadFlow1002Slice.MinSaveVersion,
            maxVersion: VmGGameLoadFlow1002Slice.CurrentSaveVersion));
        Assert.Equal(
            VmGGameLoadFlow1002Slice.GsHideConsole,
            VmGGameLoadFlow1002Slice.ResolveGameStateAfterLoadHideRequest(
                hideConsole: true,
                currentGameState: VmGGameLoadFlow1002Slice.GsFullConsole,
                gsFullConsole: VmGGameLoadFlow1002Slice.GsFullConsole,
                gsHideConsole: VmGGameLoadFlow1002Slice.GsHideConsole));
    }
}
