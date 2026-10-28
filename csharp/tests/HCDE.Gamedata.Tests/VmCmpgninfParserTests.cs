namespace HCDE.Gamedata.Tests;

public class VmCmpgninfParserTests
{
    [Fact]
    public void TryApplyProperty_ParsesWaveAndDuelLimits()
    {
        var block = new VmCmpgninfBlock();
        Assert.True(VmCmpgninfParser.TryApplyProperty(block, "mapname", "MAP01", out _));
        Assert.True(VmCmpgninfParser.TryApplyProperty(block, "wavelimit", "4000", out _));
        Assert.True(VmCmpgninfParser.TryApplyProperty(block, "duellimit", "300", out _));
        Assert.Equal(3000, block.WaveLimit);
        Assert.Equal(255, block.DuelLimit);
        Assert.True(block.ShouldApply());
    }
}
