namespace HCDE.Gamedata.Tests;

public class VmVoxelSpinNameParserTests
{
    [Fact]
    public void TryParse_ReadsSpinSign()
    {
        Assert.True(VmVoxelSpinNameParser.TryParse("TROOA-12", 29, out var r));
        Assert.Equal(-24, r.SpinPerSecond);
    }
}
