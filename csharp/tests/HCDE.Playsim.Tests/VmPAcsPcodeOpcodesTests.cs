namespace HCDE.Playsim.Tests;

public class VmPAcsPcodeOpcodesTests
{
    [Fact]
    public void DecodeOpcodeName_KnownValues()
    {
        Assert.Equal(0, VmPAcsPcodeOpcodes.DecodeOpcodeName("PCD_NOP"));
        Assert.Equal(VmPAcsPcodeOpcodes.PCD_TAGSTRING, VmPAcsPcodeOpcodes.DecodeOpcodeName("PCD_TAGSTRING"));
    }

    [Fact]
    public void IsArithmeticOpcode_DetectsAddSubBand()
    {
        Assert.True(VmPAcsPcodeOpcodes.IsArithmeticOpcode(14));
        Assert.False(VmPAcsPcodeOpcodes.IsArithmeticOpcode(3));
    }

    [Fact]
    public void IsDirectLspec_DetectsDirectForms()
    {
        Assert.True(VmPAcsPcodeOpcodes.IsDirectLspec(9));
        Assert.False(VmPAcsPcodeOpcodes.IsDirectLspec(4));
    }

    [Fact]
    public void OpcodeTable_HasCommandCountEntries()
    {
        Assert.Equal(VmPAcsPcodeOpcodes.PcodeCommandCount + 1, VmPAcsPcodeOpcodes.OpcodeTable.Length);
        Assert.Equal(VmPAcsPcodeOpcodes.PCD_CALLSTACK, VmPAcsPcodeOpcodes.OpcodeTable[VmPAcsPcodeOpcodes.PCD_CALLSTACK]);
    }
}
