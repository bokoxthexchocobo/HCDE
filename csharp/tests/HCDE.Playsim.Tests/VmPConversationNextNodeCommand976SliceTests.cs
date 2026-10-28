namespace HCDE.Playsim.Tests;

public class VmPConversationNextNodeCommand976SliceTests
{
    [Fact]
    public void NextNodeTransition_ComputesDialogueIndex()
    {
        Assert.False(VmPConversationNextNodeCommand976Slice.HasNextNodeTransition(0));
        var next = VmPConversationNextNodeCommand976Slice.ComputeNextDialogueIndex(10, 3);
        Assert.Equal(12, next);
        Assert.True(VmPConversationNextNodeCommand976Slice.NextDialogueIndexInRange(12, 20));
        Assert.False(VmPConversationNextNodeCommand976Slice.NextDialogueIndexInRange(25, 20));
    }

    [Fact]
    public void ShouldChainConversationAfterNextNode_RespectsCloseAndIntermission()
    {
        Assert.True(VmPConversationNextNodeCommand976Slice.ShouldChainConversationAfterNextNode(
            closeDialog: false,
            intermissionActive: false));
        Assert.False(VmPConversationNextNodeCommand976Slice.ShouldChainConversationAfterNextNode(
            true,
            false));
        Assert.True(VmPConversationNextNodeCommand976Slice.ShouldStopNpcVoiceDuringIntermission(
            false,
            true));
    }

    [Fact]
    public void ConversationCommand_RoutesNetcodes()
    {
        Assert.True(VmPConversationNextNodeCommand976Slice.NetcodeDispatchesHandleReply(
            VmPConversationNextNodeCommand976Slice.DemConvReply));
        Assert.True(VmPConversationNextNodeCommand976Slice.NetcodeIsConvNullOrClose(
            VmPConversationNextNodeCommand976Slice.DemConvNull));
        Assert.True(VmPConversationNextNodeCommand976Slice.NullNetcodeClearsConversationPointers(
            VmPConversationNextNodeCommand976Slice.DemConvNull));
        Assert.True(VmPConversationNextNodeCommand976Slice.ConvReplyFromConsolePlayer(0, 0));
    }

    [Fact]
    public void DemoPlaybackClosesConversationMenu_WhenBothFlags()
    {
        Assert.True(VmPConversationNextNodeCommand976Slice.DemoPlaybackClosesConversationMenu(true, true));
        Assert.False(VmPConversationNextNodeCommand976Slice.DemoPlaybackClosesConversationMenu(false, true));
    }

    [Fact]
    public void TerminalResponse_StringTablePrefix()
    {
        Assert.True(VmPConversationNextNodeCommand976Slice.TerminalResponseHasText("$foo"));
        Assert.True(VmPConversationNextNodeCommand976Slice.TerminalStringUsesLookupTable("$foo"));
        Assert.True(VmPConversationNextNodeCommand976Slice.TerminalResponsePrefersStatusBar(new object(), true));
    }

    [Fact]
    public void FieldExportCounts_MatchNativeBand()
    {
        Assert.Equal(12, VmPConversationNextNodeCommand976Slice.StrifeDialogueNodeFieldExportCount);
        Assert.Equal(9, VmPConversationNextNodeCommand976Slice.StrifeDialogueReplyFieldExportCountInBand);
    }

    [Fact]
    public void TryResolveNextDialogueAssignment_MatchesNativeBranches()
    {
        Assert.True(VmPConversationNextNodeCommand976Slice.TryResolveNextDialogueAssignment(
            nextNode: 2,
            conversationRoot: 5,
            dialogueCount: 10,
            closeDialog: false,
            intermissionActive: false,
            out var index));
        Assert.Equal(6, index);

        Assert.False(VmPConversationNextNodeCommand976Slice.TryResolveNextDialogueAssignment(
            2,
            5,
            6,
            false,
            false,
            out _));
    }

    [Fact]
    public void TerminalStringTable_AndCommandElseBranch()
    {
        Assert.Equal("foo", VmPConversationNextNodeCommand976Slice.StripTerminalStringTablePrefix("$foo"));
        Assert.True(VmPConversationNextNodeCommand976Slice.ShouldRunConversationCommandElseBranch(
            VmPConversationNextNodeCommand976Slice.DemConvClose));
        Assert.True(VmPConversationNextNodeCommand976Slice.ConversationNpcUpdateApplies(
            conversationNpcNotNull: true,
            VmPConversationNextNodeCommand976Slice.DemConvNull));
        Assert.True(VmPConversationNextNodeCommand976Slice.TerminalPrintsWithoutStatusBar(null));
        Assert.True(VmPConversationNextNodeCommand976Slice.TeardownClearsPlayerConversation(
            intermissionActive: false,
            slideshowActive: false));
        Assert.False(VmPConversationNextNodeCommand976Slice.TeardownClearsPlayerConversation(
            true,
            false));
    }
}
