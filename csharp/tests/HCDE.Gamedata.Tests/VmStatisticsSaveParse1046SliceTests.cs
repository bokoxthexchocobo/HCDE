namespace HCDE.Gamedata.Tests;

public class VmStatisticsSaveParse1046SliceTests
{
    [Fact]
    public void BuildNestedLevelStatDraft_ZeroSkill()
    {
        var level = VmStatisticsSaveParse1046Slice.BuildNestedLevelStatDraft("MAP01", "info", "0:01:00");
        Assert.Equal("MAP01", level.Name);
        Assert.Equal(0, level.Skill);
        Assert.Equal(VmStatisticsParser.ParseTimeToTics("0:01:00"), level.TimeNeededTics);
    }

    [Fact]
    public void ComputeSessionDateSortKey_ParsesDdMmYyyy()
    {
        var left = VmStatisticsSaveParse1046Slice.ComputeSessionDateSortKey("01.02.2020");
        var right = VmStatisticsSaveParse1046Slice.ComputeSessionDateSortKey("15.03.2021");
        Assert.True(VmStatisticsSaveParse1046Slice.CompareSessionDateSortKeys(left, right) < 0);
    }

    [Fact]
    public void CompareEpisodeHeaderNamesNatural_CaseInsensitive()
    {
        Assert.Equal(0, VmStatisticsSaveParse1046Slice.CompareEpisodeHeaderNamesNatural("DOOM2.E1", "doom2.e1"));
    }

    [Fact]
    public void ShouldEmitSavedSessionRow_RequiresNonEmptyInfo()
    {
        Assert.False(VmStatisticsSaveParse1046Slice.ShouldEmitSavedSessionRow(""));
        Assert.True(VmStatisticsSaveParse1046Slice.ShouldEmitSavedSessionRow("kills"));
    }

    [Fact]
    public void FormatSavedSessionRow_MatchesNativeShape()
    {
        var row = VmStatisticsSaveParse1046Slice.FormatSavedSessionRow(
            1, "01.02.2020", "stats", 35 * 90, 3);
        Assert.Contains("01.02.2020", row);
        Assert.Contains("00:01:30", row);
    }

    [Fact]
    public void FormatSavedNestedLevelRow_IncludesLevelName()
    {
        var row = VmStatisticsSaveParse1046Slice.FormatSavedNestedLevelRow("MAP01", "info", 35);
        Assert.Contains("MAP01", row);
    }

    [Fact]
    public void FindEpisodeStatisticsIndex_MatchesSectionCaseInsensitive()
    {
        Assert.Equal(1, VmStatisticsSaveParse1046Slice.FindEpisodeStatisticsIndex(["A", "B"], "b"));
        Assert.Equal(-1, VmStatisticsSaveParse1046Slice.FindEpisodeStatisticsIndex(["A"], "C"));
    }

    [Fact]
    public void FormatStatisticsEntryDate_UsesLocalTimeOrFallback()
    {
        var dt = new DateTime(2020, 2, 1);
        Assert.Equal("01.02.2020", VmStatisticsSaveParse1046Slice.FormatStatisticsEntryDate(dt));
        Assert.Equal("00.00.0000", VmStatisticsSaveParse1046Slice.FormatStatisticsEntryDate(null));
    }

    [Fact]
    public void BuildStatisticsEntryDraft_CapturesFields()
    {
        var draft = VmStatisticsSaveParse1046Slice.BuildStatisticsEntryDraft(
            "info", 70, 2, new DateTime(2021, 3, 5));
        Assert.Equal("05.03.2021", draft.DateName);
        Assert.Equal(2, draft.Skill);
    }

    [Fact]
    public void FormatSavedEpisodeBlockHeader_AndFooter()
    {
        Assert.Contains("EP \"Name\"", VmStatisticsSaveParse1046Slice.FormatSavedEpisodeBlockHeader("EP", "Name"));
        Assert.Equal("}\n\n", VmStatisticsSaveParse1046Slice.FormatSavedEpisodeBlockFooter());
    }

    [Fact]
    public void ShouldReloadEpisodeStatistics_RequiresPath()
    {
        Assert.False(VmStatisticsSaveParse1046Slice.ShouldReloadEpisodeStatistics(""));
        Assert.True(VmStatisticsSaveParse1046Slice.ShouldReloadEpisodeStatistics("zdoomstat.txt"));
    }

    [Fact]
    public void FormatUnableToSaveStatisticsMessage_IncludesPath()
    {
        Assert.Contains("Unable to save", VmStatisticsSaveParse1046Slice.FormatUnableToSaveStatisticsMessage("x.txt"));
    }
}
