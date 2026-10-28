namespace HCDE.Playsim.Tests;

public class VmGGameDmSpawnFlowTests
{
    [Fact]
    public void DeathmatchSpawn_ShouldFallbackWhenNoDmStarts()
    {
        Assert.True(VmGGameDeathmatchSpawnFlow.ShouldFallbackToPlayerStart(0));
    }

    [Fact]
    public void PickPlayerStart_RandomizesWhenForced()
    {
        Assert.True(VmGGamePickPlayerStart.ShouldRandomize(0, VmGGamePickPlayerStart.PpsForceRandom, 1));
    }

    [Fact]
    public void QueueBody_LimitsPlayerCorpsesWhenFilterSet()
    {
        Assert.Equal(16, VmGGameQueueBody.ResolveBodyLimit(VmGGameQueueBody.SvCorpseFilterPlayers, 16));
    }

    [Fact]
    public void QueueBody_DestroysWhenLimitZero()
    {
        Assert.True(VmGGameQueueBody.ShouldDestroyImmediately(0));
    }

    [Fact]
    public void DeathmatchSpawn_DetectsLivingPlayers()
    {
        Assert.True(VmGGameDeathmatchSpawnFlow.PlayerInGameWithMob(true, true, 50));
    }

    [Fact]
    public void PickPlayerStart_CollectsOpenStarts()
    {
        var starts = new[] { 1, 2 };
        var open = new List<int>();
        VmGGamePickPlayerStart.CollectOpenStarts(starts, 0, s => s == 2, open);
        Assert.Single(open);
    }

    [Fact]
    public void PlayerPop_RevertsSpyCamera()
    {
        var mob = new object();
        Assert.True(VmGGamePlayerPop.ShouldRevertSpyCamera(true, mob, mob));
    }

    [Fact]
    public void ScreenShot_QueuesWhenIdle()
    {
        Assert.True(VmGGameScreenShot.ShouldQueueScreenshot(VmGGameScreenShot.GaNothing, "shot.png"));
    }

    [Fact]
    public void LoadGame_SetsActionWhenNamed()
    {
        Assert.Equal(VmGGameLoadGame.GaLoadGame, VmGGameLoadGame.ResolveLoadGameAction(false));
    }

    [Fact]
    public void CheckSaveWads_FailsWhenMissing()
    {
        Assert.False(VmGGameLoadGame.SaveGameWadsComplete(true));
    }
}
