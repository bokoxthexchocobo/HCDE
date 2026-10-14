using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UdmfActorSpawnHealthTests
{
    [Theory]
    [InlineData(1, 20)]
    [InlineData(2.5, 50)]
    [InlineData(-37, 37)]
    public void UdmfHealthReachesActorAndRevival(double health, int expected)
    {
        var text = FormattableString.Invariant($"namespace=\"ZDoom\"; sector {{ heightceiling=128; }} thing {{ type=3004; health={health}; skill3=true; single=true; }}");
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(expected, actor.Health); Assert.Equal(expected, actor.ResurrectionHealth);
        Assert.Equal(expected, actor.SpawnHealth());
    }

    [Fact]
    public void ZeroHealthDefersDeathUntilFirstTick()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004, Health = 0 }] });
        var actor = sim.Actors.Single(); Assert.Equal(0, actor.Health); Assert.Equal(0, actor.DeathCount);
        sim.Tick(); Assert.Equal(1, actor.DeathCount); Assert.True(actor.Killed);
    }
}
