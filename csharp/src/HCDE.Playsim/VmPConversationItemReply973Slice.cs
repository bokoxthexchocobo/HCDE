namespace HCDE.Playsim;

/// <summary>
/// Strife conversation item checks, reply filtering, and net reply encoding from <c>p_conversation.cpp</c> (**151–300**).
/// </summary>
public static class VmPConversationItemReply973Slice
{
    public const int DemConvNull = -1;
    public const int DemConvClose = -2;

    public static void ClearConversationPointers(
        Action<bool>? setFaceTalker,
        Action<object?>? setNpc,
        Action<object?>? setPc,
        Action<double>? setNpcAngle,
        double nullAngle = 0)
    {
        setFaceTalker?.Invoke(false);
        setNpc?.Invoke(null);
        setPc?.Invoke(null);
        setNpcAngle?.Invoke(nullAngle);
    }

    public static bool CheckStrifeItem(
        object? itemType,
        int requiredAmount,
        Func<object?, int>? amountOnPc,
        object? conversationPc)
    {
        if (itemType is null || requiredAmount == 0)
            return true;
        if (conversationPc is null)
            return false;
        var amount = amountOnPc?.Invoke(itemType) ?? -1;
        if (amount < 0)
            return false;
        return requiredAmount < 0 || amount >= requiredAmount;
    }

    public static bool TakeStrifeItemBlocked(object? itemType, int amount, bool isQuestItem, bool isKey, bool isSigil)
    {
        if (itemType is null || amount == 0)
            return true;
        return isQuestItem || isKey || isSigil;
    }

    public static float ClampDlgMusicVolume(float value)
    {
        if (value < 0f)
            return 0f;
        return value > 1f ? 1f : value;
    }

    public static bool ShouldSkipReply(
        bool replyTextEmpty,
        IReadOnlyList<(object? Item, int Amount)> requireChecks,
        IReadOnlyList<(object? Item, int Amount)> excludeChecks,
        Func<object?, int, bool> hasItem)
    {
        if (replyTextEmpty)
            return true;

        foreach (var (item, amount) in requireChecks)
        {
            if (!hasItem(item, amount))
                return true;
        }

        foreach (var (item, amount) in excludeChecks)
        {
            if (hasItem(item, amount))
                return true;
        }

        return false;
    }

    public enum ConversationReplyWireKind
    {
        Null,
        Close,
        Reply
    }

    public static ConversationReplyWireKind SelectReplyWireKind(int node) =>
        node switch
        {
            DemConvNull => ConversationReplyWireKind.Null,
            DemConvClose => ConversationReplyWireKind.Close,
            _ => ConversationReplyWireKind.Reply
        };

    public static bool ReplyWireWritesNodeAndReply(ConversationReplyWireKind kind) =>
        kind == ConversationReplyWireKind.Reply;

    public static bool ShouldAssignStaticLastReply(ConversationReplyWireKind kind) =>
        kind == ConversationReplyWireKind.Reply;

    public static bool FreeConversationsShouldCloseMenu(bool menuIsConversationKind) => menuIsConversationKind;
}
