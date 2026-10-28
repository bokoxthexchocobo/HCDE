namespace HCDE.Playsim.Tests;

public class VmPConversationHead984SliceTests
{
    [Fact]
    public void SetConversation_RegistersRoots()
    {
        var dialogueRoots = new Dictionary<int, int>();
        var classRoots = new Dictionary<object, int>();
        var actorClass = new object();

        Assert.True(VmPConversationHead984Slice.TryRegisterConversationRoots(
            convId: 3,
            actorClass,
            (id, idx) => dialogueRoots[id] = idx,
            (cls, idx) => classRoots[cls!] = idx,
            dlgIndex: 7,
            out var written));
        Assert.Equal(2, written);
        Assert.Equal(7, dialogueRoots[3]);
        Assert.Equal(7, classRoots[actorClass]);
    }

    [Fact]
    public void ConversationLookups_MissReturnsInvalid()
    {
        Assert.Null(VmPConversationHead984Slice.ResolveStrifeTypeFromTable(1, _ => null));
        Assert.Equal(
            VmPConversationHead984Slice.InvalidConversationId,
            VmPConversationHead984Slice.ResolveConversationIndexById(9, _ => null));
        Assert.Equal(
            VmPConversationHead984Slice.InvalidConversationId,
            VmPConversationHead984Slice.ResolveConversationIndexByClassName("npc", _ => null));
    }

    [Fact]
    public void ReplyChainAndFindNode()
    {
        var n2 = new object();
        var n1 = new object();
        var chain = new Dictionary<object, object?> { [n1] = n2, [n2] = null };

        Assert.Equal(2, VmPConversationHead984Slice.FreeDialogueReplyChain(
            n1,
            node => node is null ? null : chain.GetValueOrDefault(node)));

        var nodes = new List<object?> { new object(), new object(), new object() };
        var target = nodes[1];
        var index = VmPConversationHead984Slice.FindDialogueNodeIndex(
            target,
            i => i < nodes.Count ? nodes[i] : null);
        Assert.Equal(1, index);
    }
}
