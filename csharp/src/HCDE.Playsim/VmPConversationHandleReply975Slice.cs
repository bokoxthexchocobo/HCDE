namespace HCDE.Playsim;

/// <summary>
/// <c>HandleReply</c> validation, item gates, give/take policy, and log/terminal hooks from <c>p_conversation.cpp</c> (**451–600**).
/// </summary>
public static class VmPConversationHandleReply975Slice
{
    public const int Mf5Inconversation = 1 << 20;
    public const int Mf8Dontfacetalker = 1;
    public const string HaveEnoughTextKey = "$txt_haveenough";

    public static bool HandleReplyShouldAbort(object? conversationNpc, int nodeNum, int dialogueCount) =>
        conversationNpc is null || nodeNum < 0 || nodeNum >= dialogueCount;

    public static (bool Found, object? Reply) AdvanceToReplyChild(
        object? head,
        int replyIndex,
        Func<object?, object?> getNext)
    {
        var reply = head;
        var i = 0;
        while (reply is not null && i != replyIndex)
        {
            reply = getNext(reply);
            i++;
        }

        return (reply is not null, reply);
    }

    public static bool IsDefaultReplyPath(object? reply) => reply is null;

    public static int ClearInConversationFlag(int flags5) => flags5 & ~Mf5Inconversation;

    public static bool ShouldRestoreNpcAngle(int npcFlags8) => (npcFlags8 & Mf8Dontfacetalker) == 0;

    public static bool ShouldClearConversationState(int gameActionIntermission) =>
        gameActionIntermission == 0;

    public static bool ReplyItemCheckPasses(
        int checkCount,
        Func<int, (object? Item, int Amount)> getCheck,
        Func<object?, int, bool> hasItemOnPlayer)
    {
        for (var i = 0; i < checkCount; i++)
        {
            var check = getCheck(i);
            if (!hasItemOnPlayer(check.Item, check.Amount))
                return false;
        }

        return true;
    }

    public static bool ShouldShowQuickNo(bool isConsole, bool quickNoNotEmpty) =>
        isConsole && quickNoNotEmpty;

    public static bool ShouldAttemptInventoryGive(object? giveType) => giveType is not null;

    public static bool WeaponAlreadyOwnedBlocksGive(bool giveIsWeapon, bool playerHasWeapon) =>
        giveIsWeapon && playerHasWeapon;

    public static int FlameThrowerDialogAmmoGive(string typeName) =>
        typeName == "FlameThrower" ? 40 : -1;

    public static bool CombineTakeStuffWithSpecial(bool takeStuff, bool specialSucceeded) =>
        takeStuff || specialSucceeded;

    public static string SelectTerminalReplyText(bool takeStuff, string quickYes, string haveEnoughKey) =>
        takeStuff ? quickYes : haveEnoughKey;

    public static string? ResolveLogString(string? logString, Func<string, string?> lookupStringTable)
    {
        if (string.IsNullOrEmpty(logString))
            return null;
        if (logString[0] != '$')
            return logString;
        return lookupStringTable(logString[1..]);
    }

    public static bool ShouldSetLogNumber(string? logString, int logNumber) =>
        string.IsNullOrEmpty(logString) && logNumber != 0;

    public static bool ShouldTerminalReply(string? replyText, bool isConsole) =>
        isConsole && !string.IsNullOrEmpty(replyText);
}
