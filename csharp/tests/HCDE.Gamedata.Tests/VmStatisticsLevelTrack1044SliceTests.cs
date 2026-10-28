namespace HCDE.Gamedata.Tests;

public class VmStatisticsLevelTrack1044SliceTests
{
    [Fact]
    public void BuildLevelStatEntry_TruncatesNameAndInfo()
    {
        var entry = VmStatisticsLevelTrack1044Slice.BuildLevelStatEntry(
            new string('A', 30),
            new string('B', 80),
            35);
        Assert.Equal(23, entry.Name.Length);
        Assert.Equal(59, entry.Info.Length);
        Assert.Equal(35, entry.TimeNeeded);
    }

    [Fact]
    public void ShouldAssignStartEpisodeOnNewGame_SkipsDmAndMp()
    {
        Assert.False(VmStatisticsLevelTrack1044Slice.ShouldAssignStartEpisodeOnNewGame(true, false));
        Assert.True(VmStatisticsLevelTrack1044Slice.ShouldAssignStartEpisodeOnNewGame(false, false));
    }

    [Fact]
    public void FindOrReserveLevelIndex_CaseInsensitiveReuse()
    {
        var data = new List<VmOneLevelStats> { new() { LevelName = "MAP01" } };
        var idx = VmStatisticsLevelTrack1044Slice.FindOrReserveLevelIndex(data, "map01");
        Assert.Equal(0, idx);
        Assert.Single(data);
    }

    [Fact]
    public void ApplyLevelCounterSnapshot_CopiesCounters()
    {
        var level = new VmOneLevelStats();
        VmStatisticsLevelTrack1044Slice.ApplyLevelCounterSnapshot(
            level,
            new VmStatisticsLevelTrack1044Slice.LevelCounterSnapshot(10, 4, 20, 5, 3, 1, 700));
        Assert.Equal(10, level.TotalKills);
        Assert.Equal(4, level.KillCount);
        Assert.Equal(700, level.LevelTime);
    }

    [Fact]
    public void ReconcileKillCountWhenNoLivingMonsters_ForcesFullClear()
    {
        Assert.Equal(10, VmStatisticsLevelTrack1044Slice.ReconcileKillCountWhenNoLivingMonsters(4, 10, 0));
        Assert.Equal(4, VmStatisticsLevelTrack1044Slice.ReconcileKillCountWhenNoLivingMonsters(4, 10, 2));
    }

    [Fact]
    public void ShouldSkipLevelStatistics_WhenNoStatisticsFlag()
    {
        Assert.True(VmStatisticsLevelTrack1044Slice.ShouldSkipLevelStatistics(
            VmStatisticsLevelTrack1044Slice.Level2NoStatistics));
    }

    [Fact]
    public void IsEndSequenceMapTransition_DetectsPrefix()
    {
        Assert.True(VmStatisticsLevelTrack1044Slice.IsEndSequenceMapTransition("enDSeQ01"));
        Assert.False(VmStatisticsLevelTrack1044Slice.ShouldResolveNextLevelInfo("enDSeQ01"));
    }

    [Fact]
    public void ReachedEpisodeStatisticsCheckpoint_EndGameOrMissingNext()
    {
        Assert.True(VmStatisticsLevelTrack1044Slice.ReachedEpisodeStatisticsCheckpoint(
            nextLevelInfoMissing: true,
            nextLevelFlags2: 0,
            startEpisodeActive: true));
        Assert.True(VmStatisticsLevelTrack1044Slice.ReachedEpisodeStatisticsCheckpoint(
            false,
            VmStatisticsLevelTrack1044Slice.Level2EndGame,
            true));
        Assert.False(VmStatisticsLevelTrack1044Slice.ReachedEpisodeStatisticsCheckpoint(false, 0, false));
    }

    [Fact]
    public void BuildEpisodeStatisticsSectionKey_Uppercases()
    {
        Assert.Equal("DOOM2.E1M1", VmStatisticsLevelTrack1044Slice.BuildEpisodeStatisticsSectionKey("doom2", "E1M1"));
    }

    [Fact]
    public void ResolveEpisodeNameForStatistics_LocalizesDollarPrefix()
    {
        var resolved = VmStatisticsLevelTrack1044Slice.ResolveEpisodeNameForStatistics(
            "$EP1NAME",
            key => "Episode 1");
        Assert.Equal("Episode 1", resolved);
    }

    [Fact]
    public void SortLevelDataByName_OrdersAlphabetically()
    {
        var data = new List<VmOneLevelStats>
        {
            new() { LevelName = "MAP02" },
            new() { LevelName = "MAP01" },
        };
        VmStatisticsLevelTrack1044Slice.SortLevelDataByName(data);
        Assert.Equal("MAP01", data[0].LevelName);
    }

    [Fact]
    public void ShouldPersistEpisodeStatistics_OnlyModeOne()
    {
        Assert.True(VmStatisticsLevelTrack1044Slice.ShouldPersistEpisodeStatistics(1));
        Assert.False(VmStatisticsLevelTrack1044Slice.ShouldPersistEpisodeStatistics(2));
    }
}
