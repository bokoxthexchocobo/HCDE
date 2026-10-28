namespace HCDE.Playsim;

/// <summary>
/// Strife conversation registry, type lookup, node teardown, and <c>FindNode</c> from <c>p_conversation.cpp</c> (**1–150**).
/// </summary>
public static class VmPConversationHead984Slice
{
    public const int InvalidConversationId = -1;

    public static bool ShouldRegisterConversationRoot(int convId) => convId != InvalidConversationId;

    public static bool TryRegisterConversationRoots(
        int convId,
        object? actorClass,
        Action<int, int>? setDialogueRoot,
        Action<object?, int>? setClassRoot,
        int dlgIndex,
        out int rootsWritten)
    {
        rootsWritten = 0;
        if (ShouldRegisterConversationRoot(convId))
        {
            setDialogueRoot?.Invoke(convId, dlgIndex);
            rootsWritten++;
        }

        if (actorClass is not null)
        {
            setClassRoot?.Invoke(actorClass, dlgIndex);
            rootsWritten++;
        }

        return rootsWritten > 0;
    }

    public static object? ResolveStrifeTypeFromTable(
        int typeNum,
        Func<int, object?>? lookupType)
    {
        if (lookupType is null)
            return null;
        return lookupType(typeNum);
    }

    public static int ResolveConversationIndexById(
        int convId,
        Func<int, int?>? lookupRoot)
    {
        if (lookupRoot is null)
            return InvalidConversationId;
        return lookupRoot(convId) ?? InvalidConversationId;
    }

    public static int ResolveConversationIndexByClassName(
        object className,
        Func<object, int?>? lookupRoot)
    {
        if (lookupRoot is null)
            return InvalidConversationId;
        return lookupRoot(className) ?? InvalidConversationId;
    }

    public static int FreeDialogueReplyChain(object? head, Func<object?, object?> getNext)
    {
        var freed = 0;
        var tokill = head;
        while (tokill is not null)
        {
            var next = getNext(tokill);
            freed++;
            tokill = next;
        }

        return freed;
    }

    public static int FindDialogueNodeIndex(
        object? targetNode,
        Func<int, object?> getNodeAtIndex,
        int startIndex = 0)
    {
        if (targetNode is null || getNodeAtIndex is null)
            return InvalidConversationId;

        var rootNode = startIndex;
        while (getNodeAtIndex(rootNode) is not null)
        {
            if (ReferenceEquals(getNodeAtIndex(rootNode), targetNode))
                return rootNode;
            rootNode++;
        }

        return InvalidConversationId;
    }
}
