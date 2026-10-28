namespace HCDE.Playsim;

/// <summary>
/// `G_DoSaveGame` payload assembly, zip verify, and `G_ReadDemoTiccmd` head from <c>g_game.cpp</c> (**3050–3199**).
/// </summary>
public static class VmGGameSavePayload1005Slice
{
    public const int MaxPlayers = 8;
    public const int NextSkillUnset = -1;
    public const int DemoBad = 0;
    public const int DemoUserCmd = 1;
    public const int DemoEmptyUserCmd = 2;
    public const int DemoStop = 7;
    public const int DemoDropPlayer = 13;

    public enum SaveSuccessNotificationStyle
    {
        Short,
        WithFilename,
    }

    public enum DemoReadExitReason
    {
        ContinueLoop,
        UserCommandReady,
        EndOfBody,
    }

    public readonly struct SaveCompletionCleanup
    {
        public bool ClearInSave { get; init; }
        public bool UnfreezeTime { get; init; }
    }

    public static bool ShouldSerializeAllowLoadInPorts(int allowLoadInCount) =>
        allowLoadInCount > 0;

    public static bool ShouldIncludeLevelTimingInGlobals(int levelTime, int mapTime) =>
        levelTime != 0 || mapTime != 0;

    public static bool ShouldSerializeNextSkillGlobal(int nextSkill) =>
        nextSkill != NextSkillUnset;

    public static SaveSuccessNotificationStyle ResolveSaveSuccessNotification(bool longSaveMessages) =>
        longSaveMessages
            ? SaveSuccessNotificationStyle.WithFilename
            : SaveSuccessNotificationStyle.Short;

    public static bool ConfirmsSavedArchiveIntegrity(bool writeZipSucceeded, bool testOpenSucceeded) =>
        writeZipSucceeded && testOpenSucceeded;

    public static int[] JsonContentCleanupIndices() => [1, 2];

    public static SaveCompletionCleanup ResolveSaveCompletionCleanup(bool clWaitForSave) =>
        new() { ClearInSave = true, UnfreezeTime = clWaitForSave };

    public static bool ShouldEndDemoPlaybackAtBodyEnd(bool demoRecording, bool atOrPastBodyEnd) =>
        !demoRecording && atOrPastBodyEnd;

    public static bool IsDemoUserCommandPayload(int commandId) =>
        commandId == DemoUserCmd || commandId == DemoEmptyUserCmd;

    public static bool ShouldDropPlayerFromDemoCommand(int commandId, byte playerIndex) =>
        commandId == DemoDropPlayer && playerIndex < MaxPlayers;

    public static bool ShouldCheckDemoStatusOnStop(int commandId) =>
        commandId == DemoStop;

    public static bool ShouldContinueDemoCommandLoop(int commandId) =>
        !IsDemoUserCommandPayload(commandId);

    public static IReadOnlyList<string> OrderedSaveArchiveLeafNames() =>
        ["savepic.png", "info.json", "globals.json"];

    public static int ExpectedSaveArchiveEntryCount() =>
        OrderedSaveArchiveLeafNames().Count;

    public static DemoReadExitReason ResolveDemoReadExitReason(
        bool demoRecording,
        bool atOrPastBodyEnd,
        int commandId)
    {
        if (IsDemoUserCommandPayload(commandId))
            return DemoReadExitReason.UserCommandReady;
        if (ShouldEndDemoPlaybackAtBodyEnd(demoRecording, atOrPastBodyEnd))
            return DemoReadExitReason.EndOfBody;
        return DemoReadExitReason.ContinueLoop;
    }

    public static bool ShouldDispatchDemoCommandToNet(int commandId) =>
        commandId != DemoUserCmd
        && commandId != DemoEmptyUserCmd
        && commandId != DemoStop
        && commandId != DemoDropPlayer;
}
