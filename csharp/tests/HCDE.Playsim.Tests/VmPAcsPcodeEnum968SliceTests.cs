namespace HCDE.Playsim.Tests;

public class VmPAcsPcodeEnum968SliceTests
{
    [Fact]
    public void BaselineOpcodeMax_MatchesNativeRestart()
    {
        Assert.Equal(69, VmPAcsPcodeEnum968Slice.BaselineOpcodeMax);
        Assert.True(VmPAcsPcodeEnum968Slice.IsBaselineOpcode(VmPAcsPcodeEnum968Slice.PcdRestart));
        Assert.False(VmPAcsPcodeEnum968Slice.IsBaselineOpcode(70));
    }

    [Fact]
    public void LspecArgCount_StackAndDirect()
    {
        Assert.Equal(3, VmPAcsPcodeEnum968Slice.LspecArgCount(VmPAcsPcodeEnum968Slice.PcdLspec3));
        Assert.Equal(5, VmPAcsPcodeEnum968Slice.LspecArgCount(VmPAcsPcodeEnum968Slice.PcdLspec5Direct));
        Assert.Equal(0, VmPAcsPcodeEnum968Slice.LspecArgCount(VmPAcsPcodeEnum968Slice.PcdNop));
    }

    [Fact]
    public void CompareAndArithmeticRanges()
    {
        Assert.True(VmPAcsPcodeEnum968Slice.IsCompareOpcode(VmPAcsPcodeEnum968Slice.PcdEq));
        Assert.True(VmPAcsPcodeEnum968Slice.IsBinaryArithmeticOpcode(VmPAcsPcodeEnum968Slice.PcdDivide));
        Assert.False(VmPAcsPcodeEnum968Slice.IsCompareOpcode(VmPAcsPcodeEnum968Slice.PcdGoto));
    }

    [Fact]
    public void BaselineOpcodeLabel_KnownNames()
    {
        Assert.Equal("PCD_NOP", VmPAcsPcodeEnum968Slice.BaselineOpcodeLabel(0));
        Assert.Equal("PCD_LSPEC2", VmPAcsPcodeEnum968Slice.BaselineOpcodeLabel(5));
        Assert.Equal("PCD_LSPEC4DIRECT", VmPAcsPcodeEnum968Slice.BaselineOpcodeLabel(12));
    }
}
