namespace HCDE.Gamedata.Tests;

public class VmMapInfoEpisodeParserTests
{
    [Fact]
    public void ShouldIgnoreExtendedEpisode_WhenNotMenuHack()
    {
        Assert.True(VmMapInfoEpisodeParser.ShouldIgnoreExtendedEpisode(true, false));
    }

    [Fact]
    public void NormalizeEpisodeKey_LowersFirstChar()
    {
        Assert.Equal('e', VmMapInfoEpisodeParser.NormalizeEpisodeKey("E"));
    }
}
