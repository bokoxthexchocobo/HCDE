namespace HCDE.Playsim.Tests;

public class VmPConversationStart974SliceTests
{
    [Fact]
    public void StartConversationActorsValid_RejectsNullOrNonPrimary()
    {
        var pc = new object();
        Assert.False(VmPConversationStart974Slice.StartConversationActorsValid(null, true, pc, true));
        Assert.False(VmPConversationStart974Slice.StartConversationActorsValid(pc, false, pc, true));
        Assert.False(VmPConversationStart974Slice.StartConversationActorsValid(pc, true, null, true));
        Assert.False(VmPConversationStart974Slice.StartConversationActorsValid(pc, true, pc, false));
        Assert.True(VmPConversationStart974Slice.StartConversationActorsValid(pc, true, pc, true));
    }

    [Fact]
    public void NpcBusyWithOtherPlayer_MatchesReference()
    {
        var npc = new object();
        Assert.True(VmPConversationStart974Slice.NpcBusyWithOtherPlayer(npc, npc));
        Assert.False(VmPConversationStart974Slice.NpcBusyWithOtherPlayer(npc, new object()));
        Assert.False(VmPConversationStart974Slice.NpcBusyWithOtherPlayer(npc, null));
    }

    [Fact]
    public void TryResolveItemCheckDialogueIndex_JumpsWhenAllItemsPresent()
    {
        var idx = VmPConversationStart974Slice.TryResolveItemCheckDialogueIndex(
            2,
            i => i == 0 ? ("a", 1) : ("b", 2),
            (item, amt) => item as string == "a" || (item as string == "b" && amt == 2),
            itemCheckNode: 3,
            conversationRoot: 10,
            offset => offset);
        Assert.Equal(12, idx);
    }

    [Fact]
    public void TryResolveItemCheckDialogueIndex_StopsWhenItemMissing()
    {
        var idx = VmPConversationStart974Slice.TryResolveItemCheckDialogueIndex(
            1,
            _ => ("key", 1),
            (_, _) => false,
            1,
            0,
            x => x);
        Assert.Null(idx);
    }

    [Fact]
    public void ShouldFaceTalker_RespectsDontFaceTalkerFlag()
    {
        Assert.True(VmPConversationStart974Slice.ShouldFaceTalker(true, 0));
        Assert.False(VmPConversationStart974Slice.ShouldFaceTalker(
            true,
            VmPConversationStart974Slice.Mf8Dontfacetalker));
    }

    [Fact]
    public void ResolveMenuClassName_FallsBackToConversationMenu()
    {
        Assert.Equal("Custom", VmPConversationStart974Slice.ResolveMenuClassName("Custom", null));
        Assert.Equal("GameDefault", VmPConversationStart974Slice.ResolveMenuClassName(null, "GameDefault"));
        Assert.Equal("ConversationMenu", VmPConversationStart974Slice.ResolveMenuClassName(null, null));
    }

    [Fact]
    public void ShouldResetStaticLastReply_WhenNodeChanges()
    {
        var a = new object();
        var b = new object();
        Assert.True(VmPConversationStart974Slice.ShouldResetStaticLastReply(b, a));
        Assert.False(VmPConversationStart974Slice.ShouldResetStaticLastReply(a, a));
    }

    [Fact]
    public void ResumeConversationShouldRestart_RequiresBothPointers()
    {
        var a = new object();
        Assert.False(VmPConversationStart974Slice.ResumeConversationShouldRestart(null, a));
        Assert.True(VmPConversationStart974Slice.ResumeConversationShouldRestart(a, a));
    }
}
