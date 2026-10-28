namespace HCDE.Playsim.Tests;

public class VmGGameSavePayload1005SliceTests
{
    [Fact]
    public void SaveGlobalsAndArchive()
    {
        Assert.False(VmGGameSavePayload1005Slice.ShouldSerializeAllowLoadInPorts(0));
        Assert.True(VmGGameSavePayload1005Slice.ShouldIncludeLevelTimingInGlobals(
            levelTime: 0,
            mapTime: 35));
        Assert.True(VmGGameSavePayload1005Slice.ShouldSerializeNextSkillGlobal(nextSkill: 2));
        Assert.Equal(
            VmGGameSavePayload1005Slice.SaveSuccessNotificationStyle.WithFilename,
            VmGGameSavePayload1005Slice.ResolveSaveSuccessNotification(longSaveMessages: true));
        Assert.True(VmGGameSavePayload1005Slice.ConfirmsSavedArchiveIntegrity(
            writeZipSucceeded: true,
            testOpenSucceeded: true));
        Assert.Equal(new[] { 1, 2 }, VmGGameSavePayload1005Slice.JsonContentCleanupIndices());
        var cleanup = VmGGameSavePayload1005Slice.ResolveSaveCompletionCleanup(clWaitForSave: true);
        Assert.True(cleanup.ClearInSave && cleanup.UnfreezeTime);
    }

    [Fact]
    public void DemoTiccmdHead()
    {
        Assert.True(VmGGameSavePayload1005Slice.ShouldEndDemoPlaybackAtBodyEnd(
            demoRecording: false,
            atOrPastBodyEnd: true));
        Assert.True(VmGGameSavePayload1005Slice.IsDemoUserCommandPayload(
            VmGGameSavePayload1005Slice.DemoUserCmd));
        Assert.True(VmGGameSavePayload1005Slice.ShouldDropPlayerFromDemoCommand(
            VmGGameSavePayload1005Slice.DemoDropPlayer,
            playerIndex: 3));
        Assert.False(VmGGameSavePayload1005Slice.ShouldDropPlayerFromDemoCommand(
            VmGGameSavePayload1005Slice.DemoDropPlayer,
            playerIndex: 8));
        Assert.True(VmGGameSavePayload1005Slice.ShouldCheckDemoStatusOnStop(
            VmGGameSavePayload1005Slice.DemoStop));
        Assert.True(VmGGameSavePayload1005Slice.ShouldContinueDemoCommandLoop(
            VmGGameSavePayload1005Slice.DemoBad));
        Assert.Equal(3, VmGGameSavePayload1005Slice.ExpectedSaveArchiveEntryCount());
        Assert.Equal(
            VmGGameSavePayload1005Slice.DemoReadExitReason.UserCommandReady,
            VmGGameSavePayload1005Slice.ResolveDemoReadExitReason(
                demoRecording: false,
                atOrPastBodyEnd: false,
                commandId: VmGGameSavePayload1005Slice.DemoEmptyUserCmd));
        Assert.True(VmGGameSavePayload1005Slice.ShouldDispatchDemoCommandToNet(
            VmGGameSavePayload1005Slice.DemoBad));
    }
}
