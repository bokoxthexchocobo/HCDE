using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HateTargetAcquisitionTests
{
    [Fact]
    public void HatedTidIsAcquiredBeforeVisiblePlayer()
    {
        var sim = Room(); var hunter = Hunter(sim); hunter.TidToHate = 7;
        hunter.Brain!.Tick(sim, hunter); Assert.Equal(Hatee(sim).Id, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HuntPlayersControlsFallbackWhenNoHateeExists(bool hunt)
    {
        var sim = Room(); var hunter = Hunter(sim); hunter.TidToHate = 999; hunter.HuntPlayers = hunt;
        hunter.Brain!.Tick(sim, hunter);
        Assert.Equal(hunt ? sim.Players.Single().Id : (uint?)null, hunter.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoSightCheckBypassesHateeAcquisitionView(bool noSight)
    {
        var sim = Room(); var hunter = Hunter(sim); Hatee(sim).X = Fixed.FromInt(-200);
        hunter.TidToHate = 7; hunter.NoSightCheck = noSight;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.DontLookAllAround);
        Assert.Equal(noSight ? Hatee(sim).Id : (uint?)null, hunter.TargetId);
    }

    [Fact]
    public void DormantHateeIsSkippedEvenWithoutSightChecks()
    {
        var sim = Room(); var hunter = Hunter(sim); hunter.TidToHate = 7; hunter.NoSightCheck = true;
        Hatee(sim).Dormant = true; hunter.Brain!.Tick(sim, hunter); Assert.Null(hunter.TargetId);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void TargetingFlagsSurviveIdentityReconstruction(bool noSight, bool hunt)
    {
        var sim = Room(); var hunter = Hunter(sim); hunter.NoSightCheck = noSight; hunter.HuntPlayers = hunt;
        var saved = SimSavegame.Write(sim, true); hunter.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        hunter = Hunter(sim); Assert.Equal(noSight, hunter.NoSightCheck); Assert.Equal(hunt, hunter.HuntPlayers);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData("NOSIGHTCHECK")]
    [InlineData("Actor.HuntPlayers")]
    public void LegacyScriptFlagCanBeSetReadAndCleared(string name)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, name, false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out value)); Assert.False(value);
    }

    [Fact]
    public void FriendshipCopyAndReviveApplyTargetingFlags()
    {
        var sim = Room(); var hunter = Hunter(sim); var source = Hatee(sim);
        source.NoSightCheck = source.HuntPlayers = true; hunter.MasterId = source.Id;
        ActorPropertyActions.CopyFriendliness(hunter); Assert.True(hunter.NoSightCheck); Assert.True(hunter.HuntPlayers);
        ActorRaise.ReviveSupported(hunter); Assert.False(hunter.NoSightCheck); Assert.False(hunter.HuntPlayers);
    }

    [Fact]
    public void InvalidStructuredTargetingBitsRejectWriteAndRestore()
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].HateTargetingFlags = 4;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    private static Actor Hunter(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3004);
    private static Actor Hatee(AuthoritySimulation sim) => sim.Actors.Single(a => a.ThingId == 7);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004 },
            new LevelThing { Type = 3001, Id = 7, X = 200 }]
    });
}
