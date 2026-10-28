namespace HCDE.Playsim.Tests;

public class VmGGameLateTail1009SliceTests
{
    [Fact]
    public void CheckDemoStatusPlayback()
    {
        Assert.Equal(35, VmGGameLateTail1009Slice.ComputeTimingDemoElapsedTicks(
            currentTime: 100,
            startTime: 65));
        Assert.True(VmGGameLateTail1009Slice.ShouldClearPlayerInGameSlot(playerIndex: 3));
        Assert.False(VmGGameLateTail1009Slice.ShouldClearPlayerInGameSlot(playerIndex: 0));
        Assert.True(VmGGameLateTail1009Slice.ShouldReattachStatusBarToConsole(
            hasStatusBar: true,
            consolePlayer: 0));
        Assert.Equal(
            VmGGameLateTail1009Slice.DemoPlaybackEndPolicy.ExitToFullConsole,
            VmGGameLateTail1009Slice.ResolveDemoPlaybackEndPolicy(
                singleDemo: true,
                timingDemo: false));
        Assert.Equal(35f, VmGGameLateTail1009Slice.ComputeTimedDemoFps(
            gameTics: 350,
            realTics: 350,
            ticRate: VmGGameLateTail1009Slice.TicRate));
        Assert.True(VmGGameLateTail1009Slice.ShouldAdvanceDemoLoopAfterPlayback(
            demoPlayback: true,
            singleDemo: false,
            timingDemo: false));
    }

    [Fact]
    public void DemoRecordingFinish()
    {
        Assert.Equal(112, VmGGameLateTail1009Slice.EstimateDemoBodyCompressCapacity(bodyLength: 100));
        Assert.True(VmGGameLateTail1009Slice.ShouldReplaceDemoBodyWithCompressed(
            zlibResult: VmGGameLateTail1009Slice.ZLibOk,
            compressedLength: 50,
            bodyLength: 100));
        Assert.True(VmGGameLateTail1009Slice.DemoRecordingFileWriteSucceeded(
            writerOpened: true,
            writeMatchedSize: true));
        Assert.True(VmGGameLateTail1009Slice.ShouldRemoveDemoFileAfterFailedWrite(
            writerOpened: true,
            saved: false));
    }

    [Fact]
    public void SlideshowAndAutosave()
    {
        Assert.Equal(
            "Inter_Pic1",
            VmGGameLateTail1009Slice.ResolveSlideshowSelection(null, "Inter_Pic1"));
        Assert.Equal(
            VmGGameLateTail1009Slice.ScriptScreenshotPolicy.Capture,
            VmGGameLateTail1009Slice.ResolveScriptScreenshotPolicy(enableScriptScreenshot: true));
        Assert.True(VmGGameLateTail1009Slice.ShouldTraceAutosaveGameAction(
            VmGGameLateTail1009Slice.GaNothing));
    }
}
