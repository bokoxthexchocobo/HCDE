using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupTossTests
{
    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }],
    }, rngSeed: seed);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(17)]
    [InlineData(-1)]
    public void DefaultTossUsesNativeVelocityFormulasAndFiveDraws(int seed)
    {
        var sim = Room(seed); var reference = Room(seed);
        var x = ((int)(reference.NextCombatRandom() & 255) - (int)(reference.NextCombatRandom() & 255)) / 256.0;
        var y = ((int)(reference.NextCombatRandom() & 255) - (int)(reference.NextCombatRandom() & 255)) / 256.0;
        var z = 5 + (reference.NextCombatRandom() & 255) / 64.0;
        var source = sim.Players.Single();
        source.VelocityX = Fixed.FromInt(20); source.VelocityZ = Fixed.FromInt(-30);
        Assert.True(sim.SpawnDroppedPickup(source, PickupCatalog.Clip));
        var drop = Assert.Single(sim.Actors, actor => actor.DoomEdNum == PickupCatalog.Clip);
        Assert.Equal(Fixed.FromDouble(x), drop.VelocityX);
        Assert.Equal(Fixed.FromDouble(y), drop.VelocityY);
        Assert.Equal(Fixed.FromDouble(z), drop.VelocityZ);
        Assert.InRange(drop.VelocityX.ToDouble(), -255 / 256.0, 255 / 256.0);
        Assert.InRange(drop.VelocityZ.ToDouble(), 5, 5 + 255 / 64.0);
        Assert.Equal(reference.CombatRandomState, sim.CombatRandomState);
    }

    [Fact]
    public void InvalidPickupDoesNotConsumeTossRandomness()
    {
        var sim = Room(0); var before = sim.CombatRandomState;
        Assert.False(sim.SpawnDroppedPickup(sim.Players.Single(), -1));
        Assert.Equal(before, sim.CombatRandomState);
    }
}
