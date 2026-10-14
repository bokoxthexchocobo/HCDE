using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombatBehaviorSaveTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(7, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(7, true)]
    public void FlagCombinationsSurviveIdentityReconstruction(int flags, bool reconstruct)
    {
        var sim = Room(); var actor = Monster(sim); Set(actor, flags); var saved = SimSavegame.Write(sim, true);
        if (reconstruct) { actor.Destroy(); sim.Tick(); } else Set(actor, flags ^ 7);
        SimSavegame.Apply(sim, saved); actor = Monster(sim);
        Assert.Equal(flags, Flags(actor)); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredDontDrainControlsHealingButNotDamage(bool dontDrain)
    {
        var sim = Room(); var target = Monster(sim); var player = sim.Players.Single(); player.Health = 50; player.DrainStrength = 1;
        target.DontDrain = dontDrain; var saved = SimSavegame.Write(sim); target.DontDrain = !dontDrain;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(10, ActorDamage.Apply(target, 10, player).HealthLost);
        Assert.Equal(dontDrain ? 50 : 60, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredHarmFriendsControlsFriendlyDamage(bool harmFriends)
    {
        var sim = Room(); var source = Monster(sim); var target = sim.AddBot(300, 0, 3004);
        source.Friendly = target.Friendly = true; source.HarmFriends = harmFriends; sim.Infighting = 0;
        var saved = SimSavegame.Write(sim); source.HarmFriends = !harmFriends; SimSavegame.Apply(sim, saved);
        Assert.Equal(harmFriends ? 10 : 0, ActorDamage.Apply(target, 10, source, inflictor: source).HealthLost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoredNoAutoOffControlsStationaryCharge(bool noAutoOff)
    {
        var sim = Room(); var soul = Monster(sim); soul.NoAutoOffSkullFly = noAutoOff;
        soul.Brain!.StartCharge(soul, sim.Players.Single()); soul.VelocityX = soul.VelocityY = soul.VelocityZ = default;
        var saved = SimSavegame.Write(sim); soul.NoAutoOffSkullFly = !noAutoOff; SimSavegame.Apply(sim, saved);
        sim.Tick(); Assert.Equal(noAutoOff, soul.Brain.Charging);
    }

    [Fact]
    public void OlderSnapshotClearsLaterFlags()
    {
        var sim = Room(); var actor = Monster(sim); var saved = SimSavegame.Write(sim);
        Set(actor, 7); SimSavegame.Apply(sim, saved); Assert.Equal(0, Flags(actor));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void InvalidStructuredFlagsRejectBeforeMutation(int flags)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].CombatBehaviorFlags = flags;
        var checksum = sim.Checksum;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(checksum, sim.Checksum);
    }

    [Theory]
    [InlineData(0, 240, "save-combatbehavior-header")]
    [InlineData(8, 8, "save-combatbehavior-value")]
    public void MalformedArchiveIsRejected(int offset, int value, string expected)
    {
        var sim = Room(); Set(Monster(sim), 7); var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 240);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal(expected, error);
    }

    private static int Flags(Actor actor) => (actor.HarmFriends ? 1 : 0) | (actor.DontDrain ? 2 : 0) | (actor.NoAutoOffSkullFly ? 4 : 0);
    private static void Set(Actor actor, int flags)
    {
        actor.HarmFriends = (flags & 1) != 0; actor.DontDrain = (flags & 2) != 0; actor.NoAutoOffSkullFly = (flags & 4) != 0;
    }
    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(a => a.DoomEdNum == 3006 && !a.Destroyed);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3006 }]
    });
}
