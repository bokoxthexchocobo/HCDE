namespace HCDE.Gamedata.Tests;

public class VmMapInfoParserFormatTests
{
    [Fact]
    public void ResolveOpenBrace_DetectsNewFormat()
    {
        Assert.Equal(VmMapInfoFormatType.New, VmMapInfoParserFormat.ResolveOpenBrace(true, VmMapInfoFormatType.Old));
    }

    [Fact]
    public void EnsureNotUnexpectedEof_ThrowsForNewFormat()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapInfoParserFormat.EnsureNotUnexpectedEof(VmMapInfoFormatType.New, true, "map"));
    }
}
