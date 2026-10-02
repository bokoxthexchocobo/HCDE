using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceShatterDelayTests
{
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, 1)]
    public void UnforcedMovingCorpseDelaysWithoutSpawningOrConsumingRandom(int vx, int vy, int vz)
    {
        var sim = Room(); var corpse = sim.AddBot(100, 0);
        corpse.VelocityX = Fixed.FromInt(vx); corpse.VelocityY = Fixed.FromInt(vy); corpse.VelocityZ = Fixed.FromInt(vz);
        var random = sim.CombatRandomState; var count = sim.Actors.Count; var frame = corpse.States.Current;
        sim.SpawnIceChunks(corpse);
        Assert.Equal(105, corpse.States.RemainingTics); Assert.Equal(frame, corpse.States.Current);
        Assert.Equal(vx, corpse.VelocityX.ToDouble()); Assert.Equal(vy, corpse.VelocityY.ToDouble());
        Assert.Equal(vz, corpse.VelocityZ.ToDouble()); Assert.Equal(random, sim.CombatRandomState);
        Assert.Equal(count, sim.Actors.Count); Assert.Empty(sim.Actors.OfType<IceChunkActor>());
        corpse.Shattering = true; sim.SpawnIceChunks(corpse);
        Assert.NotEmpty(sim.Actors.OfType<IceChunkActor>());
        Assert.Equal(0, corpse.VelocityX.Raw); Assert.Equal(0, corpse.VelocityY.Raw); Assert.Equal(0, corpse.VelocityZ.Raw);
    }

    [Fact]
    public void StationaryUnforcedCorpseSpawnsImmediately()
    {
        var sim = Room(); var corpse = sim.AddBot(100, 0);
        Assert.False(corpse.Shattering); sim.SpawnIceChunks(corpse);
        Assert.NotEmpty(sim.Actors.OfType<IceChunkActor>());
        Assert.NotEqual(105, corpse.States.RemainingTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
