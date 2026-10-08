using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeDeathHeightTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(229376)]
    [InlineData(-65536)]
    public void PatchedDefaultHeightRestoresWithoutChangingRuntimeRadius(int heightRaw)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nHeight = {heightRaw}\n");
        var sim = Room(patch); var monster = sim.Actors.Single(actor => actor.IsMonster);
        Assert.Equal(heightRaw, monster.Height.Raw);
        ActorPropertyActions.SetSize(monster, 9.5, 2.25);
        monster.States.Configure(monster, [new ActorFrame(1, -1, ActorFreezeActions.FreezeDeath)], 0);
        Assert.Equal(heightRaw, monster.Height.Raw); Assert.Equal(Fixed.FromDouble(9.5), monster.Radius);
        var saved = SimSavegame.Write(sim); var restored = Room(patch);
        SimSavegame.Apply(restored, saved);
        var loaded = restored.Actors.Single(actor => actor.IsMonster);
        Assert.Equal(monster.Height, loaded.Height); Assert.Equal(monster.Radius, loaded.Radius);
        Assert.Equal(monster.States.RemainingTics, loaded.States.RemainingTics);
        Assert.Equal(saved, SimSavegame.Write(restored));
    }

    [Fact]
    public void PlayerFreezeUsesStandingDefaultWithoutChangingStoredFullHeight()
    {
        var sim = Room(); var player = sim.Players.Single(); var spawnHeight = player.Height;
        ActorPropertyActions.SetSize(player, 12, 80);
        player.Height = Fixed.FromInt(20);
        ActorFreezeActions.FreezeDeath(player);
        Assert.Equal(spawnHeight, player.Height); Assert.Equal(80, player.FullHeight);
        Assert.Equal(Fixed.FromInt(12), player.Radius);
    }

    [Fact]
    public void HeightIsRestoredBeforeMonsterSpecialRuns()
    {
        var sim = Room();
        var monster = new HeightObserver { Simulation = sim, IsMonster = true, ResurrectionHeight = Fixed.FromInt(56) };
        var defaultHeight = monster.ResurrectionHeight.Value;
        monster.Height = Fixed.FromInt(2);
        ActorPropertyActions.SetSpecial(monster, 130, 0);
        ActorFreezeActions.FreezeDeath(monster);
        Assert.Equal(defaultHeight, monster.Height); Assert.Equal(0, monster.Special);
        Assert.True(monster.Observed);
    }

    private sealed class HeightObserver : Actor
    {
        internal bool Observed { get; private set; }
        public override void Activate(Actor? activator)
        {
            Assert.Equal(ResurrectionHeight!.Value, Height);
            Observed = true;
        }
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch = null) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 96 }],
    }, dehacked: patch, rngSeed: 42);
}
