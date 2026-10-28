namespace HCDE.Playsim.Tests;

public class VmPSetupLevel982SliceTests
{
    [Fact]
    public void SetupLevel_AudioAndMapGates()
    {
        Assert.True(VmPSetupLevel982Slice.ConsolePlayerResetsViewHeight(true, beforeSoundReset: true));
        Assert.True(VmPSetupLevel982Slice.ShouldStartHubMusic(primaryLevelReentering: false));
        Assert.True(VmPSetupLevel982Slice.MapOpenSucceeded(new object(), openAttempted: true));
        Assert.True(VmPSetupLevel982Slice.ShouldFireNewGameEvent(newGame: true, mapLoaded: true));
    }

    [Fact]
    public void PlayerSpawnPasses()
    {
        Assert.True(VmPSetupLevel982Slice.DeathmatchSpawnPassActive(deathmatch: true, playerInGame: true));
        Assert.True(VmPSetupLevel982Slice.RandomPlayerStartPassActive(
            VmPSetupLevel982Slice.Level2RandomPlayerStarts,
            playerInGame: true));
        Assert.True(VmPSetupLevel982Slice.NewGameSpawnFillNeeded(
            newGame: true,
            playerInGame: true,
            playerStateEnter: 1,
            mobMissing: true));
        Assert.Equal(VmPSetupLevel982Slice.SpfWeaponfullyup,
            VmPSetupLevel982Slice.SelectSpawnWeaponFlags(VmPSetupLevel982Slice.Level2Preraiseweapon));
    }

    [Fact]
    public void DeathmatchRelocationAndEndMonsters()
    {
        var oldMob = new object();
        var newMob = new object();
        Assert.True(VmPSetupLevel982Slice.UnfriendlyDeathmatchRelocationEnabled(
            deathmatchStartsCount: 1,
            playerInGame: true,
            mobPresent: true));
        Assert.True(VmPSetupLevel982Slice.ShouldDestroyPreviousSpawn(
            replacementSucceeded: true,
            newMob,
            oldMob));
        Assert.True(VmPSetupLevel982Slice.ShouldClearEndLevelMonsterCounters(
            VmPSetupLevel982Slice.Df2Nocountendmonst,
            actorCountsAsKill: true,
            sectorFlags: VmPSetupLevel982Slice.SecfEndgodmode | VmPSetupLevel982Slice.SecfEndlevel,
            sectorDamage: 1));
    }

    [Fact]
    public void PrecachePassAndMobClear()
    {
        Assert.True(VmPSetupLevel982Slice.ShouldRunPrecachePass(true, levelLoadFinished: true));
        Assert.True(VmPSetupLevel982Slice.TryClearMobBeforeSpawn(
            clearMob: true,
            () => { },
            out var cleared));
        Assert.True(cleared);
    }
}
