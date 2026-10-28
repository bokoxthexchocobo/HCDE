namespace HCDE.Playsim.Tests;

public class VmGGameDemoPlayback1008SliceTests
{
    [Fact]
    public void ProcessIffDemoTail()
    {
        Assert.Equal(
            VmGGameDemoPlayback1008Slice.DemoIffTailFailure.NoPlayers,
            VmGGameDemoPlayback1008Slice.ResolveNoPlayersFailure(numPlayers: 0));
        Assert.Equal(
            VmGGameDemoPlayback1008Slice.DemoIffTailFailure.NoBody,
            VmGGameDemoPlayback1008Slice.ResolveNoBodyFailure(bodyHit: false));
        Assert.True(VmGGameDemoPlayback1008Slice.ShouldMarkNetgameFromDemoPlayerCount(numPlayers: 2));
        Assert.True(VmGGameDemoPlayback1008Slice.ShouldDecompressDemoBody(uncompSize: 1024));
        Assert.True(VmGGameDemoPlayback1008Slice.DecompressDemoBodySucceeded(
            VmGGameDemoPlayback1008Slice.ZLibOk));
    }

    [Fact]
    public void DoPlayDemoHead()
    {
        Assert.True(VmGGameDemoPlayback1008Slice.IsDemoLumpInFilesystem(lumpIndex: 0));
        Assert.Equal(
            "demo.lmp",
            VmGGameDemoPlayback1008Slice.AppendDemoExtensionForFileOpen("demo", ".lmp"));
        Assert.True(VmGGameDemoPlayback1008Slice.IsDemoBufferTooSmallForForm(bufferSize: 2));
        Assert.Equal(
            VmGGameDemoPlayback1008Slice.SingleDemoMessagePolicy.PrintPlaying,
            VmGGameDemoPlayback1008Slice.ResolveSingleDemoMessagePolicy(singleDemo: true));
        Assert.True(VmGGameDemoPlayback1008Slice.IsNonZdoomFormDemo(formId: 0));
        Assert.True(VmGGameDemoPlayback1008Slice.ShouldRestoreCvarsAfterIffFailure(
            iffParseFailed: true,
            cvarsWereBackedUp: true));
        Assert.True(VmGGameDemoPlayback1008Slice.ShouldInitNewLevelFromDemoMap("MAP01"));
        Assert.True(VmGGameDemoPlayback1008Slice.RequiresSavegameWhenMapNameEmpty(sectorCount: 0));
    }

    [Fact]
    public void TimeDemoAndStatusHead()
    {
        Assert.Equal(
            VmGGameDemoPlayback1008Slice.GaPlayDemo,
            VmGGameDemoPlayback1008Slice.ResolveTimeDemoGameAction(
                VmGGameDemoPlayback1008Slice.GaNothing));
        var flags = VmGGameDemoPlayback1008Slice.ResolveTimeDemoDrawFlags(
            nodrawArg: true,
            noblitArg: false);
        Assert.True(flags.Nodrawers && !flags.Noblit);
        Assert.Equal(
            VmGGameDemoPlayback1008Slice.DemoStatusUserInfoPolicy.RestorePlayerInfo,
            VmGGameDemoPlayback1008Slice.ResolveDemoStatusUserInfoPolicy(demoRecording: false));
        Assert.True(VmGGameDemoPlayback1008Slice.ShouldComputeTimingDemoElapsed(
            demoPlayback: true,
            timingDemo: true));
    }
}
