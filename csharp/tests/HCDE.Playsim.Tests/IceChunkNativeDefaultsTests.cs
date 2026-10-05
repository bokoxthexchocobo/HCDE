using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkNativeDefaultsTests
{
    [Fact]
    public void NativeDefaultsAreVisibleToScripts()
    {
        var chunk = new IceChunkActor(70);
        Assert.True(AcsActorFlags.TryGet(chunk, "CANNOTPUSH", out var cannotPush));
        Assert.True(cannotPush);
        Assert.True(AcsActorFlags.TryGet(chunk, "NOTELEPORT", out var noTeleport));
        Assert.True(noTeleport);
    }

    [Fact]
    public void TeleportRejectsIceDebrisWithoutChangingMotion()
    {
        var sim = Room();
        var chunk = new IceChunkActor(70) { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(32), VelocityX = Fixed.FromInt(3) };
        Assert.False(LineSpecials.Execute(sim, chunk, LineSpecials.Teleport, 0));
        Assert.Equal(32, chunk.X.ToDouble());
        Assert.Equal(3, chunk.VelocityX.ToDouble());
        Assert.Equal(70, chunk.RemainingTics);
    }

    [Fact]
    public void FreezeDeathSpawnAppliesDefaultsToEveryChunk()
    {
        var sim = Room(); var corpse = sim.AddBot(64, 0);
        sim.SpawnIceChunks(corpse);
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray();
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk =>
        {
            Assert.True(chunk.NoTeleport);
            Assert.True(chunk.CannotPush);
            Assert.False(LineSpecials.Execute(sim, chunk, LineSpecials.Teleport, 0));
        });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
