using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceShatterCorpseRemovalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CompletedShatterRemovesCorpseAndCannotSpawnDuplicateDebris(bool forced)
    {
        var sim = Room(); var corpse = sim.AddBot(100, 0); corpse.Shattering = forced;
        if (forced) corpse.VelocityX = Fixed.FromInt(2);
        sim.SpawnIceChunks(corpse);
        Assert.True(corpse.Destroyed); Assert.False(corpse.Solid); Assert.False(corpse.Shootable);
        Assert.Equal(-1, corpse.States.Current);
        var count = sim.Actors.OfType<IceChunkActor>().Count(); Assert.True(count >= 25);
        var random = sim.CombatRandomState;
        sim.SpawnIceChunks(corpse); ActorDamage.Apply(corpse, 7, damageType: "Fire");
        Assert.Equal(count, sim.Actors.OfType<IceChunkActor>().Count()); Assert.Equal(random, sim.CombatRandomState);
        sim.Tick(); Assert.DoesNotContain(corpse, sim.Actors);
        Assert.Equal(count, sim.Actors.OfType<IceChunkActor>().Count());
    }

    [Fact]
    public void DelayedMovingCorpseRemainsPresentAndBlocking()
    {
        var sim = Room(); var corpse = sim.AddBot(100, 0); corpse.VelocityX = Fixed.FromInt(2);
        sim.SpawnIceChunks(corpse);
        Assert.False(corpse.Destroyed); Assert.True(corpse.Solid); Assert.True(corpse.Shootable);
        Assert.Contains(corpse, sim.Actors); Assert.Empty(sim.Actors.OfType<IceChunkActor>());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
