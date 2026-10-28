namespace HCDE.Client.Tests;

public class VmSbarinfoCommandPlayerType1093SliceTests
{
    [Fact]
    public void ResolvePlayerTypeClassListLoopContinues_WithStringOrIdentifier()
    {
        Assert.True(VmSbarinfoCommandPlayerType1093Slice.ResolvePlayerTypeClassListLoopContinues(
            nextIdentifierPresent: false,
            nextStringConstPresent: true));
    }

    [Fact]
    public void ResolvePlayerTypeFlowControlTruthFromDescendants_RequiresDescendantMatch()
    {
        Assert.True(VmSbarinfoCommandPlayerType1093Slice.ResolvePlayerTypeFlowControlTruthFromDescendants(
            playerClassMissing: false,
            spawnClassDescendsFromAnyListedType: true));
    }

    [Fact]
    public void ValidateHasWeaponPieceNumber_RejectsBelowOne()
    {
        var (valid, normalized) = VmSbarinfoCommandHasWeaponPiece1093Slice.ValidateHasWeaponPieceNumber(0);
        Assert.False(valid);
        Assert.Equal(1, normalized);
    }

    [Fact]
    public void ResolveHasWeaponPieceFlowControlTruth_ChecksPieceMaskBit()
    {
        Assert.True(VmSbarinfoCommandHasWeaponPiece1093Slice.ResolveHasWeaponPieceFlowControlTruth(
            pieceWeaponMatches: true,
            pieceMask: 0b101,
            pieceIndex: 3));
    }

    [Fact]
    public void CreateDrawGemCtorDefaults_MatchesNativeInit()
    {
        var d = VmSbarinfoCommandDrawGem1093Slice.CreateDrawGemCtorDefaults();
        Assert.Equal(-1, d.Chain);
        Assert.Equal(1, d.ChainSize);
    }

    [Fact]
    public void ApplyDrawGemChainWiggleOffset_AddsWiggleWhenValuesDiffer()
    {
        Assert.Equal(
            12,
            VmSbarinfoCommandDrawGem1093Slice.ApplyDrawGemChainWiggleOffset(
                wiggleEnabled: true,
                drawValue: 1,
                goalValue: 50,
                baseY: 10,
                chainWiggle: 2));
    }

    [Fact]
    public void ComputeDrawGemChainHorizontalOffset_ModuloChainSize()
    {
        Assert.Equal(
            2,
            VmSbarinfoCommandDrawGem1093Slice.ComputeDrawGemChainHorizontalOffset(
                chainWidth: 100,
                leftPadding: 0,
                rightPadding: 0,
                drawValue: 52,
                chainSize: 5));
    }

    [Fact]
    public void TryClassifyDrawGemParseFlagKeyword_MapsInterpolate()
    {
        var (ok, flag) = VmSbarinfoCommandDrawGem1093Slice.TryClassifyDrawGemParseFlagKeyword("interpolate");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawGem1093Slice.DrawGemParseFlag.Interpolate, flag);
    }

    [Fact]
    public void ResolveDrawGemParseFlagListContinuation_EndsOnComma()
    {
        var (cont, comma) = VmSbarinfoCommandDrawGem1093Slice.ResolveDrawGemParseFlagListContinuation(
            nextTokenIsPipe: false);
        Assert.False(cont);
        Assert.True(comma);
    }

    [Fact]
    public void ApplyDrawGemSignedPaddingToken_SupportsNegative()
    {
        Assert.Equal(-8, VmSbarinfoCommandDrawGem1093Slice.ApplyDrawGemSignedPaddingToken(
            negative: true,
            magnitude: 8));
    }
}
