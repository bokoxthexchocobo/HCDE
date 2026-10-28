namespace HCDE.Client.Tests;

public class VmSbarinfoParser1063SliceTests
{
    [Fact]
    public void ResolveSignedCoordinate_HonorsNegativePrefix()
    {
        Assert.Equal(-12, VmSbarinfoParser1063Slice.ResolveSignedCoordinate(true, 12));
        Assert.Equal(5, VmSbarinfoParser1063Slice.ResolveSignedCoordinate(false, 5));
    }

    [Fact]
    public void ShouldStoreRelativeCenter_OnlyWhenFullScreen()
    {
        Assert.True(VmSbarinfoParser1063Slice.ShouldStoreRelativeCenter(fullScreenOffsets: true, hasCenterModifier: true));
        Assert.False(VmSbarinfoParser1063Slice.ShouldStoreRelativeCenter(false, true));
    }

    [Fact]
    public void BuildNamedTranslationToken_WrapsBracketSyntax()
    {
        Assert.Equal("[Gold]", VmSbarinfoParser1063Slice.BuildNamedTranslationToken("Gold"));
    }

    [Fact]
    public void IsMissingTranslation_UsesUndefinedSentinel()
    {
        Assert.True(VmSbarinfoParser1063Slice.IsMissingTranslation(VmVFontParseInit1062Slice.ColorUndefined));
        Assert.False(VmSbarinfoParser1063Slice.IsMissingTranslation(3));
    }

    [Fact]
    public void ShouldSkipFlowTruthToggle_WhenUnchanged()
    {
        Assert.True(VmSbarinfoParser1063Slice.ShouldSkipFlowTruthToggle(true, true));
        Assert.False(VmSbarinfoParser1063Slice.ShouldSkipFlowTruthToggle(false, true));
    }

    [Fact]
    public void ClassifyFlowBlockStart_DetectsMissingCommand()
    {
        Assert.Equal(
            VmSbarinfoParser1063Slice.FlowBlockParseShape.MissingCommand,
            VmSbarinfoParser1063Slice.ClassifyFlowBlockStart(hasOpeningBrace: false, hasSingleCommand: false));
    }

    [Fact]
    public void TryParseNotPrefix_RecognizesNot()
    {
        Assert.True(VmSbarinfoParser1063Slice.TryParseNotPrefix("not", out var negate));
        Assert.True(negate);
    }

    [Fact]
    public void ClassifyMainBlockFlag_RecognizesNativeKeywords()
    {
        Assert.Equal(
            VmSbarinfoParser1063Slice.MainBlockHudFlag.ForceScaled,
            VmSbarinfoParser1063Slice.ClassifyMainBlockFlag("forcescaled"));
        Assert.Equal(
            VmSbarinfoParser1063Slice.MainBlockHudFlag.Unknown,
            VmSbarinfoParser1063Slice.ClassifyMainBlockFlag("bogus"));
    }

    [Fact]
    public void CombineDrawAlpha_MultipliesLikeNativeDraw()
    {
        Assert.Equal(0.5, VmSbarinfoParser1063Slice.CombineDrawAlpha(0.5, 1.0));
        Assert.Equal(0.25, VmSbarinfoParser1063Slice.CombineDrawAlpha(0.5, 0.5));
    }

    [Fact]
    public void MainBlockStartsTrue_MatchesNativeInit()
    {
        Assert.True(VmSbarinfoParser1063Slice.MainBlockStartsTrue());
    }
}
