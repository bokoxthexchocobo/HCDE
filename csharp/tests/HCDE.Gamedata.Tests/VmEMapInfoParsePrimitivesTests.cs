namespace HCDE.Gamedata.Tests;

public class VmEMapInfoParsePrimitivesTests
{
    [Fact]
    public void GetDefaultLevelNum_ParsesMap01()
    {
        var num = VmEMapInfoParsePrimitives.GetDefaultLevelNum("MAP01", out var id24);
        Assert.Equal(1, num);
        Assert.Equal(1, id24);
    }
}
