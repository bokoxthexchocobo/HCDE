namespace HCDE.Gamedata.Tests;

public class VmStatisticsParserTests
{
    [Fact]
    public void ParseTimeToTics_ConvertsHms()
    {
        Assert.Equal(35, VmStatisticsParser.ParseTimeToTics("0:0:1"));
    }

    [Fact]
    public void CompareEpisodeNames_IsCaseInsensitive()
    {
        Assert.Equal(0, VmStatisticsParser.CompareEpisodeNames("Epi", "epi"));
    }
}
