namespace HCDE.Playsim.Tests;

public class VmPAcsPcodeEnumContinued970SliceTests
{
    [Fact]
    public void ContinuedRange_MatchesNativeBand()
    {
        Assert.Equal(221, VmPAcsPcodeEnumContinued970Slice.ContinuedOpcodeMin);
        Assert.Equal(368, VmPAcsPcodeEnumContinued970Slice.ContinuedOpcodeMax);
        Assert.True(VmPAcsPcodeEnumContinued970Slice.IsContinuedOpcode(368));
        Assert.False(VmPAcsPcodeEnumContinued970Slice.IsContinuedOpcode(220));
    }

    [Fact]
    public void IsTrigOpcode_CoversCosThroughVectorangle()
    {
        Assert.True(VmPAcsPcodeEnumContinued970Slice.IsTrigOpcode(
            VmPAcsPcodeEnumContinued970Slice.PcdCos));
        Assert.False(VmPAcsPcodeEnumContinued970Slice.IsTrigOpcode(
            VmPAcsPcodeEnumContinued970Slice.PcdTagstring));
    }

    [Fact]
    public void IsWorldArrayOpcode_Span()
    {
        Assert.True(VmPAcsPcodeEnumContinued970Slice.IsWorldArrayOpcode(
            VmPAcsPcodeEnumContinued970Slice.PcdAddworldarray));
    }

    [Fact]
    public void IsCallfuncOpcode_IncludesPushfunction()
    {
        Assert.True(VmPAcsPcodeEnumContinued970Slice.IsCallfuncOpcode(
            VmPAcsPcodeEnumContinued970Slice.PcdPushfunction));
    }
}
