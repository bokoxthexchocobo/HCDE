namespace HCDE.Playsim.Tests;

public class VmGGameSaveFlow1004SliceTests
{
    [Fact]
    public void QuicksaveRotation()
    {
        Assert.Equal(
            VmGGameSaveFlow1004Slice.QuicksaveSkipReason.NetGame,
            VmGGameSaveFlow1004Slice.ResolveQuicksaveSkipReason(netGame: true));
        Assert.Equal(3, VmGGameSaveFlow1004Slice.ResolveQuicksaveRotationCount(3));
        Assert.Equal(0, VmGGameSaveFlow1004Slice.ResolveLastQuicksaveSlot(
            quicksaveNum: -1,
            slotCount: 4));
        Assert.Equal(2, VmGGameSaveFlow1004Slice.ResolveLastQuicksaveSlot(
            quicksaveNum: 1,
            slotCount: 4));
    }

    [Fact]
    public void DoSaveGameHead()
    {
        Assert.True(VmGGameSaveFlow1004Slice.ShouldAbortSaveWithoutLevel(
            lineCount: 0,
            sectorCount: 1,
            gameState: VmGGameSaveFlow1004Slice.GsLevel));
        Assert.Equal("demosave", VmGGameSaveFlow1004Slice.ResolveSaveFilenameForContext(
            demoPlayback: true,
            requestedFilename: "user.sav",
            demoSaveLeafName: "demosave"));
        var cleanup = VmGGameSaveFlow1004Slice.ResolveSnapshotFailureCleanup(clWaitForSave: true);
        Assert.True(cleanup.ClearInSave && cleanup.UnfreezeTime);
    }

    [Fact]
    public void SavePicAndComment()
    {
        Assert.True(VmGGameSaveFlow1004Slice.ShouldWriteDummySavePic(
            width: 0,
            height: 100,
            storeSavePic: true));
        var elapsed = VmGGameSaveFlow1004Slice.FormatElapsedLevelTime(
            levelTimeTics: 35 * 3661,
            ticRate: 35);
        Assert.Equal(1, elapsed.Hours);
        Assert.Equal(1, elapsed.Minutes);
        Assert.Equal(1, elapsed.Seconds);
        Assert.Equal("quick02", VmGGameSaveFlow1004Slice.FormatQuicksaveFileStem(2));
        Assert.True(VmGGameSaveFlow1004Slice.UsesCurrentSaveVersionConstant(
            VmGGameSaveFlow1004Slice.CurrentSaveVersion,
            VmGGameSaveFlow1004Slice.CurrentSaveVersion));
    }
}
