namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseNextMapTests
{
    [Fact]
    public void FormatNumericNextMap_HexenUsesWarpTrans()
    {
        Assert.Equal("&wt@05", VmMapInfoParseNextMap.FormatNumericNextMap(5, true));
    }

    [Fact]
    public void FormatEndSequenceMap_EncodesIndex()
    {
        Assert.StartsWith("enDSeQ", VmMapInfoParseNextMap.FormatEndSequenceMap(0x12));
    }
}
