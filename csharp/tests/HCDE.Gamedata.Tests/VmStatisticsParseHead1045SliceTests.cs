namespace HCDE.Gamedata.Tests;

public class VmStatisticsParseHead1045SliceTests
{
    [Fact]
    public void BuildEpisodeDraft_CapturesHeaderAndName()
    {
        var ep = VmStatisticsParseHead1045Slice.BuildEpisodeDraft("DOOM2.E1", "Episode 1");
        Assert.Equal("DOOM2.E1", ep.Header);
        Assert.Equal("Episode 1", ep.DisplayName);
    }

    [Fact]
    public void IsStatisticsBlockOpenToken_OnlyBrace()
    {
        Assert.True(VmStatisticsParseHead1045Slice.IsStatisticsBlockOpenToken("{"));
        Assert.False(VmStatisticsParseHead1045Slice.IsStatisticsBlockOpenToken("}"));
    }

    [Fact]
    public void BuildSessionDraft_ParsesDurationAndTruncates()
    {
        var session = VmStatisticsParseHead1045Slice.BuildSessionDraft(
            "ignored",
            new string('N', 30),
            new string('I', 80),
            "1:02:03",
            3);
        Assert.Equal(24, session.Name.Length);
        Assert.Equal(60, session.Info.Length);
        Assert.Equal(3, session.Skill);
        Assert.Equal(VmStatisticsParser.ParseTimeToTics("1:02:03"), session.TimeNeededTics);
    }

    [Fact]
    public void SelectSessionNameFromFileTokens_UsesSecondToken()
    {
        var name = VmStatisticsParseHead1045Slice.SelectSessionNameFromFileTokens("player", "01.02.2020");
        Assert.Equal("01.02.2020", name);
    }

    [Fact]
    public void ParseColonDurationToTics_DelegatesToParser()
    {
        Assert.Equal(35, VmStatisticsParseHead1045Slice.ParseColonDurationToTics("0:0:1"));
    }

    [Fact]
    public void ShouldSkipParseWhenFileUnavailable()
    {
        Assert.True(VmStatisticsParseHead1045Slice.ShouldSkipParseWhenFileUnavailable(false));
        Assert.False(VmStatisticsParseHead1045Slice.ShouldSkipParseWhenFileUnavailable(true));
    }

    [Fact]
    public void ParseLevelStatNameFromToken_Truncates()
    {
        Assert.Equal(24, VmStatisticsParseHead1045Slice.ParseLevelStatNameFromToken(new string('X', 30)).Length);
    }

    [Fact]
    public void ShouldBeginSessionLevelBlock_MatchesOpenBrace()
    {
        Assert.True(VmStatisticsParseHead1045Slice.ShouldBeginSessionLevelBlock("{"));
    }

    [Fact]
    public void DefaultConfig_MatchesCvarDefaults()
    {
        Assert.Equal("zdoomstat.txt", VmStatisticsParseHead1045Slice.DefaultStatFileName());
        Assert.Equal(0, VmStatisticsParseHead1045Slice.DefaultSaveStatisticsMode());
    }

    [Fact]
    public void TryBuildSessionDraft_ValidatesTokens()
    {
        Assert.False(VmStatisticsParseHead1045Slice.TryBuildSessionDraft(
            "a", "", "info", "0:0:1", 1, out _));
        Assert.True(VmStatisticsParseHead1045Slice.TryBuildSessionDraft(
            "a", "b", "info", "0:0:1", 1, out var draft));
        Assert.Equal("b", draft.Name);
    }

    [Fact]
    public void ShouldParseAnotherSessionEntry_UntilCloseBrace()
    {
        Assert.True(VmStatisticsParseHead1045Slice.ShouldParseAnotherSessionEntry(episodeSectionCloseMatched: false));
        Assert.False(VmStatisticsParseHead1045Slice.ShouldParseAnotherSessionEntry(episodeSectionCloseMatched: true));
    }

    [Fact]
    public void TakePendingLevelInfoToken_Truncates()
    {
        Assert.Equal(60, VmStatisticsParseHead1045Slice.TakePendingLevelInfoToken(new string('Z', 100)).Length);
    }
}
