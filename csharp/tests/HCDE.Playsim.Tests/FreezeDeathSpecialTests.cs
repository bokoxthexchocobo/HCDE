using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FreezeDeathSpecialTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RemainingMonsterSpecialRunsOnceAndClearsEvenForHexen(bool hexen)
    {
        var sim = Room(hexen); var monster = sim.Actors.Single(actor => actor.IsMonster);
        ActorPropertyActions.SetSpecial(monster, 112, 7, 35); monster.ActivationType = 128 | 64;
        ActorFreezeActions.FreezeDeath(monster);
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, monster.Special);
        Assert.Equal(new[] { 7, 35, 0, 0, 0 }, monster.SpecialArgs);
        ActorSpecialActions.CallSpecial(monster, 112, 7, 99);
        ActorFreezeActions.FreezeDeath(monster); Assert.Equal(99, sim.LightOf(0));
    }

    [Fact]
    public void DirectCallUsesFrozenActorAsActivator()
    {
        var sim = Room(); var monster = sim.Actors.Single(actor => actor.IsMonster);
        ActorPropertyActions.SetSpecial(monster, 176, 0, 77); monster.ActivationType = 128;
        ActorFreezeActions.FreezeDeath(monster);
        Assert.Equal(77, monster.ThingId); Assert.Equal(0, monster.Special);
        Assert.Equal(0, sim.Players.Single().ThingId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PlayerAndNonMonsterSpecialsArePreserved(bool player)
    {
        var sim = Room(); var actor = player ? (Actor)sim.Players.Single() : sim.Actors.Single(a => a.IsMonster);
        actor.IsMonster = player;
        ActorPropertyActions.SetSpecial(actor, 112, 7, 35);
        ActorFreezeActions.FreezeDeath(actor);
        Assert.Equal(160, sim.LightOf(0)); Assert.Equal(112, actor.Special);
    }

    [Fact]
    public void SpecialReplacementIsClearedAfterCallAndSaveDoesNotReplayIt()
    {
        var sim = Room(); var monster = sim.Actors.Single(actor => actor.IsMonster);
        ActorPropertyActions.SetSpecial(monster, 127, 0, 112, 7, 35);
        ActorFreezeActions.FreezeDeath(monster);
        Assert.Equal(0, monster.Special); Assert.Equal(160, sim.LightOf(0));
        Assert.Equal(new[] { 7, 35, 0, 35, 0 }, monster.SpecialArgs);
        var saved = SimSavegame.Write(sim); var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(0, restored.Actors.Single(actor => actor.IsMonster).Special);
        Assert.Equal(160, restored.LightOf(0)); Assert.Equal(saved, SimSavegame.Write(restored));
    }

    private static AuthoritySimulation Room(bool hexen = false) => AuthoritySimulation.Start(new PlayLevel
    {
        HexenHack = hexen,
        Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, LightLevel = 160 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 96 }],
    }, rngSeed: 42);
}
