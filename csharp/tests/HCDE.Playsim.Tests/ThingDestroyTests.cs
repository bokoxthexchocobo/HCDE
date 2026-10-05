using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThingDestroyTests
{
    [Theory]
    [InlineData(7, 0, 0, true)]
    [InlineData(7, 1, 3, true)]
    [InlineData(0, 1, 3, true)]
    [InlineData(7, 1, 99, false)]
    public void TargetedDestroyFiltersIdAndSector(int tid, int extreme, int tag, bool killed)
    {
        var sim = Room(); var monster = sim.Actors[1];
        Assert.True(ThingDamage.Execute(sim, 133, sim.Players.Single(), tid, extreme, tag));
        Assert.Equal(killed, monster.IsDead);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void ExtremeDamageBypassesInvulnerability(int extreme, bool killed)
    {
        var sim = Room(); sim.Actors[1].Invulnerable = true;
        Assert.True(ThingDamage.Execute(sim, 133, null, 7, extreme, 0));
        Assert.Equal(killed, sim.Actors[1].IsDead);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 3, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, Id = 7, X = 100 }],
    });
}
