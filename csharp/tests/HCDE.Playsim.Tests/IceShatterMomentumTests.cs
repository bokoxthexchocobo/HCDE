using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceShatterMomentumTests
{
    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-4, 5, -6)]
    [InlineData(0, 0, 8)]
    public void ShatteringStopsCorpseWithoutTransferringMomentumToDebris(int vx, int vy, int vz)
    {
        var moving = Room(); var stationary = Room();
        var corpse = moving.AddBot(100, 0); var comparison = stationary.AddBot(100, 0);
        corpse.Z = comparison.Z = Fixed.FromInt(80);
        corpse.Shattering = comparison.Shattering = true;
        corpse.VelocityX = Fixed.FromInt(vx); corpse.VelocityY = Fixed.FromInt(vy); corpse.VelocityZ = Fixed.FromInt(vz);
        moving.SpawnIceChunks(corpse); stationary.SpawnIceChunks(comparison);
        Assert.Equal(0, corpse.VelocityX.Raw); Assert.Equal(0, corpse.VelocityY.Raw); Assert.Equal(0, corpse.VelocityZ.Raw);
        Assert.Equal(100, corpse.X.ToDouble()); Assert.Equal(80, corpse.Z.ToDouble());
        Assert.Contains(moving.Actors.OfType<IceChunkActor>(), chunk => chunk.VelocityZ.Raw > 0);
        Assert.Equal(stationary.CombatRandomState, moving.CombatRandomState);
        Assert.Equal(stationary.Checksum, moving.Checksum);
        moving.Tick(); stationary.Tick(); Assert.Equal(stationary.Checksum, moving.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
