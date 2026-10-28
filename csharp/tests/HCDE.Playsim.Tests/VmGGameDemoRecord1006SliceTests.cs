namespace HCDE.Playsim.Tests;

public class VmGGameDemoRecord1006SliceTests
{
    [Fact]
    public void ReadDemoDefaultDispatch()
    {
        Assert.True(VmGGameDemoRecord1006Slice.RoutesUnhandledDemoCommandToNet(
            commandId: 99,
            demoUserCmd: VmGGameSavePayload1005Slice.DemoUserCmd,
            demoEmptyUserCmd: VmGGameSavePayload1005Slice.DemoEmptyUserCmd,
            demoStop: VmGGameSavePayload1005Slice.DemoStop,
            demoDropPlayer: VmGGameSavePayload1005Slice.DemoDropPlayer));
    }

    [Fact]
    public void WriteDemoTiccmd()
    {
        Assert.Equal(
            VmGGameDemoRecord1006Slice.DemoRecordingWriteGate.FinalizeRecording,
            VmGGameDemoRecord1006Slice.ResolveDemoRecordingWriteGate(
                stopRecording: true,
                demoRecordingActive: true));
        Assert.True(VmGGameDemoRecord1006Slice.ShouldTraceFullConsoleAfterDemoStop(
            stopRecording: true,
            netGame: false));
        Assert.True(VmGGameDemoRecord1006Slice.ShouldWriteSpecialDemoTics(
            hasSpecData: true,
            gameTic: 35,
            ticDup: 35));
        Assert.True(VmGGameDemoRecord1006Slice.RequiresDemoBufferExpansion(
            writeOffset: 0x1FF00,
            bufferSize: 0x20000,
            safetyMargin: VmGGameDemoRecord1006Slice.DemoBufferSafetyMargin));
        var expansion = VmGGameDemoRecord1006Slice.ResolveDemoBufferExpansion(
            currentMaxSize: VmGGameDemoRecord1006Slice.InitialMaxDemoSize,
            writeOffset: 0x1000,
            bufferSize: 0x20000);
        Assert.Equal(0x40000, expansion.NewMaxDemoSize);
    }

    [Fact]
    public void RecordAndBeginDemo()
    {
        Assert.Equal(
            "foo.lmp",
            VmGGameDemoRecord1006Slice.AppendDefaultDemoExtensionIfMissing("foo", ".lmp"));
        Assert.Equal(
            "MAP01",
            VmGGameDemoRecord1006Slice.ResolveRecordingStartMap(null, "MAP01"));
        Assert.True(VmGGameDemoRecord1006Slice.ShouldWritePlayerInfoDemoChunk(
            playerIndex: 0,
            playerInGame: true));
        Assert.True(VmGGameDemoRecord1006Slice.ShouldWriteNetdemoHeaderChunk(
            multiplayer: true,
            activePlayerCount: 1));
        Assert.Equal(2, VmGGameDemoRecord1006Slice.DemoHeaderMinVersionField());
        Assert.Equal(3, VmGGameDemoRecord1006Slice.DemoHeaderFormatSubField());
        Assert.Equal(
            0x20010L,
            VmGGameDemoRecord1006Slice.RelinkDemoBufferPointer(
                pointerOffset: 0x1000,
                newBase: 0x20000,
                oldBase: 0));
        Assert.Equal(
            VmGGameDemoRecord1006Slice.UserGameModeForDemo.DemoSession,
            VmGGameDemoRecord1006Slice.ResolveUserGameModeForDemoRecord(recordDemo: true));
    }
}
