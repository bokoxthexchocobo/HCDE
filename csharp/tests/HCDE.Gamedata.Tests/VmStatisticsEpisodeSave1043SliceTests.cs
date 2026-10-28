namespace HCDE.Gamedata.Tests;

public class VmStatisticsEpisodeSave1043SliceTests
{
    private static VmOneLevelStats Level(int k, int tk, int i, int ti, int s, int ts, int time, string name) =>
        new()
        {
            KillCount = k,
            TotalKills = tk,
            ItemCount = i,
            TotalItems = ti,
            SecretCount = s,
            TotalSecrets = ts,
            LevelTime = time,
            LevelName = name,
        };

    [Fact]
    public void AccumulateEpisodeTotals_SumsAllLevels()
    {
        var levels = new[] { Level(1, 10, 2, 20, 0, 5, 0, "MAP01"), Level(3, 10, 4, 20, 1, 5, 0, "MAP02") };
        var totals = VmStatisticsEpisodeSave1043Slice.AccumulateEpisodeTotals(levels);
        Assert.Equal(4, totals.KillCount);
        Assert.Equal(20, totals.TotalKills);
        Assert.Equal(6, totals.ItemCount);
        Assert.Equal(1, totals.SecretCount);
    }

    [Fact]
    public void FormatEpisodeSummaryInfoLine_MatchesNativeShape()
    {
        var totals = new VmStatisticsEpisodeSave1043Slice.EpisodeStatTotals(4, 20, 6, 40, 1, 10);
        var line = VmStatisticsEpisodeSave1043Slice.FormatEpisodeSummaryInfoLine(totals, 2);
        Assert.Contains("   4/  20", line);
        Assert.EndsWith(",  2", line);
    }

    [Fact]
    public void FormatLevelStatInfoLine_OmitsLevelCount()
    {
        var line = VmStatisticsEpisodeSave1043Slice.FormatLevelStatInfoLine(Level(1, 10, 2, 20, 0, 5, 0, "MAP01"));
        Assert.DoesNotContain("MAP01", line);
        Assert.Contains("   1/  10", line);
    }

    [Fact]
    public void TryMatchStartEpisodeIndex_CaseInsensitive()
    {
        Assert.Equal(1, VmStatisticsEpisodeSave1043Slice.TryMatchStartEpisodeIndex(["E1M1", "E2M1"], "e2m1"));
        Assert.Null(VmStatisticsEpisodeSave1043Slice.TryMatchStartEpisodeIndex(["E1M1"], "MAP01"));
    }

    [Fact]
    public void FormatLevelStatDisplayLine_IncludesTime()
    {
        var line = VmStatisticsEpisodeSave1043Slice.FormatLevelStatDisplayLine(
            Level(1, 1, 1, 1, 0, 0, 35 * 90, "MAP01"));
        Assert.Contains("Level MAP01", line);
        Assert.Contains("Time: 1:30", line);
    }

    [Fact]
    public void ComposeLevelStatReport_ConcatenatesLevels()
    {
        var report = VmStatisticsEpisodeSave1043Slice.ComposeLevelStatReport(
            [Level(0, 0, 0, 0, 0, 0, 0, "A"), Level(0, 0, 0, 0, 0, 0, 0, "B")]);
        Assert.Equal(2, report.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Fact]
    public void CanUseFinishGameCommand_LevelOrCutscene()
    {
        Assert.True(VmStatisticsEpisodeSave1043Slice.CanUseFinishGameCommand(VmStatisticsEpisodeSave1043Slice.GsLevel));
        Assert.True(VmStatisticsEpisodeSave1043Slice.CanUseFinishGameCommand(VmStatisticsEpisodeSave1043Slice.GsCutscene));
        Assert.False(VmStatisticsEpisodeSave1043Slice.CanUseFinishGameCommand(3));
    }

    [Fact]
    public void IsSingleLevelStatisticsSaveMode_OnlyModeTwo()
    {
        Assert.True(VmStatisticsEpisodeSave1043Slice.IsSingleLevelStatisticsSaveMode(2));
        Assert.False(VmStatisticsEpisodeSave1043Slice.IsSingleLevelStatisticsSaveMode(1));
    }

    [Fact]
    public void ResolveVelocityStatCamera_FallsBackToPlayerMob()
    {
        Assert.Same("mob", VmStatisticsEpisodeSave1043Slice.ResolveVelocityStatCamera(null, false, "mob"));
        Assert.Same("cam", VmStatisticsEpisodeSave1043Slice.ResolveVelocityStatCamera("cam", true, "mob"));
    }

    [Fact]
    public void FormatVelocityStatBlock_IncludesMapName()
    {
        var text = VmStatisticsEpisodeSave1043Slice.FormatVelocityStatBlock(1.5, 9, 4, "MAP01");
        Assert.Contains("Current velocity: 1.50", text);
        Assert.Contains("Level MAP01", text);
    }

    [Fact]
    public void ResolveEpisodeNameForDisplay_EmptyWhenMissing()
    {
        Assert.Equal("", VmStatisticsEpisodeSave1043Slice.ResolveEpisodeNameForDisplay(null));
        Assert.Equal("Episode 1", VmStatisticsEpisodeSave1043Slice.ResolveEpisodeNameForDisplay("Episode 1"));
    }

    [Fact]
    public void FormatFinishGameBlockedMessage_MentionsCommand()
    {
        Assert.Contains("finishgame", VmStatisticsEpisodeSave1043Slice.FormatFinishGameBlockedMessage());
    }
}
