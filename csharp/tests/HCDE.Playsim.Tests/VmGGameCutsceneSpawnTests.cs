namespace HCDE.Playsim.Tests;

public class VmGGameCutsceneSpawnTests
{
    [Fact]
    public void FullConsole_StopsMusicOutsideStartup()
    {
        Assert.True(VmGGameFullConsole.ShouldStopMusicAfterQuit(1));
        Assert.False(VmGGameFullConsole.ShouldStopMusicAfterQuit(VmGGameFullConsole.GsStartup));
    }

    [Fact]
    public void CutsceneFlow_EndsSinglePlayerJob()
    {
        Assert.True(VmGGameCutsceneFlow.ShouldFinishSinglePlayerScreenJob(false, false, true));
    }

    [Fact]
    public void SpawnSpot_RejectsZeroType()
    {
        Assert.True(VmGGameSpawnSpot.IsInvalidStartType(0));
    }

    [Fact]
    public void SpawnPlayerTail_SkipsInventoryOnTitleMap()
    {
        Assert.False(VmGGameSpawnPlayerTail.ShouldGiveDefaultInventory(VmGGameSpawnPlayerTail.GsTitleLevel));
    }

    [Fact]
    public void PlayersRangeFromSpot_UsesNearestLiving()
    {
        var players = new (bool, bool, int, double, double)[]
        {
            (true, true, 1, 0, 0),
            (true, true, 1, 10, 0)
        };
        var range = VmGGameSpawnSpot.PlayersRangeFromSpot(5, 0, players);
        Assert.Equal(5, range);
    }

    [Fact]
    public void SelectFarthestIndex_PrefersRemoteStart()
    {
        var starts = new[] { 0.0, 10.0 };
        var idx = VmGGameSpawnSpot.SelectFarthestIndex(starts, s => s == 0 ? 1 : 9);
        Assert.Equal(1, idx);
    }
}
