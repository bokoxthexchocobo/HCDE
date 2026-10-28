namespace HCDE.Client.Tests;

public class VmSbarinfoCommandFlowControlNextCommand1097SliceTests
{
    [Theory]
    [InlineData("alpha")]
    [InlineData("ifhealth")]
    [InlineData("ifinvulnerable")]
    [InlineData("ifwaterlevel")]
    [InlineData("ifcvarint")]
    public void IsNextCommandDispatchCaseWithinEofBand_IncludesTailCases(string identifier)
    {
        var (_, kind) = VmSbarinfoCommandFlowControlNextCommand1096Slice.TryResolveSBarInfoFlowCommandKind(identifier);
        Assert.True(VmSbarinfoCommandFlowControlNextCommand1097Slice.IsNextCommandDispatchCaseWithinEofBand(kind));
    }

    [Fact]
    public void IsNextCommandDispatchCaseWithinEofBand_ExcludesInInventoryHead()
    {
        Assert.False(VmSbarinfoCommandFlowControlNextCommand1097Slice.IsNextCommandDispatchCaseWithinEofBand(
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.InInventory));
    }

    [Fact]
    public void TryResolveNextCommandTailFactoryKind_MapsIfCVarInt()
    {
        Assert.True(VmSbarinfoCommandFlowControlNextCommand1097Slice.TryResolveNextCommandTailFactoryKind(
            VmSbarinfoCommandFlowControlNextCommand1096Slice.SBarInfoFlowCommandKind.IfCVarInt,
            out var factory));
        Assert.Equal(
            VmSbarinfoCommandFlowControlNextCommand1097Slice.NextCommandTailFactoryKind.CommandIfCVarInt,
            factory);
    }

    [Fact]
    public void ResolveNextCommandUnknownIdentifierOutcome_FailsWhenSwitchMisses()
    {
        var (report, retNull) = VmSbarinfoCommandFlowControlNextCommand1097Slice
            .ResolveNextCommandUnknownIdentifierOutcome(switchMatchedKnownCase: false);
        Assert.True(report);
        Assert.True(retNull);
    }

    [Fact]
    public void ResolveNextCommandWhenIdentifierAbsent_RequiresClosingBrace()
    {
        var (mustGet, retNull) = VmSbarinfoCommandFlowControlNextCommand1097Slice
            .ResolveNextCommandWhenIdentifierAbsent();
        Assert.True(mustGet);
        Assert.True(retNull);
    }
}
