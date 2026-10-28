namespace HCDE.Playsim.Tests;

public class VmPAcsPcodeEnumContinued969SliceTests
{
    [Fact]
    public void ContinuedRange_SpansOneFiftyOpcodes()
    {
        Assert.Equal(71, VmPAcsPcodeEnumContinued969Slice.ContinuedOpcodeMin);
        Assert.Equal(220, VmPAcsPcodeEnumContinued969Slice.ContinuedOpcodeMax);
        Assert.True(VmPAcsPcodeEnumContinued969Slice.IsContinuedOpcode(220));
        Assert.False(VmPAcsPcodeEnumContinued969Slice.IsContinuedOpcode(70));
    }

    [Fact]
    public void IsPrintClusterOpcode_MatchesNativeBand()
    {
        Assert.True(VmPAcsPcodeEnumContinued969Slice.IsPrintClusterOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdPrintnumber));
        Assert.False(VmPAcsPcodeEnumContinued969Slice.IsPrintClusterOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdTimer));
    }

    [Fact]
    public void IsInventoryOpcode_CoversSpawnHead()
    {
        Assert.True(VmPAcsPcodeEnumContinued969Slice.IsInventoryOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdGiveinventorydirect));
        Assert.False(VmPAcsPcodeEnumContinued969Slice.IsInventoryOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdSpawn));
    }

    [Fact]
    public void StackAndCallHelpers()
    {
        Assert.True(VmPAcsPcodeEnumContinued969Slice.IsStackManipOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdDup));
        Assert.True(VmPAcsPcodeEnumContinued969Slice.IsCallOpcode(
            VmPAcsPcodeEnumContinued969Slice.PcdCall));
    }
}
