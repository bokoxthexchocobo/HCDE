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

    [Fact]
    public void ApplyEpisodeDraft_ReplacesExisting()
    {
        var maps = new List<string> { "E1M1" };
        var episodes = new List<VmMapInfoEpisodeDraft> { new() { Map = "E1M1", Name = "Old" } };
        VmMapInfoEpisodeParser.ApplyEpisodeDraft(new VmMapInfoEpisodeDraft { Map = "E1M1", Name = "New" }, maps, episodes);
        Assert.Equal("New", episodes[0].Name);
    }

    [Fact]
    public void TryResolveTeaserMap_UsesTeaserWhenShareware()
    {
        Assert.True(VmMapInfoEpisodeParser.TryResolveTeaserMap(true, "MAP01", "E1M1", out var map));
        Assert.Equal("MAP01", map);
    }
}
