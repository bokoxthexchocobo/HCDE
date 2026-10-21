namespace HCDE.Scripting.Tests;

public class ZccBitOpShiftConcatEmitTests
{
    [Fact]
    public void BitOpInstruction_MapsOperators()
    {
        Assert.Equal(ZccBitOpShiftConcatEmit.OpAndRr, ZccBitOpShiftConcatEmit.BitOpInstruction('&'));
    }

    [Fact]
    public void ConcatCastOpcode_PicksIntPath()
    {
        Assert.Equal(10, ZccBitOpShiftConcatEmit.ConcatCastOpcode(false, 1, false, false, false, false, false, true));
    }
}
