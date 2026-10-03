using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedCeilingSpawnTests
{
    [Theory]
    [InlineData(false, 0, 16)]
    [InlineData(false, 8, 24)]
    [InlineData(true, 0, 72)]
    [InlineData(true, 8, 64)]
    public void CeilingFlagChangesSpawnOriginAndOffsetDirection(bool ceiling, double offset, double expectedZ)
    {
        var sim = Room(ceiling, offset);
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(ceiling, actor.SpawnCeiling);
        Assert.Equal(expectedZ, actor.Z.ToDouble());
        Assert.Equal(!ceiling && offset == 0, actor.OnGround);
    }

    [Fact]
    public void CeilingPlacementDoesNotAutomaticallyDisableGravity()
    {
        var sim = Room(true, 0);
        var actor = Assert.Single(sim.Actors);
        actor.Brain!.Enabled = false;
        Assert.False(actor.NoGravity);
        sim.Tick();
        Assert.True(actor.Z.ToDouble() < 72);
    }

    [Fact]
    public void CeilingFlagAloneAffectsChecksum()
    {
        var left = Room(false, 0); var right = Room(false, 0);
        Assert.Single(left.Actors).SpawnCeiling = true;
        left.Tick(); right.Tick();
        Assert.NotEqual(left.Checksum, right.Checksum);
        Assert.Single(right.Actors).SpawnCeiling = true;
        left.Tick(); right.Tick();
        Assert.Equal(left.Checksum, right.Checksum);
    }

    private static AuthoritySimulation Room(bool ceiling, double offset) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { FloorHeight = 16, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, Z = offset }],
    }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {(ceiling ? 262 : 6)}\n"));
}
