using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkMovementAuditTests
{
    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    [InlineData(2)]
    public void IceChunkMovesOncePerTickAndRemembersOriginalPosition(int horizontalSpeed)
    {
        var sim = Room();
        var chunk = new IceChunkActor(10) { Simulation = sim, Level = sim.Level, X = Fixed.FromInt(64),
            Z = Fixed.FromInt(100), VelocityX = Fixed.FromInt(horizontalSpeed), VelocityZ = Fixed.FromInt(4),
            NoGravity = true, OnGround = false };
        chunk.Tick();
        Assert.Equal(64 + horizontalSpeed, chunk.X.ToDouble()); Assert.Equal(104, chunk.Z.ToDouble());
        Assert.Equal(64, chunk.PreviousX.ToDouble()); Assert.Equal(100, chunk.PreviousZ.ToDouble());
        Assert.Equal(9, chunk.RemainingTics);
    }

    [Fact]
    public void ExpiringChunkDoesNotApplyMovement()
    {
        var sim = Room(); var chunk = new IceChunkActor(1) { Simulation = sim, Level = sim.Level,
            Z = Fixed.FromInt(100), VelocityZ = Fixed.FromInt(4) };
        chunk.States.Restore(3, 1);
        chunk.Tick(); Assert.True(chunk.Destroyed); Assert.Equal(100, chunk.Z.ToDouble());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void SpawnedIceChunksRetainElevatedHeightAndAreAbsentFromTraceBlockmap(int mask)
    {
        var sim = Room(); var source = sim.Players.Single(); var corpse = sim.AddBot(64, 0);
        corpse.Z = Fixed.FromInt(100); sim.SpawnIceChunks(corpse); corpse.Destroy();
        foreach (var pickup in sim.Actors.Where(actor => PickupCatalog.IsPickup(actor.DoomEdNum)))
            pickup.Y = Fixed.FromInt(128);
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.NotEmpty(chunks);
        foreach (var chunk in chunks)
        {
            Assert.InRange(chunk.Z.ToDouble(), 100, 156); Assert.Equal(chunk.Z, chunk.PreviousZ);
            Assert.False(chunk.OnGround); Assert.False(chunk.IsBlockmapActor);
            chunk.X = Fixed.FromInt(32); chunk.Y = Fixed.FromInt(0); chunk.Z = Fixed.FromInt(28);
            chunk.Solid = true;
        }
        var target = sim.AddBot(100, 0);
        Assert.Same(target, CombatTrace.PickActor(sim, source, source.Angle, new BamAngle(0), 256, mask));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
