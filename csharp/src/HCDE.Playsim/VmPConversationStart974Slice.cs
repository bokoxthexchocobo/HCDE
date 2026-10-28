namespace HCDE.Playsim;

/// <summary>
/// <c>P_StartConversation</c>, <c>P_ResumeConversation</c> head, and menu-open policy from <c>p_conversation.cpp</c> (**301–450**).
/// </summary>
public static class VmPConversationStart974Slice
{
    public const int Mf5Inconversation = 1 << 20;
    public const int MfFriendly = 1;
    public const int Mf4Nohateplayers = 1;
    public const int Mf8Dontfacetalker = 1;

    public static bool StartConversationActorsValid(
        object? pc,
        bool pcHasPlayer,
        object? npc,
        bool isPrimaryLevel) =>
        pc is not null && pcHasPlayer && npc is not null && isPrimaryLevel;

    public static bool NpcBusyWithOtherPlayer(object? npc, object? otherConversationNpc) =>
        otherConversationNpc is not null && ReferenceEquals(npc, otherConversationNpc);

    public static int ApplyInConversationFlag(int flags5) => flags5 | Mf5Inconversation;

    public static bool ShouldFaceTalker(bool faceTalker, int npcFlags8) =>
        faceTalker && (npcFlags8 & Mf8Dontfacetalker) == 0;

    public static bool ShouldRestoreNpcOldTarget(int npcFlags, int npcFlags4) =>
        (npcFlags & MfFriendly) != 0 || (npcFlags4 & Mf4Nohateplayers) != 0;

    public static int? TryResolveItemCheckDialogueIndex(
        int itemCheckCount,
        Func<int, (object? Item, int Amount)> getItemCheck,
        Func<object?, int, bool> hasItemOnPlayer,
        int itemCheckNode,
        int conversationRoot,
        Func<int, int> dialogueIndexFromRootOffset)
    {
        if (itemCheckCount <= 0)
            return null;
        if (getItemCheck(0).Item is null)
            return null;

        for (var i = 0; i < itemCheckCount; i++)
        {
            var check = getItemCheck(i);
            if (!hasItemOnPlayer(check.Item, check.Amount))
                return null;
        }

        if (itemCheckNode <= 0)
            return null;

        return dialogueIndexFromRootOffset(conversationRoot + itemCheckNode - 1);
    }

    public static bool ShouldPlayPositionalSpeakerVoice(int speakerVoice, int noSound) =>
        speakerVoice != noSound;

    public static bool ShouldLowerMusicForSpeakerVoice(int speakerVoice, int noSound) =>
        speakerVoice != noSound;

    public static string ResolveMenuClassName(string? nodeMenuClass, string? defaultMenuClass)
    {
        if (!string.IsNullOrEmpty(nodeMenuClass))
            return nodeMenuClass!;
        if (!string.IsNullOrEmpty(defaultMenuClass))
            return defaultMenuClass!;
        return "ConversationMenu";
    }

    public static bool MenuClassMustBeConversationMenu(bool classFound, bool isConversationDescendant) =>
        !classFound || !isConversationDescendant;

    public static bool ShouldResetStaticLastReply(object? curNode, object? prevNode) =>
        !ReferenceEquals(curNode, prevNode);

    public static bool ResumeConversationShouldRestart(object? conversationPc, object? conversationNpc) =>
        conversationPc is not null && conversationNpc is not null;
}
