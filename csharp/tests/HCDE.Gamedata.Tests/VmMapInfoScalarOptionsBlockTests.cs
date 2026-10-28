namespace HCDE.Gamedata.Tests;

public class VmMapInfoScalarOptionsBlockTests
{
    [Fact]
    public void PackDefaultEnvironment_CombinesBytes()
    {
        Assert.Equal(0x0102, VmMapInfoScalarOptionsBlock.PackDefaultEnvironment(1, 2));
    }
}
