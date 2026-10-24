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

    [Fact]
    public void TryParseIntStrict_RejectsTrailingGarbage()
    {
        Assert.False(VmEMapInfoParsePrimitives.TryParseIntStrict("12abc", out _));
    }

    [Fact]
    public void SetFlag_TogglesSignedBits()
    {
        var flags = 0;
        VmEMapInfoParsePrimitives.SetFlag(ref flags, 0x4, true);
        Assert.Equal(0x4, flags);
        VmEMapInfoParsePrimitives.SetFlag(ref flags, 0x4, false);
        Assert.Equal(0, flags);
    }

    [Fact]
    public void TryParseDoubleStrict_ParsesDecimal()
    {
        Assert.True(VmEMapInfoParsePrimitives.TryParseDoubleStrict("1.5", out var v));
        Assert.Equal(1.5, v);
    }
}
