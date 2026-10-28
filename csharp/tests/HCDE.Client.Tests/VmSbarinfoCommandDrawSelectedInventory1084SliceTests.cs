namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawSelectedInventory1084SliceTests
{
    [Fact]
    public void CreateSelectedInventoryCtorDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoCommandDrawSelectedInventory1084Slice.CreateSelectedInventoryCtorDefaults();
        Assert.False(d.AlternateOnEmpty);
        Assert.Equal(int.MaxValue, d.CounterLength);
    }

    [Fact]
    public void ShouldDrawSelectedInventoryBar_RequiresSelectionAndBarAllowed()
    {
        Assert.True(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldDrawSelectedInventoryBar(
            hasSelectedItem: true,
            levelDisablesInventoryBar: false));
        Assert.False(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldDrawSelectedInventoryBar(
            hasSelectedItem: true,
            levelDisablesInventoryBar: true));
    }

    [Fact]
    public void ComputeArtiflashImageIndex_MatchesNativeMacro()
    {
        Assert.Equal(
            108,
            VmSbarinfoCommandDrawSelectedInventory1084Slice.ComputeArtiflashImageIndex(
                invBarOffset: 100,
                artiflashTick: 2));
    }

    [Fact]
    public void ShouldApplyItemFlashCursor_RequiresNonZeroFade()
    {
        Assert.True(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldApplyItemFlashCursor(
            itemflashEnabled: true,
            itemflashFade: 1));
        Assert.False(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldApplyItemFlashCursor(
            itemflashEnabled: true,
            itemflashFade: 0));
    }

    [Fact]
    public void ShouldDrawSelectedInventoryAmountCounter_HidesSingletonUnlessForced()
    {
        Assert.False(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldDrawSelectedInventoryAmountCounter(
            alwaysShowCounter: false,
            selectedAmount: 1));
        Assert.True(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldDrawSelectedInventoryAmountCounter(
            alwaysShowCounter: true,
            selectedAmount: 1));
    }

    [Fact]
    public void ResolveSelectedInventoryImageOffset_MapsCenterBottomBits()
    {
        var offset = VmSbarinfoCommandDrawSelectedInventory1084Slice.ResolveSelectedInventoryImageOffset(
            VmSbarinfoCommandDrawSelectedInventory1084Slice.SelectedInventoryParseKeyword.CenterBottom);
        Assert.Equal(
            VmSbarinfoCommandDrawSelectedInventory1084Slice.DrawImageOffsetFlags.CenterBottom,
            offset);
    }

    [Fact]
    public void ShouldTreatSelectedInventoryTokenAsFont_WhenUnknownIdentifier()
    {
        Assert.True(VmSbarinfoCommandDrawSelectedInventory1084Slice.ShouldTreatSelectedInventoryTokenAsFont(
            tokenIsIdentifier: true,
            recognizedKeyword: false));
    }

    [Fact]
    public void ResolveSelectedInventoryDefaultCounterPosition_Applies3030And24()
    {
        var (x, y) = VmSbarinfoCommandDrawSelectedInventory1084Slice.ResolveSelectedInventoryDefaultCounterPosition(10, 20);
        Assert.Equal(40, x);
        Assert.Equal(44, y);
    }

    [Fact]
    [Fact]
    public void ResolveSelectedInventoryParseTail_SelectsFlowControlOrSemicolon()
    {
        var (flow, semi) = VmSbarinfoCommandDrawSelectedInventory1084Slice.ResolveSelectedInventoryParseTail(
            alternateOnEmpty: true);
        Assert.True(flow);
        Assert.False(semi);
    }

    [Fact]
    public void ResolveSelectedInventoryFlowControlTruth_WhenMissingSelection()
    {
        Assert.True(VmSbarinfoCommandDrawSelectedInventory1084Slice.ResolveSelectedInventoryFlowControlTruth(
            hasSelectedItem: false,
            levelDisablesInventoryBar: false));
    }

    [Fact]
    public void ApplyCommandGameModeFlagBit_StacksFromCtorZero()
    {
        Assert.Equal(
            0x4,
            VmSbarinfoCommandGameMode1084Slice.ApplyCommandGameModeFlagBit(modes: 0, flagBit: 0x4));
    }
}
