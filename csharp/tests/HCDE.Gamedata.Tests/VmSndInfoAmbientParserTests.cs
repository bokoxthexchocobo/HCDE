namespace HCDE.Gamedata.Tests;

public class VmSndInfoAmbientParserTests
{
    [Fact]
    public void TryParseData_ReadsAmbientBlock()
    {
        Assert.True(SndInfoMapMusicParser.TryParseData(
            "$ambient 1 wind point 1.5 continuous 0.8", out var data, out var error));
        Assert.Null(error);
        var ambient = Assert.Single(data.Ambients);
        Assert.Equal(1, ambient.Index);
        Assert.Equal("wind", ambient.LogicalSound);
        Assert.Equal("continuous", ambient.Mode);
        Assert.Equal(0.8f, ambient.Volume);
    }
}
