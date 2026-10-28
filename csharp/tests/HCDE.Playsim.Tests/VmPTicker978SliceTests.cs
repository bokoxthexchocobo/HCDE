namespace HCDE.Playsim.Tests;

public class VmPTicker978SliceTests
{
    [Fact]
    public void DedicatedServer_SkipsClientSideLogic()
    {
        Assert.True(VmPTicker978Slice.DedicatedServerSkipsClientSideLogic(true, false));
        Assert.False(VmPTicker978Slice.DedicatedServerSkipsClientSideLogic(false, false));
    }

    [Fact]
    public void ClientSideLevelTick_InventoryAndThinkers()
    {
        Assert.True(VmPTicker978Slice.GameStateRunsLevelClientSideTick(VmPTicker978Slice.GsLevel));
        Assert.True(VmPTicker978Slice.ShouldDecrementPlayerInventoryTics(
            worldPaused: false,
            playerInGame: true,
            inventoryTics: 3));
        Assert.Equal(2, VmPTicker978Slice.DecrementInventoryTics(3));
        Assert.True(VmPTicker978Slice.TryDecrementInventoryTicsForPlayer(
            false,
            true,
            3,
            out var next));
        Assert.Equal(2, next);
        Assert.True(VmPTicker978Slice.ShouldClearClientSideActorInterpolation(new object()));
        Assert.True(VmPTicker978Slice.ShouldTickAutomapOnLevel(VmPTicker978Slice.GsLevel));
    }

    [Fact]
    public void CutsceneBranch_AndMenuPause()
    {
        Assert.True(VmPTicker978Slice.ShouldRunCutsceneTicker(VmPTicker978Slice.GsCutscene));
        Assert.True(VmPTicker978Slice.MenuActivePausesTicker(menuActive: 1));
        Assert.False(VmPTicker978Slice.MenuActivePausesTicker(VmPTicker978Slice.MenuOnNoPause));
        Assert.True(VmPTicker978Slice.ConsoleStatePausesTicker(VmPTicker978Slice.ConsoleDown));
    }

    [Fact]
    public void CheckTickerPausedGate_MatchesNativePredicate()
    {
        Assert.True(VmPTicker978Slice.CheckTickerPausedGate(
            netGame: false,
            gameState: VmPTicker978Slice.GsLevel,
            menuPauses: true,
            consolePauses: false,
            demoPlayback: false,
            demoRecording: false,
            viewInitialized: true,
            wipeMatchesGameState: true));

        Assert.False(VmPTicker978Slice.CheckTickerPausedGate(
            netGame: true,
            VmPTicker978Slice.GsLevel,
            true,
            false,
            false,
            false,
            true,
            true));
    }

    [Fact]
    public void ClearLevelInterpolation_HeadPredicates()
    {
        Assert.True(VmPTicker978Slice.ShouldUpdateLevelInterpolatorBeforeActorClear(levelCount: 1));
        Assert.True(VmPTicker978Slice.PauseMusicWhenMenusPauseSound(
            VmPTicker978Slice.Level2PauseMusicInMenus));
    }
}
