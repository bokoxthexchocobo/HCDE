namespace HCDE.Playsim;

/// <summary>
/// <c>HandleReply</c> <c>NextNode</c> tail, teardown, <c>P_ConversationCommand</c>, and field-export head from <c>p_conversation.cpp</c> (**601–750**).
/// </summary>
public static class VmPConversationNextNodeCommand976Slice
{
    public const int Mf5Inconversation = 1 << 20;
    public const int Mf8Dontfacetalker = 1;
    public const int DemConvReply = 59;
    public const int DemConvClose = 60;
    public const int DemConvNull = 61;
    public const int StrifeDialogueNodeFieldExportCount = 12;
    public const int StrifeDialogueReplyFieldExportCountInBand = 9;

    public static bool HasNextNodeTransition(int nextNode) => nextNode != 0;

    public static int ComputeNextDialogueIndex(int conversationRoot, int nextNode) =>
        conversationRoot + nextNode - 1;

    public static bool NextDialogueIndexInRange(int nextIndex, int dialogueCount) =>
        nextIndex >= 0 && nextIndex < dialogueCount;

    public static bool ShouldChainConversationAfterNextNode(bool closeDialog, bool intermissionActive) =>
        !closeDialog && !intermissionActive;

    public static bool ShouldStopNpcVoiceDuringIntermission(bool closeDialog, bool intermissionActive) =>
        !closeDialog && intermissionActive;

    public static bool InvalidNextNodeMessageIncludesName(bool nextNodeNameEmpty) =>
        !nextNodeNameEmpty;

    public static int ClearInConversationFlag(int flags5) => flags5 & ~Mf5Inconversation;

    public static bool ShouldRestoreNpcAngleAfterReply(int npcFlags8) =>
        (npcFlags8 & Mf8Dontfacetalker) == 0;

    public static bool ShouldClearConversationAfterReply(bool intermissionActive) =>
        !intermissionActive;

    public static bool ShouldRestoreConsoleMusicVolume(bool isConsole) => isConsole;

    public static bool DemoPlaybackClosesConversationMenu(bool demoPlayback, bool menuIsConversationKind) =>
        demoPlayback && menuIsConversationKind;

    public static bool NetcodeDispatchesHandleReply(int netcode) => netcode == DemConvReply;

    public static bool ConvReplyFromConsolePlayer(int playerNum, int consolePlayerNum) =>
        playerNum == consolePlayerNum;

    public static bool NetcodeIsConvNullOrClose(int netcode) =>
        netcode == DemConvNull || netcode == DemConvClose;

    public static bool NullNetcodeClearsConversationPointers(int netcode) => netcode == DemConvNull;

    public static bool TerminalResponseHasText(string? text) => !string.IsNullOrEmpty(text);

    public static bool TerminalStringUsesLookupTable(string text) => text[0] == '$';

    public static bool TerminalResponsePrefersStatusBar(object? statusBar, bool hasStatusBar) =>
        hasStatusBar && statusBar is not null;
}
