namespace HCDE.Playsim.Tests;

public class VmPConversationHandleReply975SliceTests
{
    [Fact]
    public void HandleReplyShouldAbort_InvalidNodeOrNpc()
    {
        Assert.True(VmPConversationHandleReply975Slice.HandleReplyShouldAbort(null, 0, 5));
        Assert.True(VmPConversationHandleReply975Slice.HandleReplyShouldAbort(new object(), 5, 5));
        Assert.False(VmPConversationHandleReply975Slice.HandleReplyShouldAbort(new object(), 2, 5));
    }

    [Fact]
    public void AdvanceToReplyChild_WalksLinkedReplies()
    {
        var r0 = new object();
        var r1 = new object();
        var (found, reply) = VmPConversationHandleReply975Slice.AdvanceToReplyChild(
            r0,
            1,
            cur => ReferenceEquals(cur, r0) ? r1 : null);
        Assert.True(found);
        Assert.Same(r1, reply);
        Assert.True(VmPConversationHandleReply975Slice.IsDefaultReplyPath(
            VmPConversationHandleReply975Slice.AdvanceToReplyChild(r0, 2, _ => null).Reply));
    }

    [Fact]
    public void ReplyItemCheckPasses_AllRequiredPresent()
    {
        Assert.True(VmPConversationHandleReply975Slice.ReplyItemCheckPasses(
            2,
            i => i == 0 ? ("a", 1) : ("b", 2),
            (item, amt) => item as string == "a" || (item as string == "b" && amt == 2)));
        Assert.False(VmPConversationHandleReply975Slice.ReplyItemCheckPasses(
            1,
            _ => ("x", 1),
            (_, _) => false));
    }

    [Fact]
    public void WeaponAlreadyOwnedBlocksGive_WhenDuplicateWeapon()
    {
        Assert.True(VmPConversationHandleReply975Slice.WeaponAlreadyOwnedBlocksGive(true, true));
        Assert.False(VmPConversationHandleReply975Slice.WeaponAlreadyOwnedBlocksGive(true, false));
    }

    [Fact]
    public void FlameThrowerDialogAmmoGive_OnlyForFlameThrower()
    {
        Assert.Equal(40, VmPConversationHandleReply975Slice.FlameThrowerDialogAmmoGive("FlameThrower"));
        Assert.Equal(-1, VmPConversationHandleReply975Slice.FlameThrowerDialogAmmoGive("Pistol"));
    }

    [Fact]
    public void CombineTakeStuffWithSpecial_OrSemantics()
    {
        Assert.True(VmPConversationHandleReply975Slice.CombineTakeStuffWithSpecial(false, true));
        Assert.False(VmPConversationHandleReply975Slice.CombineTakeStuffWithSpecial(false, false));
    }

    [Fact]
    public void SelectTerminalReplyText_PicksHaveEnoughWhenTakeFails()
    {
        Assert.Equal("yes", VmPConversationHandleReply975Slice.SelectTerminalReplyText(true, "yes", "$x"));
        Assert.Equal(
            VmPConversationHandleReply975Slice.HaveEnoughTextKey,
            VmPConversationHandleReply975Slice.SelectTerminalReplyText(false, "yes", VmPConversationHandleReply975Slice.HaveEnoughTextKey));
    }

    [Fact]
    public void ResolveLogString_ExpandsDollarPrefix()
    {
        var resolved = VmPConversationHandleReply975Slice.ResolveLogString(
            "$foo",
            key => key == "foo" ? "bar" : null);
        Assert.Equal("bar", resolved);
    }

    [Fact]
    public void ShouldTerminalReply_RequiresConsoleAndText()
    {
        Assert.True(VmPConversationHandleReply975Slice.ShouldTerminalReply("hi", true));
        Assert.False(VmPConversationHandleReply975Slice.ShouldTerminalReply("hi", false));
        Assert.False(VmPConversationHandleReply975Slice.ShouldTerminalReply("", true));
    }
}
