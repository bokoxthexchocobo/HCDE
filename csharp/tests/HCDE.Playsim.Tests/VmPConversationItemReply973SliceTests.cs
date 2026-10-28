namespace HCDE.Playsim.Tests;

public class VmPConversationItemReply973SliceTests
{
    [Fact]
    public void ClearConversationPointers_ResetsAllFields()
    {
        var face = true;
        object? npc = new object();
        object? pc = new object();
        var angle = 90.0;
        VmPConversationItemReply973Slice.ClearConversationPointers(
            v => face = v,
            v => npc = v,
            v => pc = v,
            v => angle = v);
        Assert.False(face);
        Assert.Null(npc);
        Assert.Null(pc);
        Assert.Equal(0, angle);
    }

    [Fact]
    public void CheckStrifeItem_NullTypeAlwaysPasses()
    {
        Assert.True(VmPConversationItemReply973Slice.CheckStrifeItem(null, 5, _ => 0, new object()));
    }

    [Fact]
    public void CheckStrifeItem_RequiresAmountOnPc()
    {
        var pc = new object();
        Assert.False(VmPConversationItemReply973Slice.CheckStrifeItem(
            "key",
            3,
            _ => 2,
            pc));
        Assert.True(VmPConversationItemReply973Slice.CheckStrifeItem(
            "key",
            3,
            _ => 3,
            pc));
    }

    [Fact]
    public void TakeStrifeItemBlocked_SkipsQuestKeySigil()
    {
        Assert.True(VmPConversationItemReply973Slice.TakeStrifeItemBlocked("x", 1, isQuestItem: true, isKey: false, isSigil: false));
        Assert.True(VmPConversationItemReply973Slice.TakeStrifeItemBlocked("x", 1, isQuestItem: false, isKey: true, isSigil: false));
        Assert.True(VmPConversationItemReply973Slice.TakeStrifeItemBlocked("x", 1, isQuestItem: false, isKey: false, isSigil: true));
        Assert.False(VmPConversationItemReply973Slice.TakeStrifeItemBlocked("x", 1, false, false, false));
    }

    [Fact]
    public void ClampDlgMusicVolume_ClampsToUnitInterval()
    {
        Assert.Equal(0f, VmPConversationItemReply973Slice.ClampDlgMusicVolume(-0.5f));
        Assert.Equal(1f, VmPConversationItemReply973Slice.ClampDlgMusicVolume(2f));
        Assert.Equal(0.25f, VmPConversationItemReply973Slice.ClampDlgMusicVolume(0.25f));
    }

    [Fact]
    public void ShouldSkipReply_EmptyTextOrFailedChecks()
    {
        Assert.True(VmPConversationItemReply973Slice.ShouldSkipReply(
            replyTextEmpty: true,
            requireChecks: Array.Empty<(object?, int)>(),
            excludeChecks: Array.Empty<(object?, int)>(),
            hasItem: (_, _) => true));

        var require = new List<(object?, int)> { ("a", 1) };
        Assert.True(VmPConversationItemReply973Slice.ShouldSkipReply(
            false,
            require,
            Array.Empty<(object?, int)>(),
            (_, _) => false));

        var exclude = new List<(object?, int)> { ("b", 1) };
        Assert.True(VmPConversationItemReply973Slice.ShouldSkipReply(
            false,
            Array.Empty<(object?, int)>(),
            exclude,
            (_, _) => true));

        Assert.False(VmPConversationItemReply973Slice.ShouldSkipReply(
            false,
            require,
            exclude,
            (item, _) => item as string != "b"));
    }

    [Fact]
    public void SelectReplyWireKind_MapsDemConversationOpcodes()
    {
        Assert.Equal(
            VmPConversationItemReply973Slice.ConversationReplyWireKind.Null,
            VmPConversationItemReply973Slice.SelectReplyWireKind(VmPConversationItemReply973Slice.DemConvNull));
        Assert.Equal(
            VmPConversationItemReply973Slice.ConversationReplyWireKind.Close,
            VmPConversationItemReply973Slice.SelectReplyWireKind(VmPConversationItemReply973Slice.DemConvClose));
        Assert.Equal(
            VmPConversationItemReply973Slice.ConversationReplyWireKind.Reply,
            VmPConversationItemReply973Slice.SelectReplyWireKind(3));
        Assert.True(VmPConversationItemReply973Slice.ReplyWireWritesNodeAndReply(
            VmPConversationItemReply973Slice.ConversationReplyWireKind.Reply));
    }

    [Fact]
    public void RememberStaticLastReply_ReturnsReplyIndex() =>
        Assert.Equal(7, VmPConversationItemReply973Slice.RememberStaticLastReply(7));
}
